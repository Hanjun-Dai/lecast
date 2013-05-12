using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lecast_P2
{
    /// <summary>
    /// 原始数据处理类
    /// </summary>
    class RawData
    {
        public static void LoadRawTrainingData() 
        {
            string[] lines = File.ReadAllLines("train_raw.csv");
            var conn = DBManager.GetConnection("train", true);
            for (int i = 1; i < lines.Length; ++i)
            {
                Console.WriteLine(i);
                string[] words = lines[i].Split(',');
                for (int j = 0; j < words.Length; ++j)
                {
                    if (words[j].StartsWith("\""))
                        words[j] = words[j].Substring(1, words[j].Length - 2);
                }
                DBManager.InsertItem(words[0], words[1], words[2], words[3], words[4], words[5], conn);
            }
        }

        public static void LoadRawTestData() 
        {
            string[] lines = File.ReadAllLines("test_raw.csv");
            var conn = DBManager.GetConnection("test", true);
            for (int i = 1; i < lines.Length; ++i)
            {
                Console.WriteLine(i);
                string[] words = lines[i].Split(',');
                for (int j = 0; j < words.Length; ++j)
                {
                    if (words[j].StartsWith("\""))
                        words[j] = words[j].Substring(1, words[j].Length - 2);
                }
                DBManager.InsertItem(words[0], string.Empty, words[1], words[2], words[3], words[4], conn);
            }
        }
    }
}
