using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zhuxinsuan.Utils;

namespace zhuxinsuan.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                sqllitehelper _sh = new sqllitehelper();
                int db = _sh.InsertData();
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

        public ActionResult gsk()
        {

            return View();
        }
    }
}