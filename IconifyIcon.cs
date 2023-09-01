using Iconify;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sandbox.UI;

public struct IconifyIcon
{
	public const int CurrentVersion = 1;

	public string Prefix { get; private set; }
	public string Name { get; private set; }

	public bool IsTintable { get; private set; }

	private readonly string Url => $"https://api.iconify.design/{Prefix}/{Name}.svg?width=100%";
	private readonly string LocalPath => $"{Prefix}/{Name}.svg";
	private readonly string MetadataPath => $"{Prefix}/{Name}.json";

	private async Task<string> FetchImageDataAsync()
	{
		var response = await Http.RequestAsync( Url, "GET" );
		var iconContents = await response.Content.ReadAsStringAsync();

		Log.Trace( Url );

		// this API doesn't actually return a 404 status code :( check the document for '404' itself...
		if ( response.StatusCode == HttpStatusCode.NotFound || iconContents == "404" )
			throw new Exception( $"Failed to fetch icon {this}" );
		
		return iconContents;
	}

	private string FetchMetadata()
	{
		var metadata = new IconMetadata()
		{
			Version = CurrentVersion,
			TimeFetched = DateTime.Now
		};

		return Json.Serialize( metadata );
	}

	public async Task EnsureIconDataIsCachedAsync( BaseFileSystem fs )
	{
		bool shouldFetch = !fs.FileExists( LocalPath );
		
		if ( fs.FileExists( MetadataPath ) )
		{
			var metadata = fs.ReadJson<IconMetadata>( MetadataPath );
			shouldFetch &= (metadata.Version != CurrentVersion);
		}
		else
		{
			shouldFetch = true;
		}

		if ( shouldFetch )
		{
			var directory = Path.GetDirectoryName( LocalPath );
			fs.CreateDirectory( directory );

			var iconContents = await FetchImageDataAsync();
			fs.WriteAllText( LocalPath, iconContents );

			var metadataContents = FetchMetadata();
			fs.WriteAllText( MetadataPath, metadataContents );
		}
	}

	public async Task<Texture> LoadTextureAsync( Rect rect, Color? tintColor )
	{
		var fs = IconifyOptions.Current.CacheFileSystem;
		await EnsureIconDataIsCachedAsync( fs );

		// HACK: Check whether this icon is tintable based on whether it references CSS currentColor
		var imageData = await fs.ReadAllTextAsync( LocalPath );
		IsTintable = imageData.Contains( "currentColor" );

		var pathParams = BuildPathParams( rect, tintColor );
		var path = LocalPath + pathParams;
    
		return Texture.Load( fs, path );
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
