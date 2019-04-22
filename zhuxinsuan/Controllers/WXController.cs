using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using zhuxinsuan.Utils;

namespace zhuxinsuan.Controllers
{
    public class WXController : Controller
    {
        sqllitehelper sss = new sqllitehelper();
        static List<string> msgidlist = new List<string>();
        // GET: WX
        /// <summary>
        /// 公众号接入测试
        /// </summary>
        /// <param name="signature">微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
        /// <returns>开发者通过检验signature对请求进行校验（下面有校验方式）。若确认此次GET请求来自微信服务器，请原样返回echostr参数内容，则接入生效，成为开发者成功，否则接入失败。</returns>
        [HttpGet]
        [ActionName("Check")]
        public ActionResult Get(string signature, string timestamp, string nonce, string echostr)
        {
            /*以下为首次验证开发者*/
            var myToken = ConfigurationManager.AppSettings["myToken"];
            sss.InsertData("配置的myToken:" + myToken);
            var list = new string[] { myToken, timestamp, nonce };
            Array.Sort(list);
            var sortStr = string.Join("", list);
            new sqllitehelper().InsertData("明文sortStr:" + sortStr + "|echostr:" + echostr);
            var sha1Str = FormsAuthentication.HashPasswordForStoringInConfigFile(sortStr, "SHA1").ToLower();

            new sqllitehelper().InsertData("sha1Str:" + sha1Str + "||signature:" + signature);
            if (sha1Str == signature)//根据微信的验证规则做判断
            {
                sss.InsertData("比较结果相等呀sha1Str:" + sha1Str + "||signature:" + signature);
                //return GetReturn(echostr);
                return Content(echostr);
            }
            new sqllitehelper().InsertData("校验错误了:22");
            //return GetReturn("error");
            return Content("");
        }

        /// <summary>
        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML。
        /// </summary>
        [HttpPost]
        [ActionName("Check")]
        public ActionResult Post(string signature, string timestamp, string nonce, string echostr)
        {
            //Stream requestStream = System.Web.HttpContext.Current.Request.InputStream;
            //byte[] requestByte = new byte[requestStream.Length];
            //requestStream.Read(requestByte, 0, (int)requestStream.Length);
            //string requestStr = Encoding.UTF8.GetString(requestByte);
            //sss.InsertData("requestStr:" + requestStr);
            //return Content(requestStr);

            wxmessage wx = GetWxMessage();
            string res = "success";
            sss.InsertData("DDDD");
            if (!string.IsNullOrEmpty(wx.EventName) && wx.EventName.Trim() == "subscribe")
            {
                //刚关注时的时间，用于欢迎词
                string content = "";
                content = "您好，欢迎关注郭顺开公众号";
                res = sendTextMessage(wx, content);
                return Content(res);
            }
            else if (!string.IsNullOrEmpty(wx.MsgType) && wx.MsgType.Trim() == "text")
            {
                res = sendTextMessage(wx, "接收到的消息text（:<a href=\"http://www.qq.com\">" + wx.Content + "</a>)");
                return Content(res);
            }
            else if (!string.IsNullOrEmpty(wx.MsgType) && wx.MsgType.Trim() == "image" && !msgidlist.Contains(wx.MsgId))
            {
                res = sendTextMessage(wx, "接收到的消息image（:<a href=\"" + wx.PicUrl + "\">" + wx.MediaId + "</a>)");
                return Content(res);
            }
            else if (!string.IsNullOrEmpty(wx.MsgType) && wx.MsgType.Trim() == "voice" && !msgidlist.Contains(wx.MsgId))
            {
                res = sendTextMessage(wx, "接收到的消息voice（:" + wx.MediaId + ")");
                return Content(res);
            }
            return Content(res);

        }


        public static HttpResponseMessage GetReturn(string message)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(message, Encoding.UTF8, "text/html")
            };
        }




        /// <summary>
        /// 获取和设置微信类中的信息
        /// </summary>
        /// <returns></returns>
        private wxmessage GetWxMessage()
        {
            wxmessage wx = new wxmessage();
            StreamReader str = new StreamReader(Request.InputStream, Encoding.UTF8);
            XmlDocument xml = new XmlDocument();
            xml.Load(str);
            str.Close();
            str.Dispose();
            sss.InsertData("xml对象：" + Newtonsoft.Json.JsonConvert.SerializeObject(xml));
            if (xml.SelectSingleNode("xml").SelectSingleNode("MsgId") != null)
            {
                wx.MsgId = xml.SelectSingleNode("xml").SelectSingleNode("MsgId").InnerText;
            }


            wx.ToUserName = xml.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            wx.FromUserName = xml.SelectSingleNode("xml").SelectSingleNode("FromUserName").InnerText;
            wx.MsgType = xml.SelectSingleNode("xml").SelectSingleNode("MsgType").InnerText;
            if (wx.MsgType.Trim() == "text")
            {
                wx.Content = xml.SelectSingleNode("xml").SelectSingleNode("Content").InnerText;
            }
            else if (wx.MsgType.Trim() == "image")
            {
                wx.PicUrl = xml.SelectSingleNode("xml").SelectSingleNode("PicUrl").InnerText;
                wx.MediaId = xml.SelectSingleNode("xml").SelectSingleNode("MediaId").InnerText;
            }
            else if (wx.MsgType.Trim() == "voice")
            {
                wx.MediaId = xml.SelectSingleNode("xml").SelectSingleNode("MediaId").InnerText;
            }
            else if (wx.MsgType.Trim() == "event")
            {
                wx.EventName = xml.SelectSingleNode("xml").SelectSingleNode("Event").InnerText;
                wx.EventKey = xml.SelectSingleNode("xml").SelectSingleNode("EventKey").InnerText;
            }
            return wx;
        }

        /// <summary>  
        /// 发送文字消息  
        /// </summary>  
        /// <param name="wx" />获取的收发者信息  
        /// <param name="content" />内容  
        /// <returns></returns>  
        private string sendTextMessage(wxmessage wx, string content)
        {

            if (wx.MsgId != null && (!msgidlist.Contains(wx.MsgId)))
            {
                msgidlist.Add(wx.MsgId);
            }
            sss.InsertData("content:" + content);
            string res = "";
            switch (wx.MsgType)
            {
                case "text":
                    res = string.Format(Message_Text,
                    wx.FromUserName, wx.ToUserName, DateTime.Now.Ticks, "文字标题如下:\n"+content+"\n文本结尾引脚");
                    break;
                case "image":
                    res = string.Format(Message_Image,
                    wx.FromUserName, wx.ToUserName, DateTime.Now.Ticks, wx.MsgType,wx.MediaId);
                    break;
                case "voice":
                    res = string.Format(Message_Voice,
                    wx.FromUserName, wx.ToUserName, DateTime.Now.Ticks, wx.MsgType, wx.MediaId);
                    break;
                default:
                    res = string.Format(Message_Text,
                    wx.FromUserName, wx.ToUserName, DateTime.Now.Ticks, content);
                    break;
            }
            sss.InsertData("res:" + res);
            return res;
        }
        /// <summary>
        /// 普通文本消息
        /// </summary>
        private static string Message_Text
        {
            get
            {
                return @"<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>{2}</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{3}]]></Content></xml>";
            }
        }

        /// <summary>
        /// 图片消息
        /// </summary>
        private static string Message_Image
        {
            get
            {
                return @"<xml>
 <ToUserName><![CDATA[{0}]]></ToUserName>
 <FromUserName><![CDATA[{1}]]></FromUserName>
 <CreateTime>{2}</CreateTime>
 <MsgType><![CDATA[{3}]]></MsgType>
 <Image>
 <MediaId><![CDATA[{4}]]></MediaId>
 </Image>
 </xml>";
            }
        }

        /// <summary>
        /// 声音消息
        /// </summary>
        private static string Message_Voice
        {
            get
            {
                return @"<xml>
 <ToUserName><![CDATA[{0}]]></ToUserName>
 <FromUserName><![CDATA[{1}]]></FromUserName>
 <CreateTime>{2}</CreateTime>
 <MsgType><![CDATA[{3}]]></MsgType>
 <Voice>
 <MediaId><![CDATA[{4}]]></MediaId>
 </Voice>
 </xml>";
            }
        }


        /// <summary>
        /// datetime转换为unixtime
        /// 将时间类型转换为整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
    }

    public class wxmessage
    {
        /// <summary>
        /// 本公众帐号
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// 用户帐号
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// 发送时间戳
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 发送的文本内容 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息的类型
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 图片链接
        /// </summary>
        public string PicUrl { get; set; }
        /// <summary>
        /// 图片消息媒体id
        /// </summary>
        public string MediaId { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MsgId { get; set; }



        //这两个属性会在后面的讲解中提到
        public string Recognition { get; set; }
        public string EventKey { get; set; }
    }
}