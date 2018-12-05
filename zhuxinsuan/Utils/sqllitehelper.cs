using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;

namespace zhuxinsuan.Utils
{
    public class sqllitehelper
    {
        //string connstr = "data source=" + AppDomain.CurrentDomain.BaseDirectory + @"App_Data\zxs.db;Version=3";
        string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["connStr"].ToString();

        public DataTable GetData()
        {
            using (var conn = new SQLiteConnection(connstr))
            {
                string _ip = clienthelper.GetWebClientIp();
                var cmd = new SQLiteCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "select * from accesslog a order by a.insertdate asc"
                };
                var adpter = new SQLiteDataAdapter { SelectCommand = cmd };
                var table = new DataTable();
                conn.Open();
                int i = adpter.Fill(table);
                conn.Close();
                return table;
            }
        }
        public int InsertData(string msg)
        {
            using (var conn = new SQLiteConnection(connstr))
            {
                string _ip = clienthelper.GetWebClientIp();
                var cmd = new SQLiteCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "INSERT INTO \"main\".\"accesslog\"(\"note\", \"insertdate\") VALUES ('" + msg + "||" + _ip + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')"
                };
                conn.Open();
                int i = cmd.ExecuteNonQuery();
                conn.Close();
                return i;
            }
        }

        public int InsertToken(string token)
        {
            DataTable dt = this.GetToken();
            if (dt != null && dt.Rows.Count > 0)
            {
                using (var conn = new SQLiteConnection(connstr))
                {
                    string _ip = clienthelper.GetWebClientIp();
                    var cmd = new SQLiteCommand()
                    {
                        Connection = conn,
                        CommandType = System.Data.CommandType.Text,
                        CommandText = "update baidutoken set token='" + token + "',insertdate='"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"'"
                    };
                    conn.Open();
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();
                    return i;
                }
            }
            else
            {
                using (var conn = new SQLiteConnection(connstr))
                {
                    string _ip = clienthelper.GetWebClientIp();
                    var cmd = new SQLiteCommand()
                    {
                        Connection = conn,
                        CommandType = System.Data.CommandType.Text,
                        CommandText = "INSERT INTO \"main\".\"baidutoken\"(\"token\", \"insertdate\") VALUES ('" + token + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')"
                    };
                    conn.Open();
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();
                    return i;
                }
            }

        }

        public DataTable GetToken()
        {
            using (var conn = new SQLiteConnection(connstr))
            {
                string _ip = clienthelper.GetWebClientIp();
                var cmd = new SQLiteCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "select * from baidutoken a order by a.insertdate asc"
                };
                var adpter = new SQLiteDataAdapter { SelectCommand = cmd };
                var table = new DataTable();
                conn.Open();
                int i = adpter.Fill(table);
                conn.Close();
                return table;
            }
        }

        public bool updateorinserttoken()
        {
            try
            {
                string backstr = "";
                string url = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id=aMFjoWoASorGfLEvDKzO4qN9&client_secret=jfwT0OmRi4OjXDb93Z29eMvH9EmObjeZ";
                //获取token
                //第一步-- - 构建HttpWebRequest
                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.ContentType = "application/json";
                WebReq.Method = "Get";
                //第二步--写入参数
                //WebReq.ContentLength = Encoding.UTF8.GetByteCount(param);
                //using (StreamWriter requestW = new StreamWriter(WebReq.GetRequestStream()))
                //{
                //    //requestW.Write(param);
                //}
                //第三步--获取返回的结果
                using (HttpWebResponse response = (HttpWebResponse)WebReq.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                    backstr = sr.ReadToEnd();
                }
                Hashtable dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(backstr);
                this.InsertToken(dic["access_token"] != null ? dic["access_token"].ToString() : "");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}