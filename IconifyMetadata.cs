using System.Collections.Generic;

public class Author
{
	public string name { get; set; }
	public string url { get; set; }
}

public class Info
{
	public string name { get; set; }
	public int total { get; set; }
	public Author author { get; set; }
	public License license { get; set; }
	public List<string> samples { get; set; }
	public int height { get; set; }
	public int displayHeight { get; set; }
	public string category { get; set; }
	public bool palette { get; set; }
}

public class License
{
	public string title { get; set; }
	public string spdx { get; set; }
	public string url { get; set; }
}

public class IconifyMetadata
{
	public string prefix { get; set; }
	public Info info { get; set; }
	public int lastModified { get; set; }
}
