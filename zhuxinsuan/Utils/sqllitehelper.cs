using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Data;

namespace zhuxinsuan.Utils
{
    public class sqllitehelper
    {
        string connstr = "data source=" + AppDomain.CurrentDomain.BaseDirectory + @"App_Data\zxs.db;Version=3";

        public DataTable GetData()
        {
            using (var conn=new SQLiteConnection(connstr))
            {
                string _ip = clienthelper.GetWebClientIp();
                var cmd = new SQLiteCommand() {
                    Connection = conn,
                    CommandType=System.Data.CommandType.Text,
                    CommandText= "select * from accesslog a order by a.insertdate asc"
                };
                var adpter = new SQLiteDataAdapter { SelectCommand = cmd };
                var table = new DataTable();
                conn.Open();
                int i = adpter.Fill(table);
                conn.Close();
                return table;
            }
        }
        public int InsertData()
        {
            using (var conn = new SQLiteConnection(connstr))
            {
                string _ip = clienthelper.GetWebClientIp();
                var cmd = new SQLiteCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "INSERT INTO \"main\".\"accesslog\"(\"note\", \"insertdate\") VALUES ('" + _ip + "', '"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"')"
                };
                conn.Open();
                int i=cmd.ExecuteNonQuery();
                conn.Close();
                return i;
            }
        }
    }
}