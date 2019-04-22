using Kenel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using zhuxinsuan.Utils;

namespace zhuxinsuan.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            sqllitehelper _sh = new sqllitehelper();
            try
            {
                if (HttpRuntime.Cache["ftoken"] == null)
                {
                    //缓存要依赖的文件的路径（我这里写的是绝对路径，也可以写虚拟路径，到时候给它转成物理路径就可以了）
                    string filePath = Server.MapPath("~/App_Data/cacketrigger.txt");

                    //这段代码无缓存依赖没有关系，这里仅仅是将上面那个路径的文件读出来而已（到时候用作缓存的值，当然你也可以设置别的值，这里仅仅是做是案例而已）
                    DataTable dt = _sh.GetToken();
                    string msg = (dt != null && dt.Rows.Count > 0) ? dt.Rows[0][0].ToString() : "";
                    //创建一个缓存依赖对象（并用缓存依赖文件的服务器端的物理路径给它初始化。只要文件的内容改变，它就会通知framework清空缓存
                    System.Web.Caching.CacheDependency cDep = new System.Web.Caching.CacheDependency(filePath);
                    //第一个参数：缓存的键
                    //第二个参数：缓存的值
                    //第三个参数：缓存依赖的对象
                    //第四个参数：缓存的绝对过期时间，从用户第一次请求开始计时（因为这里我们是使用的缓存依赖，所以这里使用NoAbsoluteExpiration，表示没有到期时间，即永不过期）
                    //第五个参数：缓存的可调过期时间，从用户最后一次请求开始计时(因为这里我们是使用缓存依赖，所以这里使用NoSlidingExpiration，表示没有可调过期时间)
                    //第六个参数：指定 System.Web.Caching.Cache 对象中存储的项的相对优先级。(它是一个枚举) Normal是默认值
                    //第七个参数：缓存依赖的回调函数（缓存被清除或修改的时候调用此方法）
                    //【其实第七个参数它是一个委托，既然它是一个委托，所以我们使用它的时候需要传递一个方法签名和这个委托签名一样的方法给它】
                    //Cache.Insert()方法也可以用Cache.Add()方法替代，只是Cache.Add(）方法的参数个数比较多，有时候我们不需要那么多参数的时候就使用Cache.Insert()，比较方便
                    HttpRuntime.Cache.Insert("ftoken", msg, cDep, System.Web.Caching.Cache.NoAbsoluteExpiration,
                        System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, CacheCallBack);
                    ViewBag.bdyctoken = msg;
                }
                else
                {
                    //如果key为“fmsg”的缓存存在数据，就将它赋值给ViewData["msg"]
                    ViewBag.bdyctoken = HttpRuntime.Cache["ftoken"].ToString();
                }
                int db = _sh.InsertData("");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Plus()
        {
            return View();
        }

        public ActionResult gsk()
        {

            if (HttpRuntime.Cache["fmsg"] == null)
            {
                //缓存要依赖的文件的路径（我这里写的是绝对路径，也可以写虚拟路径，到时候给它转成物理路径就可以了）
                string filePath = Server.MapPath("~/App_Data/cacketrigger.txt");

                //这段代码无缓存依赖没有关系，这里仅仅是将上面那个路径的文件读出来而已（到时候用作缓存的值，当然你也可以设置别的值，这里仅仅是做是案例而已）
                string msg = System.IO.File.ReadAllText(filePath, System.Text.Encoding.Default);

                //创建一个缓存依赖对象（并用缓存依赖文件的服务器端的物理路径给它初始化。只要文件的内容改变，它就会通知framework清空缓存
                System.Web.Caching.CacheDependency cDep = new System.Web.Caching.CacheDependency(filePath);
                //第一个参数：缓存的键
                //第二个参数：缓存的值
                //第三个参数：缓存依赖的对象
                //第四个参数：缓存的绝对过期时间，从用户第一次请求开始计时（因为这里我们是使用的缓存依赖，所以这里使用NoAbsoluteExpiration，表示没有到期时间，即永不过期）
                //第五个参数：缓存的可调过期时间，从用户最后一次请求开始计时(因为这里我们是使用缓存依赖，所以这里使用NoSlidingExpiration，表示没有可调过期时间)
                //第六个参数：指定 System.Web.Caching.Cache 对象中存储的项的相对优先级。(它是一个枚举) Normal是默认值
                //第七个参数：缓存依赖的回调函数（缓存被清除或修改的时候调用此方法）
                //【其实第七个参数它是一个委托，既然它是一个委托，所以我们使用它的时候需要传递一个方法签名和这个委托签名一样的方法给它】
                //Cache.Insert()方法也可以用Cache.Add()方法替代，只是Cache.Add(）方法的参数个数比较多，有时候我们不需要那么多参数的时候就使用Cache.Insert()，比较方便
                HttpRuntime.Cache.Insert("fmsg", msg, cDep, System.Web.Caching.Cache.NoAbsoluteExpiration,
                    System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, CacheCallBack);
                ViewBag.msg = msg;
            }
            else
            {
                //如果key为“fmsg”的缓存存在数据，就将它赋值给ViewData["msg"]
                ViewBag.msg = HttpRuntime.Cache["fmsg"].ToString();
            }
            sqllitehelper _sh = new sqllitehelper();
            DataTable dt = _sh.GetData();
            ViewBag.alldata = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
            return View();
        }

        /// <summary>
        /// 缓存依赖的回调函数（缓存被清除的时候调用此方法）
        /// </summary>
        /// <param name="key">缓存的键</param>
        /// <param name="value">缓存的值</param>
        /// <param name="reason">缓存被移除的原因</param>
        void CacheCallBack(string key, object value, CacheItemRemovedReason reason)
        {
            //是用logHelper对象来记录日记
            sqllitehelper _sh = new sqllitehelper();
            int db = _sh.InsertData("在Home控制器下的Index方法中，Cache[" + key + "]=" + value.ToString() + "因为" + reason.ToString() + "被删除了");
        }

        public ActionResult UpdateToken()
        {
            HttpResult httpResult = new HttpResult();
            sqllitehelper _sh = new sqllitehelper();
            try
            {
                bool re = _sh.updateorinserttoken("windowserver调用");
                if (re)
                {
                    httpResult.ErrorMessage = "更新token成功";
                    httpResult.Result = true;
                    //获取删除token缓存
                    if (HttpRuntime.Cache["ftoken"] != null)
                    {
                        HttpRuntime.Cache.Remove("ftoken");
                    }
                    _sh.InsertData("更新token成功");
                }
                else
                {
                    httpResult.ErrorMessage = "更新token失败";
                    httpResult.Result = false;
                    _sh.InsertData("更新token失败");
                }
            }
            catch(Exception ex)
            {
                httpResult.ErrorMessage = ex.ToString();
                httpResult.Result = false;
                _sh.InsertData("更新token失败"+ ex.ToString());

            }
            return Json(httpResult,JsonRequestBehavior.AllowGet);
        }
    }
}