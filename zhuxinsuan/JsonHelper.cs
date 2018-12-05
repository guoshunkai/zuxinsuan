using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Kenel
{
    public static class JsonHelper
    {

        public static string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        /// <summary>
        /// 将对象序列化json字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        public static string Encode(object o)
        {
            if (o == null || o.ToString() == "null") return null;

            if (o != null && (o.GetType() == typeof(String) || o.GetType() == typeof(string)))
            {
                return o.ToString();
            }
            IsoDateTimeConverter dt = new IsoDateTimeConverter();
            dt.DateTimeFormat = DateTimeFormat;
            return JsonConvert.SerializeObject(o, dt);

        }

        public static string Object2Json(object o)
        {
            return Encode(o);
        }

        /// <summary>
        /// 将json字符串序列为对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Decode(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            object o = JsonConvert.DeserializeObject(json);
            if (o.GetType() == typeof(String) || o.GetType() == typeof(string))
            {
                o = JsonConvert.DeserializeObject(o.ToString());
            }
            object v = toObject(o);
            return v;
        }

        public static object Json2Object(string json)
        {
            return Decode(json);
        }
        public static object Decode(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }
        private static object toObject(object o)
        {
            if (o == null) return null;

            if (o.GetType() == typeof(string))
            {
                //判断是否符合2010-09-02T10:00:00的格式
                string s = o.ToString();
                if (s.Length == 19 && s[10] == 'T' && s[4] == '-' && s[13] == ':')
                {
                    o = System.Convert.ToDateTime(o);
                }
            }
            else if (o is JObject)
            {
                JObject jo = o as JObject;

                Hashtable h = new Hashtable();

                foreach (KeyValuePair<string, JToken> entry in jo)
                {
                    h[entry.Key] = toObject(entry.Value);
                }

                o = h;
            }
            else if (o is IList)
            {

                ArrayList list = new ArrayList();
                list.AddRange((o as IList));
                int i = 0, l = list.Count;
                for (; i < l; i++)
                {
                    list[i] = toObject(list[i]);
                }
                o = list;

            }
            else if (typeof(JValue) == o.GetType())
            {
                JValue v = (JValue)o;
                o = toObject(v.Value);
            }
            else
            {
            }
            return o;
        }



        /// <summary>
        /// 将datatable转换为ArrayList
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ArrayList DataTable2ArrayList(DataTable data)
        {
            return DataTable2ArrayList(data, true);
        }

        /// <summary>
        /// 将datatable转换为ArrayList
        /// </summary>
        /// <param name="data">目标datatable</param>
        /// <param name="IsShowNull">是否转换值未Null的字段，true表示转换,flase表示不转换</param>
        /// <returns></returns>
        public static ArrayList DataTable2ArrayList(DataTable data, bool IsShowNull)
        {
            ArrayList array = new ArrayList();
            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];

                Hashtable record = new Hashtable();
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    object cellValue = row[j];
                    if (cellValue.GetType() == typeof(DBNull))
                    {
                        cellValue = null;
                        if (!IsShowNull)
                        {
                            continue;
                        }
                    }
                    record[data.Columns[j].ColumnName] = cellValue;
                }
                array.Add(record);
            }
            return array;
        }

        /// <summary>
        /// 将ArrayList转换为Json对象
        /// </summary>
        /// <param name="dataAll">数据源</param>
        /// <returns></returns>
        public static HttpResult ArrayList2Json(ArrayList dataAll)
        {
            return ArrayList2Json(dataAll, true);
        }

        /// <summary>
        /// 将ArrayList转换为Json对象
        /// </summary>
        /// <param name="dataAll">数据源</param>
        /// <param name="IsShowId">是否显示ID</param>
        /// <returns></returns>

        public static HttpResult ArrayList2Json(ArrayList dataAll, bool IsShowId)
        {
            HttpResult httpResult = new HttpResult();
            if (dataAll == null)
            {
                httpResult.Result = false;
                httpResult.ErrorMessage = "运行时错误：函数ArrayList2Json中参数dataAll为NUL！";
                return httpResult;
            }

            try
            {
                string k = string.Empty;
                string v = string.Empty;
                ArrayList data = new ArrayList();
                for (int i = 0, l = dataAll.Count; i < l; i++)
                {
                    Hashtable LowerRecord = new Hashtable();

                    Hashtable record = (Hashtable)dataAll[i];

                    foreach (DictionaryEntry de in record)
                    {
                        if (de.Key != null)
                        {
                            k = de.Key.ToString().ToLower();
                        }

                        if (de.Value != null)
                        {
                            v = de.Value.ToString();
                        }
                        else
                        {
                            v = "";
                        }

                        LowerRecord.Add(k, v);
                    }

                    if (record == null) continue;
                    if (IsShowId)
                    {
                        LowerRecord.Add("id", (i + 1).ToString());
                    }

                    data.Add(LowerRecord);
                }

                Hashtable jsonResult = new Hashtable();
                if (dataAll.Count > 0)
                {
                    var o = ((Hashtable)dataAll[dataAll.Count - 1])["totalnum"];
                    if (o != null)
                    {
                        jsonResult["total"] = o;
                    }
                    else
                    {
                        jsonResult["total"] = dataAll.Count;
                    }

                }
                else
                {
                    jsonResult["total"] = "0";
                }
                //2.将分页数据与总条数装箱，封装为对象

                jsonResult["rows"] = data;
                httpResult.KeyValue = JsonHelper.Encode(jsonResult);

            }
            catch (Exception ex)
            {
                httpResult.Result = false;
                httpResult.ErrorMessage = ex.Message;
            }

            return httpResult;

        }


        /// <summary>
        /// 对象转化为JSON字符串
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string JsonSerialize(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            return InternalJsonSerialize(o);
        }

        private static string InternalJsonSerialize(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            JavaScriptSerializer seri = new JavaScriptSerializer();
            return seri.Serialize(o);
        }


        /// <summary>
        /// 将json字符串反序列化为对象(支持泛型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("s");
            }

            return (T)InternalJsonDeserialize<T>(s);

        }

        private static object InternalJsonDeserialize<T>(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("s");
            }
            JavaScriptSerializer seri = new JavaScriptSerializer();
            return seri.Deserialize<T>(s);
        }





    }
}
