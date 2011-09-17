
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

public class Place
{
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
}