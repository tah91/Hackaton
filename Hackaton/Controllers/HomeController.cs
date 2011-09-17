using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hackaton.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Message = "Buisness man, welcome";

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
