
using System.Collections.Generic;
public class Search
{
	public string Place { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public int OfferType { get; set; }
	public bool Wifi { get; set; }
	public bool Cafe { get; set; }
	public bool Resto { get; set; }
	public bool Parking { get; set; }
	public bool EasyAccess { get; set; }
}

public class FunSearch
{
	public string Place { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public int FunType { get; set; }
}

public class Place
{
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string Category { get; set; }
	public int FriendCount { get; set; }
	public int EworkyId { get; set; }
	public string FoursquareId { get; set; }
}

public class FunPlace
{
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string StartTime { get; set; }
	public string Artists { get; set; }
	public int FriendCount { get; set; }
	public string FoursquareId { get; set; }
}

public class Comment
{
	public string Author { get; set; }
	public string Post { get; set; }
}

public class Mayor
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Photo { get; set; }
	public string Gender { get; set; }
}

public class PlaceDetail
{
	public PlaceDetail(Place place)
	{
		EworkyId = place.EworkyId;
		FoursquareId = place.FoursquareId;
		Name = place.Name;
		Latitude = place.Latitude;
		Longitude = place.Longitude;
		Category = place.Category;
		FriendCount = place.FriendCount;
		Comments = new List<Comment>();
		Categories = new List<string>();
	}

	public int EworkyId { get; set; }
	public string FoursquareId { get; set; }
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string Description { get; set; }
	public string EworkyImage { get; set; }
	public string Category { get; set; }
	public int FriendCount { get; set; }
	public List<Comment> Comments { get; set; }
	public List<string> Categories { get; set; }
	public Mayor Mayor { get; set; }
}

public class FunPlaceDetail
{
	public FunPlaceDetail(FunPlace place)
	{
		FoursquareId = place.FoursquareId;
		Name = place.Name;
		Latitude = place.Latitude;
		Longitude = place.Longitude;
		FriendCount = place.FriendCount;
		Comments = new List<Comment>();
		Categories = new List<string>();
		StartTime = place.StartTime;
		Artists = place.Artists;
	}

	public string StartTime { get; set; }
	public string Artists { get; set; }
	public string FoursquareId { get; set; }
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string Description { get; set; }
	public string Image { get; set; }
	public string Category { get; set; }
	public int FriendCount { get; set; }
	public List<Comment> Comments { get; set; }
	public List<string> Categories { get; set; }
	public Mayor Mayor { get; set; }
}