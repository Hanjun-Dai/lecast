using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;

namespace Lecast_P2
{
    /// <summary>
    /// 单词网络中每个节点
    /// </summary>
    class WordNode 
    {
        public HashSet<String> pre, next;                      //每个单词的前驱后继集合
        public int cnt, groupID;                               //单词出现次数、所在网络编号
        public HashSet<String> neighbours;                     //单词的所有可能的替换词
        public string correctWord;                             //该单词对应的正确的单词

        public WordNode(string name) 
        {
            pre = new HashSet<string>();
            next = new HashSet<string>();
            neighbours = new HashSet<string>();
            correctWord = name;
            cnt = 0;
        }

        public void Tick() 
        {
            cnt++;
        }

        public void TryAddNeighbour(string word) 
        {
            if (neighbours.Contains(word)) return;
            neighbours.Add(word);
        }

        public void TryAddPre(string word) 
        {
            if (pre.Contains(word)) return;
            pre.Add(word);
        }

        public void TryAddNext(string word) 
        {
            if (next.Contains(word)) return;
            next.Add(word);
        }

        public static int CountOverlap(HashSet<string> a, HashSet<string> b) 
        {
            int cnt = 0;
            foreach (var word in a)
                if (b.Contains(word)) cnt++;
            return cnt;
        }
    }

    /// <summary>
    /// 单词串预处理类
    /// </summary>
    class WordProcessor
    {
        public const int MAX_EDIT_DIST = 2;                                //作为可替换单词的最大编辑距离
        public const int MIN_DELTA = 10;                                   //发现错误拼写的最小频数差别
        public static Dictionary<string, WordNode> globalDict;             //全局单词网络（不考虑单个查询)
        public static Dictionary<string, Dictionary<string, WordNode>> skuKeywords; //每个产品的keyword集合
        public static Dictionary<string, Dictionary<int, int>> skuVersion;    //每个产品的版本号
        public static Dictionary<string, int> skuMaxWordCnt;               //每个产品keyword中出现次数最多的单词的出现次数

        #region Spell check
        static void OutputConnections(Dictionary<string, WordNode> dict, string filename, string sku) 
        {
            string[] words = dict.Keys.ToArray();
            for (int i = 0; i < words.Length - 1; ++i)
                if (words[i].Length >= Tools.MIN_WORD_LENGTH)
                {
                    for (int j = i + 1; j < words.Length; ++j)
                    {
                        if (words[j].Length < Tools.MIN_WORD_LENGTH) continue;
                        if (Tools.ISEditDistOK(words[i], words[j]))
                        {
                            WordNode x = GetOrAddNode(dict, words[i]), y = GetOrAddNode(dict, words[j]);
                            int t = Tools.GetWordSim(x, y);
                            File.AppendAllLines(filename, new string[] { sku + " " + words[i] + " " + words[j] + " " + t + " " + x.cnt + " " + y.cnt });
                        }
                    }
                }
        }

        public static void FindConnections()
        {
            OutputConnections(globalDict, "result_global.txt", "");
            foreach (var sku in skuKeywords.Keys)
            {
                OutputConnections(skuKeywords[sku], "result_local.txt", sku);
            }
        }

        static Tuple<string, int> FindCorrectWord(string word, int groupID, HashSet<string> visited, Dictionary<string, WordNode> dict) 
        {
            string best = word;
            int maxCnt = dict[word].cnt;
            dict[word].groupID = groupID;
            foreach (var key in dict[word].neighbours)
                if (!visited.Contains(key))
                {
                    visited.Add(key);
                    Tuple<string, int> result = FindCorrectWord(key, groupID, visited, dict);
                    if (result.Item2 > maxCnt) 
                    {
                        maxCnt = result.Item2; best = result.Item1;
                    }
                }
            return new Tuple<string, int>(best, maxCnt);
        }

        static void TryCorrectWords(Dictionary<string, WordNode> dict) 
        {
            HashSet<string> visited = new HashSet<string>();
            Dictionary<int, Tuple<string, int>> result = new Dictionary<int, Tuple<string, int>>();
            int curID = 0;
            foreach (var key in dict.Keys)
                if (!visited.Contains(key))
                {
                    var word = FindCorrectWord(key, curID, visited, dict);
                    result.Add(curID, word);
                    dict[key].correctWord = word.Item1;
                    curID++;
                }
                else 
                {
                    var word = result[dict[key].groupID];
                    dict[key].correctWord = word.Item1;
                }
        }

        static void TryAddRelation(Dictionary<string, WordNode> dict, string a, string b)
        {
            WordNode x = GetOrAddNode(dict, a), y = GetOrAddNode(dict, b);
            x.TryAddNeighbour(b);
            y.TryAddNeighbour(a);
        }

        static void BuildEquivalenceGraph(Tuple<string, string, int, int, int>[] relations, Dictionary<string, WordNode> dict)
        {
            for (int i = 0; i < relations.Length; ++i)
            {
                if (relations[i].Item3 > 0 || Math.Abs(relations[i].Item4 - relations[i].Item5) > MIN_DELTA)
                    TryAddRelation(dict, relations[i].Item1, relations[i].Item2);
            }
        }

        /// <summary>
        /// 建立全局的单词等价网络
        /// </summary>
        public static void BuildGlobalEquGraph()
        {
            string[] a = File.ReadAllLines("result_global.txt", Encoding.Default);
            Tuple<string, string, int, int, int>[] relations = new Tuple<string, string, int, int, int>[a.Length];
            for (int i = 0; i < a.Length; ++i)
            {
                string[] words = a[i].Trim().Split(' ');
                relations[i] = new Tuple<string, string, int, int, int>(words[0], words[1], int.Parse(words[2]), int.Parse(words[3]), int.Parse(words[4]));
            }
            BuildEquivalenceGraph(relations, globalDict);

            TryCorrectWords(globalDict);
        }

        /// <summary>
        /// 对于每个产品单独建立单词等价网络
        /// </summary>
        public static void BuildLocalEquGraph()
        {
            string[] a = File.ReadAllLines("result_local.txt", Encoding.Default);
            string pre = "hello world";
            List<Tuple<string, string, int, int, int>> list = new List<Tuple<string, string, int, int, int>>();
            for (int i = 0; i < a.Length; ++i)
            {
                string[] words = a[i].Trim().Split(' ');
                if (words[0] != pre)
                {
                    if (list.Count > 0)
                    {
                        BuildEquivalenceGraph(list.ToArray(), GetOrAddDict(pre));
                        TryCorrectWords(GetOrAddDict(pre));
                    }
                    pre = words[0];
                    list.Clear();
                }
                list.Add(new Tuple<string, string, int, int, int>(words[1], words[2], int.Parse(words[3]), int.Parse(words[4]), int.Parse(words[5])));
            }
            if (list.Count > 0)
            {
                BuildEquivalenceGraph(list.ToArray(), GetOrAddDict(pre));
                TryCorrectWords(GetOrAddDict(pre)); 
            }
        }
        #endregion

        static Dictionary<string, WordNode> GetOrAddDict(string sku) 
        {
            Dictionary<string, WordNode> dict = null;
            if (skuKeywords.ContainsKey(sku))
                dict = skuKeywords[sku];
            else
            {
                dict = new Dictionary<string, WordNode>();
                skuKeywords.Add(sku, dict);
            }
            return dict;
        }

        static WordNode GetOrAddNode(Dictionary<string, WordNode> dict, string word) 
        {
            WordNode node = null;
            if (dict.ContainsKey(word))
                return dict[word];
            else 
            {
                node = new WordNode(word);
                dict.Add(word, node);
            }
            return node;
        }

        /// <summary>
        /// 给产品添加其版本号
        /// </summary>
        /// <param name="sku">产品id</param>
        /// <param name="v">版本号</param>
        static void AddVersion(string sku, int v) 
        {
            Dictionary<int, int> dict = null;
            if (skuVersion.ContainsKey(sku))
                dict = skuVersion[sku];
            else
            {
                dict = new Dictionary<int, int>();
                skuVersion.Add(sku, dict);
            }
            if (dict.ContainsKey(v))
                dict[v]++;
            else dict.Add(v, 1);
        }

        /// <summary>
        /// 添加查询词到对应产品
        /// </summary>
        /// <param name="sku">产品id</param>
        /// <param name="query">查询语句</param>
        static void AddQueryWords(string sku, string query) 
        {
            string[] words = Tools.CleanWords(query);
            if (words == null || words.Length == 0) return;

            var dict = GetOrAddDict(sku);

            for (int i = 0; i < words.Length; ++i) 
            {
                if (string.IsNullOrEmpty(words[i])) continue;   
                var node = GetOrAddNode(dict, words[i]);
                node.Tick();
                if (i > 0)
                    node.TryAddPre(words[i - 1]);
                if (i < words.Length - 1)
                    node.TryAddNext(words[i + 1]);

                int v = Tools.TryGetInt(words[i]);
                if (v > 0 && v < 20)
                    AddVersion(sku, v);

                node = GetOrAddNode(globalDict, words[i]);
                node.Tick();
                if (i > 0)
                    node.TryAddPre(words[i - 1]);
                if (i < words.Length - 1)
                    node.TryAddNext(words[i + 1]);
            }
        }

        /// <summary>
        /// 建立单词等价网络
        /// </summary>
        /// <param name="tableName"></param>
        public static void BuildWordGraph(string tableName) 
        {
            skuKeywords = new Dictionary<string, Dictionary<string, WordNode>>();
            skuVersion = new Dictionary<string, Dictionary<int, int>>();
            globalDict = new Dictionary<string, WordNode>();

            var conn = DBManager.GetConnection(tableName, false);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select sku, query from " + tableName;
            var reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                string sku = reader.GetString(0);
                string query = reader.GetString(1);
                AddQueryWords(sku, query);
            }

            skuMaxWordCnt = new Dictionary<string, int>();
            foreach (var sku in skuKeywords.Keys) 
            {
                var dict = skuKeywords[sku];
                int maxCnt = 0;
                foreach (var word in dict.Keys)
                    maxCnt = Math.Max(maxCnt, dict[word].cnt);
                skuMaxWordCnt.Add(sku, maxCnt);
            }

            reader.Close();
            conn.Close();
        }

        /// <summary>
        /// 尝试更正一个单词
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">待更正单词</param>
        /// <returns>更正后的单词</returns>
        static string FindMostSimWord(Dictionary<string, WordNode> dict, string key) 
        {
            if (key.Length < Tools.MIN_WORD_LENGTH) return key;
            string[] words = dict.Keys.ToArray();
            string ans = key;
            int maxcnt = 0;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].Length >= Tools.MIN_WORD_LENGTH) 
                {
                    if (Tools.ISEditDistOK(words[i], key))
                        if (dict[words[i]].cnt > maxcnt)
                        {
                            ans = words[i]; maxcnt = dict[words[i]].cnt;
                        }
                }
            return ans;
        }

        /// <summary>
        /// 清洗、纠正用户查询
        /// </summary>
        /// <param name="sku">产品id</param>
        /// <param name="query">用户查询</param>
        /// <returns>单词序列</returns>
        public static string[] GetCorrectQuery(string sku, string query)
        {
            string[] words = Tools.CleanWords(query);

            if (WordProcessor.skuKeywords.ContainsKey(sku))
            {
                var dict = WordProcessor.skuKeywords[sku];
                for (int i = 0; i < words.Length; ++i)
                {
                    if (dict.ContainsKey(words[i]))
                    {
                        var node = dict[words[i]];
                        words[i] = node.correctWord;
                    }
                    else
                    {
                        string pre = words[i];
                        words[i] = FindMostSimWord(dict, words[i]);
                        if (pre == words[i] && WordProcessor.globalDict.ContainsKey(words[i]))
                            words[i] = WordProcessor.globalDict[words[i]].correctWord;
                    }
                }
            }

            return words;
        }
    }
}
