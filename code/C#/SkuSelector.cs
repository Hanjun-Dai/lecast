using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;

namespace Lecast_P2
{
    /// <summary>
    /// 算法主框架，计算每个产品最终得分，并根据得分排序输出
    /// </summary>
    class SkuSelector
    {
        const int DISP_NUM = 5;

        public static Dictionary<string, HashSet<string>> userHist;

        static void Sort(Tuple<string, double, double>[] matchResults) 
        {
            for (int i = 0; i < matchResults.Length - 1; ++i)
                for (int j = i + 1; j < matchResults.Length; ++j)
                    if (matchResults[i].Item3 + matchResults[i].Item2 < matchResults[j].Item3 + matchResults[j].Item2) 
                    {
                        var tmp = matchResults[i]; matchResults[i] = matchResults[j]; matchResults[j] = tmp;
                    }
        }

        /// <summary>
        /// 为当前用户猜测其最可能喜欢的产品
        /// </summary>
        /// <param name="user">用户id</param>
        /// <param name="query">用户输入的查询</param>
        /// <param name="click_time">用户点击产品的时间</param>
        /// <returns></returns>
        static string GuessBestSku(string user, string query, DateTime click_time) 
        {
            string[] games = WordProcessor.skuKeywords.Keys.ToArray();
            Tuple<string, double, double>[] matchResults = new Tuple<string, double, double>[games.Length];
            for (int i = 0; i < games.Length; ++i) 
            {
                double wordsScore = SkuMatcher.GetSkuQuerySim(games[i], query);
                double cfScore = CF.GetCFValue(games[i], user, click_time);
                if (userHist.ContainsKey(user) && userHist[user].Contains(games[i]))
                {
                    //用户不太可能点击一个他之前点击过的产品
                    wordsScore = 0; cfScore = 0;
                }
                //打分结果由两部分构成，分别是查询词得分和协同过滤得分。
                matchResults[i] = new Tuple<string, double, double>(games[i], wordsScore, cfScore);
            }

            Sort(matchResults);
            string skus = matchResults[0].Item1;
            for (int i = 1; i < DISP_NUM; ++i)
                skus = skus + " " + matchResults[i].Item1;

            return skus;
        }

        /// <summary>
        /// 加载用户历史记录
        /// </summary>
        public static void LoadHist()
        {
            userHist = new Dictionary<string, HashSet<string>>();

            var conn = DBManager.GetConnection("train", false);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select user,sku from train";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string user = reader.GetString(0);
                string sku = reader.GetString(1);
                if (!userHist.ContainsKey(user))
                    userHist.Add(user, new HashSet<string>());
                if (!userHist[user].Contains(sku))
                    userHist[user].Add(sku);
            }

            reader.Close();
            conn.Close();
        }

        /// <summary>
        /// 查询主过程
        /// </summary>
        public static void Query() 
        {
            var conn = DBManager.GetConnection("test", false);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select * from test";
            var reader = cmd.ExecuteReader();
            List<string> result = new List<string>(50000);
            while (reader.Read())
            {
                string user = reader.GetString(0);
                string query = reader.GetString(3);
                DateTime click_time = reader.GetDateTime(4);
                string skus = GuessBestSku(user, query, click_time);
                result.Add(skus);
            }

            File.WriteAllLines("predictions.csv", new string[]{"sku"});
            File.AppendAllLines("predictions.csv", result);
            reader.Close();
            conn.Close();
        }
    }
}
