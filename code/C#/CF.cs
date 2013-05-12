using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;

namespace Lecast_P2
{
    /// <summary>
    /// 协同过滤算法类
    /// </summary>
    class CF
    {
        const int SKU_NUM = 413;                                // 产品数量（暂时为硬编码）
        const double lambda = 5;                                // 权重系数
        static Dictionary<string, int> userIdx;                 // 用户id到数字编号的映射
        static Dictionary<string, int> skuIdx;                  // 产品id到数字编号的映射
        static double[,] simMatrix, sku_day, sku_hour;          // 产品相似度矩阵

        /// <summary>
        /// 建立点击信息矩阵
        /// </summary>
        static void BuildItemMatrix() 
        {
            userIdx = new Dictionary<string,int>();
            skuIdx = new Dictionary<string, int>();
            var conn = DBManager.GetConnection("train", false);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select user, sku, click_time from train";
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string user = reader.GetString(0);
                string sku = reader.GetString(1);
                DateTime click_time = reader.GetDateTime(2);
                if (!userIdx.ContainsKey(user)) 
                    userIdx.Add(user, userIdx.Count);
                if (!skuIdx.ContainsKey(sku))
                    skuIdx.Add(sku, skuIdx.Count);
                //File.AppendAllLines("clickinfo.txt", new string[] { userIdx[user] + " " + skuIdx[sku] + " " + click_time.ToString() });
            }

            //foreach (var key in userIdx.Keys)
            //    File.AppendAllLines("userid.txt", new string[] { userIdx[key] + " " + key });

            //foreach (var key in skuIdx.Keys)
            //    File.AppendAllLines("skuid.txt", new string[] { skuIdx[key] + " " + key });

            reader.Close();
            conn.Close();
        }
        
        /// <summary>
        /// 加载用matlab预处理的产品信息，包括产品-产品相似度，产品点击率关于两个时间维度的分布
        /// </summary>
        static void LoadInfos() 
        {
            string[] tmp = File.ReadAllLines("simmatrix.txt");

            simMatrix = new double[SKU_NUM, SKU_NUM];
            for (int i = 0; i < SKU_NUM; ++i) 
            {
                string[] t = tmp[i].Trim().Split(' ');
                for (int j = 0; j < SKU_NUM; ++j)
                    simMatrix[i, j] = double.Parse(t[j]);
            }

            tmp = File.ReadAllLines("sku_day.txt");
            sku_day = new double[SKU_NUM, 28];
            for (int i = 0; i < SKU_NUM; ++i) 
            {
                string[] t = tmp[i].Trim().Split(' ');
                for (int j = 0; j < 28; ++j)
                    sku_day[i, j] = double.Parse(t[j]);
            }

            tmp = File.ReadAllLines("sku_hour.txt");
            sku_hour = new double[SKU_NUM, 24];
            for (int i = 0; i < SKU_NUM; ++i)
            {
                string[] t = tmp[i].Trim().Split(' ');
                for (int j = 0; j < 24; ++j)
                    sku_hour[i, j] = double.Parse(t[j]);
            }

            Console.WriteLine("Load finished");
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize() 
        {
            SkuSelector.LoadHist();     //加载用户历史数据
            BuildItemMatrix();          //建立点击信息矩阵（供matlab处理）
            LoadInfos();                //加载matlab处理结果
        }

        /// <summary>
        /// 获取用户关于当前产品的推荐值
        /// </summary>
        /// <param name="sku">产品编号</param>
        /// <param name="user">用户id</param>
        /// <param name="time">点击时间</param>
        /// <returns>推荐值</returns>
        public static double GetCFValue(string sku, string user, DateTime time) 
        {
            double score = 0;
            int idx = skuIdx[sku];

            int day = Tools.GetDayDelta(time);
            int hour = time.Hour;
            score += sku_day[idx, day] + sku_hour[idx, hour];

            //只有当用户有历史记录的时候才进行个性化推荐
            if (SkuSelector.userHist.ContainsKey(user))
            {
                double lower = 0, sum = 0;
                HashSet<string> hist = SkuSelector.userHist[user];
                foreach (var id in hist) 
                {
                    int sku_id = skuIdx[id];
                    if (sku_id != idx)
                    {
                        sum += simMatrix[sku_id, idx];
                        lower = Math.Min(simMatrix[sku_id, idx], lower);
                    }
                }

                if (sum > 0)
                    sum /= GetSum(idx, lower);

                score += sum;
            }

            return score * lambda;
        }

        static double GetSum(int sku, double lowerbound) 
        {
            double ans = 0;
            for (int i = 0; i < SKU_NUM; ++i)
                if (i != sku && simMatrix[sku, i] >= lowerbound)
                    ans += simMatrix[sku, i];
            return ans;
        }
    }
}
