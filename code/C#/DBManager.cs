using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Lecast_P2
{
    /// <summary>
    /// 数据库接口
    /// </summary>
    class DBManager
    {
        static string _tableName;
        public static void InsertItem(string user, string sku, string category, string query, string click_time, string query_time, MySqlConnection conn)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Insert into " + _tableName + " values('" + user + "', '"
                + sku + "', '" + category + "', '" + query + "', '" + click_time + "', '" + query_time + "');";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static MySqlConnection GetConnection(string tableName, bool isFirst)
        {
            _tableName = tableName;
            string cs = @"server=localhost;userid=root;password=root;database=lecast";
            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                if (isFirst)
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "drop table if exists " + tableName;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "create table " + tableName + "(user VARCHAR(100), sku VARCHAR(20), category VARCHAR(20), query VARCHAR(255),  click_time TIMESTAMP, query_time TIMESTAMP)";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }

            return conn;
        }
    }
}
