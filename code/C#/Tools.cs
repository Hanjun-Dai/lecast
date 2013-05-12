using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lecast_P2
{
    class Tools
    {
        static char[] spliteChars = {' ', '_', '-', '/', '+'};
        public static bool IsLetter(char ch) 
        {
            return (ch >= 'a' && ch <= 'z');
        }

        public static bool IsDigit(char ch) 
        {
            return (ch >= '0' && ch <= '9');
        }

        /// <summary>
        /// 去除特殊字符
        /// </summary>
        /// <param name="word">待清洗的单词</param>
        /// <returns>清洗后的单词</returns>
        public static string RemoveSpecialChar(string word) 
        {
            for (int i = 0; i < word.Length; ++i)
            {
                if (IsDigit(word[i]) || IsLetter(word[i]))
                    continue;
                word = word.Remove(i, 1); 
                i--;
            }
            return word;
        }

        /// <summary>
        /// 清洗查询
        /// </summary>
        /// <param name="query">用户输入的查询</param>
        /// <returns>清洗后的查询词序列</returns>
        public static string[] CleanWords(string query) 
        {
            string[] buf = query.ToLower().Trim().Split(spliteChars);
            List<string> words = new List<string>(buf);
            for (int i = 0; i < words.Count; ++i)
                words[i] = RemoveSpecialChar(words[i]);
            for (int i = 0; i < words.Count; ++i)
                if (words[i].Length > 0 && IsLetter(words[i][0]) && IsDigit(words[i][words[i].Length - 1])) 
                {
                    if (words[i].Contains("2k")) continue;
                    Tuple<string, string> tuple = TrySplitNum(words[i]);
                    words[i] = tuple.Item1;
                    words.Insert(i + 1, tuple.Item2);
                }

            return words.ToArray();
        }

        /// <summary>
        /// 尝试把尾部包含数字的单词分离
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static Tuple<string, string> TrySplitNum(string word) 
        {
            string w = "";
            while (true) 
            {
                if (IsLetter(word[0]))
                {
                    w = w + word[0];
                    word = word.Remove(0, 1);
                }
                else break;
            }
            return new Tuple<string, string>(w, word);
        }

        /// <summary>
        /// 尝试把单词转换为阿拉伯数字
        /// </summary>
        /// <param name="word">待转换的单词</param>
        /// <returns>转换后的阿拉伯数字</returns>
        public static int TryGetInt(string word) 
        {
            int result = -1;
            if (int.TryParse(word, out result))
                return result;
            return TryConvertRome(word);
        }

        /// <summary>
        /// 尝试把罗马数字转换为阿拉伯数字
        /// </summary>
        /// <param name="word">可能为罗马数字的单词</param>
        /// <returns>转换后的结果</returns>
        public static int TryConvertRome(string word) 
        {
            word = word.ToUpper();
            switch (word) 
            {
                case "II": return 2;
                case "III": return 3;
                case "IV" : return 4;
                case "V": return 5;
                case "VI": return 6;
                case "VII": return 7;
                case "VIII": return 8;
                case "IX": return 9;
                case "X": return 10;
                case "XI": return 11;
                case "XII": return 12;
                case "XIII": return 13;
                case "XIV": return 14;
                case "XV": return 15;
                case "XVI": return 16;
                case "XVII": return 17;
                case "XVIII": return 18;
                case "XIX": return 19;
                default: return -1;
            }
        }


        public const int MIN_WORD_LENGTH = 5;

        /// <summary>
        /// 判断字符串编辑距离是否小于预设阈值
        /// </summary>
        /// <param name="a">字符串a</param>
        /// <param name="b">字符串b</param>
        /// <returns>是否满足条件</returns>
        public static bool ISEditDistOK(string a, string b) 
        {
            if (a.Length < MIN_WORD_LENGTH || b.Length < MIN_WORD_LENGTH) return false;

            int n = a.Length, m = b.Length;

            int[,] f = new int[n + 1, m + 1];

            for (int i = 0; i <= n; ++i)
                f[i, 0] = i;
            for (int j = 0; j <= m; ++j)
                f[0, j] = j;

            for (int i = 1; i <= n; ++i) 
            {
                for (int j = 1; j <= m; ++j) 
                {
                    int cost = 0;
                    if (b[j - 1] != a[i - 1])
                        cost = 1;

                    f[i, j] = Math.Min(Math.Min(f[i - 1, j], f[i, j - 1]) + 1, f[i - 1, j - 1] + cost);
                }
            }

            if (a.Length == MIN_WORD_LENGTH || b.Length == MIN_WORD_LENGTH)
                return f[n, m] <= 1;
            else return f[n, m] <= 2;
        }

        /// <summary>
        /// 返回单词相似度
        /// </summary>
        /// <param name="x">单词x对应节点</param>
        /// <param name="y">单词y对应节点</param>
        /// <returns></returns>
        public static int GetWordSim(WordNode x, WordNode y) 
        {
            return WordNode.CountOverlap(x.pre, y.pre) + WordNode.CountOverlap(x.next, y.next);
        }

        /// <summary>
        /// 获取某个时间所属时间区间（每3天一划分）
        /// </summary>
        /// <param name="time">时间信息</param>
        /// <returns>所属区间</returns>
        public static int GetDayDelta(DateTime time) 
        {
            DateTime t = new DateTime(2011, 8, 11);
            return time.Subtract(t).Days / 3;
        }
    }
}
