using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;

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

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Search()
		{
			return View(new Search());
		}

		RouteValueDictionary GetRvd(Search search)
		{
			var rvd = new RouteValueDictionary();
			//fill it
			rvd["Latitude"] = search.Latitude;
			rvd["Longitude"] = search.Longitude;
			rvd["OfferType"] = search.OfferType;
			rvd["Wifi"] = search.Wifi;
			rvd["Cafe"] = search.Cafe;
			rvd["Resto"] = search.Resto;
			rvd["Parking"] = search.Parking;
			rvd["EasyAccess"] = search.EasyAccess;

			return rvd;
		}

		Search GetSearch(NameValueCollection collection)
		{
			var search = new Search();
			search.Latitude = double.Parse(collection["Latitude"] ) ;
			search.Longitude = double.Parse(collection["Longitude"]) ;
			search.OfferType=int.Parse(collection["OfferType"]) ;
			search.Wifi = bool.Parse(collection["Wifi"]) ;
			search.Cafe = bool.Parse(collection["Cafe"] );
			search.Resto = bool.Parse(collection["Resto"]);
			search.Parking = bool.Parse(collection["Parking"]);
			search.EasyAccess = bool.Parse(collection["EasyAccess"]);

			return search;
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Search(Search search)
		{
			var rvd = GetRvd(search);
			return RedirectToAction("Results", rvd);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Results()
		{
			var results = new List<Place>();
			var search = GetSearch(Request.Params);

			//process search
			results.Add(new Place
			{
				Name = "test",
				Latitude = 48,
				Longitude = 2
			});

			results.Add(new Place
			{
				Name = "plopi",
				Latitude = 87,
				Longitude = 9
			});

			return View(results);
		}

		public static RouteValueDictionary GetRVD(Place place)
		{
			var rvd = new RouteValueDictionary();
			//fill it
			rvd["Name"] = place.Name;
			rvd["Latitude"] = place.Latitude;
			rvd["Longitude"] = place.Longitude;

			return rvd;
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Detail(Place place)
		{
			return View(place);
		}
	}
}
