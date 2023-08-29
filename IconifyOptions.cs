using Sandbox;

namespace Iconify;

/// <summary>
/// Contains all configurable options in the Iconify library.
/// </summary>
public class IconifyOptions
{
	/// <summary>
	/// The instance of <see cref="IconifyOptions"/> that the library will use.
	/// </summary>
	public static IconifyOptions Current { get; set; }

	/// <summary>
	/// The file system that cached icons will be written to. 
	/// </summary>
	public BaseFileSystem CacheFileSystem { get; set; } = FileSystem.Data;

	static IconifyOptions()
	{
		if ( FileSystem.Data is null )
			return;

		FileSystem.Data.CreateDirectory( "iconify" );
		Current = new IconifyOptions()
			.WithCacheFileSystem( FileSystem.Data.CreateSubSystem( "iconify" ) );
	}

	/// <summary>
	/// Sets the <see cref="CacheFileSystem"/> property.
	/// </summary>
	/// <param name="fs">The file system that cached icons will be written to.</param>
	/// <returns>The same instance of <see cref="IconifyOptions"/>.</returns>
	public IconifyOptions WithCacheFileSystem( BaseFileSystem fs )
	{
		CacheFileSystem = fs;
		return this;
	}
}
