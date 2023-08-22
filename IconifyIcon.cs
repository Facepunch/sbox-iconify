using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.UI;

public struct IconifyIcon
{
	public string Prefix { get; private set; }
	public string Name { get; private set; }

	public bool IsTintable { get; private set; }

	private readonly string Url => $"https://api.iconify.design/{Prefix}/{Name}.svg";
	private readonly string LocalPath => $"iconify/{Prefix}/{Name}.svg";

	private async Task<string> FetchImageDataAsync()
	{
		Log.Info( $"FetchImageData" );

		var response = await Http.RequestAsync( "GET", Url );
		var iconContents = await response.Content.ReadAsStringAsync();

		// this API doesn't actually return a 404 status code :( check the document for '404' itself...
		if ( response.StatusCode == HttpStatusCode.NotFound || iconContents == "404" )
			throw new Exception( $"Failed to fetch icon {this}" );

		iconContents = RemoveHardCodedDimensions( iconContents );
		return iconContents;
	}

	private string RemoveHardCodedDimensions( string content )
	{
		// HACK: the API always returns an SVG with a hard-coded width/height, we don't want that right now
		return content.Replace( " width=\"1em\" height=\"1em\"", "" );
	}

	public async Task EnsureIconDataIsCachedAsync()
	{
		if ( !FileSystem.Data.FileExists( LocalPath ) )
		{
			Log.Info( $"Cache miss for icon '{this}', fetching from API..." );

			var directory = Path.GetDirectoryName( LocalPath );
			FileSystem.Data.CreateDirectory( directory );

			var iconContents = await FetchImageDataAsync();
			FileSystem.Data.WriteAllText( LocalPath, iconContents );
		}
	}

	public async Task<Texture> LoadTextureAsync( Rect rect, Color? tintColor )
	{
		await EnsureIconDataIsCachedAsync();

		// HACK: Check whether this icon is tintable based on whether it references CSS currentColor
		var imageData = FileSystem.Data.ReadAllText( LocalPath );
		IsTintable = imageData.Contains( "currentColor" );

		var pathParams = BuildPathParams( rect, tintColor );
		var path = LocalPath + pathParams;

		Log.Info( $"Fetching {path}" );
		return Texture.Load( FileSystem.Data, path );
	}

	private string BuildPathParams( Rect rect, Color? tintColor )
	{
		var pathParamsBuilder = new StringBuilder( "?" );

		if ( IsTintable && tintColor.HasValue )
			pathParamsBuilder.Append( $"color={tintColor.Value.Hex}&" );

		var width = Math.Max( 32, rect.Width );
		var height = Math.Max( 32, rect.Height );

		pathParamsBuilder.Append( $"w={width}&h={height}" );
		return pathParamsBuilder.ToString();
	}
	
	public IconifyIcon( string path )
	{
		if ( !path.Contains( ':' ) )
			throw new ArgumentException( $"Icon must be in the format 'prefix:name', got '{path}'" );

		var splitName = path.Split( ':', StringSplitOptions.RemoveEmptyEntries );

		if ( splitName.Length != 2 )
			throw new ArgumentException( $"Icon must be in the format 'prefix:name', got '{path}'" );

		Prefix = splitName[0].Trim();
		Name = splitName[1].Trim();
	}

	public static implicit operator IconifyIcon( string path ) => new IconifyIcon( path );

	public override string ToString()
	{
		return $"{Prefix}:{Name}";
	}
}
