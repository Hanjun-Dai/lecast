using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lecast_P2
{
    /// <summary>
    /// 基于搜索词匹配的得分
    /// </summary>
    class SkuMatcher
    {
        const double COMBO_BONUS = 0.8;

        /// <summary>
        /// 获取单词的得分
        /// </summary>
        /// <param name="dict">数据字典</param>
        /// <param name="sku">产品id</param>
        /// <param name="word">单词</param>
        /// <returns>得分</returns>
        static double GetWordPairScore(Dictionary<string, WordNode> dict, string sku, string word) 
        {
            var node = dict[word];
            return Math.Log10(node.cnt + 1) * (node.cnt + 0.0) / WordProcessor.skuMaxWordCnt[sku];
        }

        /// <summary>
        /// 根据搜索词确定产品关联度
        /// </summary>
        /// <param name="sku">产品编号</param>
        /// <param name="query">用户输入的查询</param>
        /// <returns>关联度打分</returns>
        public static double GetSkuQuerySim(string sku, string query)
        {
            double score = 0;
            string[] words = WordProcessor.GetCorrectQuery(sku, query);
            var dict = WordProcessor.skuKeywords[sku];

            for (int i = 0; i < words.Length; ++i)
            {
                if (dict.ContainsKey(words[i]))
                {
                    score += GetWordPairScore(dict, sku, words[i]);
                    if (i > 0) 
                    {
                        var node = dict[words[i]];
                        if (node.pre.Contains(words[i - 1]))
                            score += COMBO_BONUS;
                    }
                }
            }
            return score;
        }
    }
}
