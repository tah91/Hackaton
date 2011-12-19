using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Hackaton.Controllers
{
	public class HomeController : Controller
	{
		public static string RequestToken = "https://foursquare.com/oauth2/authenticate?client_id={0}&response_type=code&redirect_uri={1}";
		public static string RequestTokenAccept = "https://foursquare.com/oauth2/access_token?client_id={0}&client_secret={1}&grant_type=authorization_code&redirect_uri={2}&code={3}";
		//public static string TokenKey = string.Empty;

		//public static string ClientId = "VE2DPRCAQ4ILRQRL4ELAIKXD12WTPBA2O0FKARN5Q3KLYKPE";
		//public static string ClientSecret = "D2R4MX2D5KIUBPHSBLLKAKVHSG1WQ1PUKAJHJ5YV3VRYY14J";
		//public static string RedirectUrl = "http://localhost:10659/Home/Token";

		//public static string ClientId = "LEK3IBMSCZP4Y2NBVEQEVGLDNQRVYUDDIOWDNUHCIT2GC2YR";
		//public static string ClientSecret = "1M1SS4IDQNCVWXIKU2JYOP2H55DC0JYWYRPA15BDRQ5W2GQA";
		//public static string RedirectUrl = "http://127.0.0.1/Home/Token";

		public static string ClientId = "DFOFTOSU2VYAO5TKCENWR1PIIRBHY1B4HGEZVSUAI3EIURFA";
		public static string ClientSecret = "HK4W330SMCPM3HRBLHZGCKPQC2GTJRA2XCNXSN3LCCF4Z5JM";
		public static string RedirectUrl = "http://debugeworky.cloudapp.net/Home/Token";

		string GetTokenKey()
		{
			if (Session["4SQTokenKey"] != null)
				return (string)Session["4SQTokenKey"];
			else return string.Empty;
		}

		void SetTokenKey(string key)
		{
			Session["4SQTokenKey"] = key;
		}

		public ActionResult Index()
		{
			if (string.IsNullOrEmpty(GetTokenKey()))
			{
				var redirect = string.Format(RequestToken, ClientId, RedirectUrl);
				return Redirect(redirect);
			}
			ViewBag.Message = "NOMADDICT";
			return View();
		}

		public ActionResult Token(string code)
		{
			var accept = String.Format(RequestTokenAccept, ClientId, ClientSecret, RedirectUrl, code);
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					string textString = client.DownloadString(accept);
					JObject venuesJson = JObject.Parse(textString);
					SetTokenKey((string)venuesJson["access_token"]);
				}
				catch (WebException)
				{
				}
			}
			return RedirectToAction("Index");
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

		public const string eworkySearch = "http://www.eworky.com/api/localisation/search?json=1&maxcount=20";
		public static string FoursquareSearch = "https://api.foursquare.com/v2/venues/search?oauth_token={0}&v=20110917";

		public class VenueSummary
		{
			public double Lat { get; set; }
			public double Lng { get; set; }
			public string Id { get; set; }
			public double Dist { get; set; }
			public int HereNow { get; set; }
		}

		public const double EarthRadius = 6376.5; //kms

		public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
		{
			var dLat1InRad = lat1 * Math.PI / 180.0;
			var dLng1InRad = lng1 * Math.PI / 180.0;
			var dLat2InRad = lat2 * Math.PI / 180.0;
			var dLng2InRad = lng2 * Math.PI / 180.0;

			var dLatitude = dLat2InRad - dLat1InRad;
			var dLongitude = dLng2InRad - dLng1InRad;

			/* Intermediate result a. */
			var a = Math.Pow(Math.Sin(dLatitude / 2.0), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2.0), 2);
			/* Intermediate result c (great circle distance in Radians). */
			var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

			var dDistance = EarthRadius * c;
			return dDistance;
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Results()
		{
			var results = new List<Place>();
			var search = GetSearch(Request.Params);

			//process eworky search
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					var offerType = -1;
					switch(search.OfferType)
					{
						case 0:
							offerType = 0;
							break;
						case 1:
							offerType = 2;
							break;
						case 2:
							offerType = 4;
							break;
						default:
							break;
					}
					var features = string.Empty;
					if (search.Wifi)
						features += 0 + ",";
					if (search.Cafe)
						features += 1 + ",";
					if (search.Resto)
						features += 2 + ",";
					if (search.Parking)
						features += 3 + ",";
					if (search.EasyAccess)
						features += 4 + ",";
					features = features.Trim(',');
					if (!string.IsNullOrEmpty(features))
						features = "[" + features + "]";

					var path = eworkySearch + "&latitude=" + search.Latitude + "&longitude=" + search.Longitude;
					if (offerType != -1)
						path += "&offerType=" + offerType;
					if (!string.IsNullOrEmpty(features))
						path += "&features=" + features;
					string textString = client.DownloadString(path);
					JObject eworkyJson = JObject.Parse(textString);
					var places = eworkyJson["response"];
					foreach (var item in places)
					{
						results.Add(new Place()
						{
							Name = (string)item["name"],
							Latitude = (double)item["latitude"],
							Longitude = (double)item["longitude"],
							EworkyId = (int)item["id"]
						});
					}
				}
				catch (WebException)
				{
				}
			}

			//find nearest 4SQ
			foreach (var item in results)
			{
				using (var client = new WebClient())
				{
					client.Encoding = Encoding.UTF8;
					try
					{
						var path = string.Format(FoursquareSearch, GetTokenKey());
						var query = path + "&ll=" + item.Latitude + "," + item.Longitude + "&limit=20";
						string textString = client.DownloadString(query);
						JObject venuesJson = JObject.Parse(textString);
						var venues = venuesJson["response"]["venues"];
						var distList = new List<VenueSummary>();
						foreach (var venue in venues)
						{
							var lat  = (double)venue["location"]["lat"];
							var lng = (double)venue["location"]["lng"];
							var id = (string)venue["id"];
							var toAdd = new VenueSummary
							{
								Lat = (double)venue["location"]["lat"],
								Lng = (double)venue["location"]["lng"],
								Id = (string)venue["id"],
								HereNow = (int)venue["hereNow"]["count"],
								Dist = GetDistance(item.Latitude, item.Longitude, lat, lng)
							};
							distList.Add(toAdd);
						}
						var nearest = distList.Min(v => v.Dist);
						var nearestVenue = (from v in distList where v.Dist == nearest select v).FirstOrDefault();
						item.FoursquareId = nearestVenue.Id;
						item.FriendCount = nearestVenue.HereNow;
					}
					catch (WebException)
					{
						item.FoursquareId = "";
					}
				}
			}

			return View(results);
		}

		public static RouteValueDictionary GetRVD(Place place)
		{
			var rvd = new RouteValueDictionary();
			//fill it
			rvd["Latitude"] = place.Latitude;
			rvd["Longitude"] = place.Longitude;
			rvd["EworkyId"] = place.EworkyId;
			rvd["FoursquareId"] = place.FoursquareId;
			return rvd;
		}

		public const string eworkyDetail = "http://www.eworky.com/api/localisation/details?json=1";
		public static string FoursquareDetail = "https://api.foursquare.com/v2/venues/{0}?oauth_token={1}&v=20110917";

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Detail(Place place)
		{
			var detail = new PlaceDetail(place);
			//get eworky details
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					var path = eworkyDetail + "&id=" + place.EworkyId;
					string textString = client.DownloadString(path);
					JObject eworkyJson = JObject.Parse(textString);
					var eworkyPlace = eworkyJson["response"];
					detail.Name = (string)eworkyPlace["name"];
					detail.Description= (string)eworkyPlace["description"];
					detail.EworkyImage = (string)eworkyPlace["image"];
					var comments = eworkyPlace["comments"];
					foreach(var com in comments)
					{
						detail.Comments.Add(new Comment
						{
							Author= (string)com["author"]["firstName"],
							Post=(string)com["post"]
						});
					}
				}
				catch (WebException)
				{
				}
			}

			//get 4SQ details
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					var path = string.Format(FoursquareDetail, place.FoursquareId, GetTokenKey());
					string textString = client.DownloadString(path);
					JObject foursquareJson = JObject.Parse(textString);
					var venue = foursquareJson["response"]["venue"];

					var mayor = venue["mayor"]["user"];
					if (mayor != null)
					{
						detail.Mayor = new Mayor
						{
							FirstName = (string)mayor["firstName"],
							LastName = (string)mayor["lastName"],
							Photo = (string)mayor["photo"],
							Gender = (string)mayor["gender"],
						};
					}

					//comments
					var groups = venue["tips"]["groups"];
					if (groups != null)
					{
						foreach (var g in groups)
						{
							var tips = g["items"];
							foreach (var t in tips)
							{
								detail.Comments.Add(new Comment
								{
									Author = (string)t["user"]["firstName"],
									Post = (string)t["text"]
								});
							}
						}
					}

					//categories
					var categories = venue["categories"];
					foreach (var c in categories)
					{
						detail.Categories.Add((string)c["shortName"]);
					}

				}
				catch (WebException)
				{
				}
			}

			return View(detail);
		}

		public static string FoursquareTodo = "https://api.foursquare.com/v2/venues/{0}/marktodo?oauth_token={1}&v=20110917";
		public ActionResult Todo(string id)
		{
			//get 4SQ details
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					var path = string.Format(FoursquareTodo, id, GetTokenKey());
					string textString = client.UploadString(path,"plop");
					JObject foursquareJson = JObject.Parse(textString);

					return Content("<div>Done!</div>");
				}
				catch (WebException)
				{
				}
			}
			return null;
		}

		RouteValueDictionary GetRvd(FunSearch funSearch)
		{
			var rvd = new RouteValueDictionary();
			//fill it
			rvd["Place"] = funSearch.Place;
			rvd["Latitude"] = funSearch.Latitude;
			rvd["Longitude"] = funSearch.Longitude;
			rvd["FunType"] = funSearch.FunType;

			return rvd;
		}

		public static RouteValueDictionary GetRVD(FunPlace place)
		{
			var rvd = new RouteValueDictionary();
			//fill it
			//rvd["Name"] = place.Name;
			rvd["Latitude"] = place.Latitude;
			rvd["Longitude"] = place.Longitude;
			rvd["FriendCount"] = place.FriendCount;
			rvd["FoursquareId"] = place.FoursquareId;
			//rvd["StartTime"] = place.StartTime;
			//rvd["Artists"] = place.Artists;
			return rvd;
		}

		FunSearch GetFunSearch(NameValueCollection collection)
		{
			var funSearch = new FunSearch();
			funSearch.Place = (string)collection["Place"];
			funSearch.Latitude = double.Parse(collection["Latitude"]);
			funSearch.Longitude = double.Parse(collection["Longitude"]);
			funSearch.FunType = int.Parse(collection["FunType"]);

			return funSearch;
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult FunSearch()
		{
			return View(new FunSearch());
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult FunSearch(FunSearch funSearch)
		{
			var rvd = GetRvd(funSearch);
			return RedirectToAction("FunResults", rvd);
		}

		public static string FoursquareFunSearch = "https://api.foursquare.com/v2/venues/search?oauth_token={0}&v=20110917&limit=20";
		public static string Songkick = "http://api.songkick.com/api/3.0/events.json?location=geo:{0}&apikey=hackday";

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult FunResults()
		{
			var results = new List<FunPlace>();
			var funSearch = GetFunSearch(Request.Params);

			var today = DateTime.Now;
			//case concert :
			if (funSearch.FunType == 3)
			{
				using (var client = new WebClient())
				{
					client.Encoding = Encoding.UTF8;
					try
					{
						var location = funSearch.Latitude + "," + funSearch.Longitude;
						var path = string.Format(Songkick, location);
						string textString = client.DownloadString(path);
						JObject songKickJson = JObject.Parse(textString);
						var concerts = songKickJson["resultsPage"]["results"]["event"];
						foreach (var c in concerts)
						{
							try
							{
								var dateStr = (string)c["start"]["date"];
								var date = DateTime.Parse(dateStr);
								if (today.Date != date.Date)
									continue;
								var toAdd = new FunPlace()
								{
									Name = (string)c["displayName"],
									Latitude = (double)c["location"]["lat"],
									Longitude = (double)c["location"]["lng"],
									StartTime = (string)c["start"]["time"]
								};
								var performances = c["performance"];
								foreach (var p in performances)
								{
									toAdd.Artists += p["displayName"] + ",";
								}
								toAdd.Artists = toAdd.Artists.TrimEnd(',');
								Session["SKName"] = (string)c["displayName"];
								Session["SKStartTime"] = (string)c["start"]["time"];
								Session["SKArtists"] = toAdd.Artists;
								results.Add(toAdd);
							}
							catch (Exception)
							{
								continue;
							}
						}
					}
					catch (WebException)
					{
					}
				}
				//find nearest 4SQ
				foreach (var item in results)
				{
					using (var client = new WebClient())
					{
						client.Encoding = Encoding.UTF8;
						try
						{
							var path = string.Format(FoursquareSearch, GetTokenKey());
							var query = path + "&ll=" + item.Latitude + "," + item.Longitude + "&limit=20";
							string textString = client.DownloadString(query);
							JObject venuesJson = JObject.Parse(textString);
							var venues = venuesJson["response"]["venues"];
							var distList = new List<VenueSummary>();
							foreach (var venue in venues)
							{
								var lat = (double)venue["location"]["lat"];
								var lng = (double)venue["location"]["lng"];
								var id = (string)venue["id"];
								var toAdd = new VenueSummary
								{
									Lat = (double)venue["location"]["lat"],
									Lng = (double)venue["location"]["lng"],
									Id = (string)venue["id"],
									HereNow = (int)venue["hereNow"]["count"],
									Dist = GetDistance(item.Latitude, item.Longitude, lat, lng)
								};
								distList.Add(toAdd);
							}
							var nearest = distList.Min(v => v.Dist);
							var nearestVenue = (from v in distList where v.Dist == nearest select v).FirstOrDefault();
							item.FoursquareId = nearestVenue.Id;
							item.FriendCount = nearestVenue.HereNow;
						}
						catch (WebException)
						{
							item.FoursquareId = "";
						}
					}
				}
			}
			else
			{
				using (var client = new WebClient())
				{
					client.Encoding = Encoding.UTF8;
					try
					{
						var path = string.Format(FoursquareSearch, GetTokenKey());
						var query = path + "&ll=" + funSearch.Latitude + "," + funSearch.Longitude;// +"&limit=20";
						var toMatch = string.Empty;
						switch(funSearch.FunType)
						{
							case 0:
								toMatch = "Musée";
								break;
							case 1:
								toMatch = "Restaurant";
								break;
							case 2:
								toMatch = "Vie nocturne";
								break;
							default:
								break;
						}
						//query += "&query=" + filter;
						string textString = client.DownloadString(query);
						JObject venuesJson = JObject.Parse(textString);
						var venues = venuesJson["response"]["venues"];
						foreach (var v in venues)
						{
							try
							{
								var categories = v["categories"];
								var found = false;
								foreach (var c in categories)
								{
									var cat  = (string)c["name"];
									if (cat.Contains(toMatch) || (string.Compare((string)c["name"], toMatch, true) == 0))
									{
										found = true;
										break;
									}
								}
								if (found == false)
									continue;
								var toAdd = new FunPlace()
								{
									Name = (string)v["name"],
									Latitude = (double)v["location"]["lat"],
									Longitude = (double)v["location"]["lng"],
									FoursquareId = (string)v["id"],
									FriendCount = (int)v["hereNow"]["count"]
								};
								results.Add(toAdd);
							}
							catch (Exception)
							{
								continue;
							}
						}
					}
					catch (WebException)
					{
					}
				}
			}


			return View(results);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult FunDetail(FunPlace place)
		{
			var detail = new FunPlaceDetail(place);
			detail.Name= (string)Session["SKName"];
			detail.StartTime = (string)Session["SKStartTime"];
			detail.Artists = (string)Session["SKArtists"];
			//get 4SQ details
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				try
				{
					var path = string.Format(FoursquareDetail, place.FoursquareId, GetTokenKey());
					string textString = client.DownloadString(path);
					JObject foursquareJson = JObject.Parse(textString);
					var venue = foursquareJson["response"]["venue"];

					var mayor = venue["mayor"]["user"];
					if (mayor != null)
					{
						detail.Mayor = new Mayor
						{
							FirstName = (string)mayor["firstName"],
							LastName = (string)mayor["lastName"],
							Photo = (string)mayor["photo"],
							Gender = (string)mayor["gender"],
						};
					}

					//comments
					var groups = venue["tips"]["groups"];
					foreach (var g in groups)
					{
						var tips = g["items"];
						foreach (var t in tips)
						{
							detail.Comments.Add(new Comment
							{
								Author = (string)t["user"]["firstName"],
								Post = (string)t["text"]
							});
						}
					}

					//categories
					var categories = venue["categories"];
					foreach (var c in categories)
					{
						detail.Categories.Add((string)c["shortName"]);
					}

					var photos = venue["photos"]["groups"];
					foreach (var p in photos)
					{
						var items = p["items"];
						foreach (var i in items)
						{
							var url = (string)i["url"];
							if (!string.IsNullOrEmpty(url))
							{
								detail.Image = url;
								break;
							}
						}
					}
				}
				catch (WebException)
				{
				}
			}

			return View(detail);
		}
	}
}
