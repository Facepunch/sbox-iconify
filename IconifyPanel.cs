using System;
using System.IO;
using System.Threading.Tasks;

namespace Sandbox.UI;

[Alias("iconify")]
public class IconifyPanel : Panel
{
	private Texture _svgTexture;

	private bool _dirty = false;
	private string _icon = "";

	public string Icon
	{
		get => _icon;
		set
		{
			_icon = value;
			_dirty = true;
		}
	}

	public IconifyPanel()
	{
		StyleSheet.Parse("""
		IconifyPanel, iconify {
			width: 32px;
			height: 32px;
			align-self: center;
			top: -1px;
		    padding: 0 8px;
		}

		IconifyPanel:first-child, iconify:first-child {
		    padding-left: 0;
		}

		IconifyPanel:last-child, iconify:last-child {
		    padding-right: 0;
		}
		""");
	}

	private (string Pack, string Name) ParseIcon(string icon)
	{
		if (!icon.Contains(':'))
			throw new ArgumentException("_name must be in the format 'pack:name'");

		var splitName = icon.Split(':', StringSplitOptions.RemoveEmptyEntries);

		if (splitName.Length != 2)
			throw new ArgumentException("_name must be in the format 'pack:name'");

		var pack = splitName[0];
		var name = splitName[1];

		return (pack, name);
	}

	public override void OnLayout(ref Rect layoutRect)
	{
		SetIcon();
	}

	public override void SetProperty(string name, string value)
	{
		base.SetProperty(name, value);

		if (name.Equals("icon", StringComparison.OrdinalIgnoreCase))
			Icon = value;
	}

	public override void DrawBackground(ref RenderState state)
	{
		base.DrawBackground(ref state);

		Graphics.Attributes.Set("LayerMat", Matrix.Identity);
		Graphics.Attributes.Set("Texture", _svgTexture);
		Graphics.Attributes.SetCombo("D_BLENDMODE", BlendMode.Normal);
		Graphics.DrawQuad(Box.Rect, Material.UI.Basic, Color.White);
	}

	/// <summary>
	/// Fetches the icon - if it doesn't exist on disk, it will fetch it for you.
	/// </summary>
	private async Task<string> FetchIconAsync(string iconPath)
	{
		var (pack, name) = ParseIcon(iconPath);
		var localPath = $"iconify/{pack}/{name}.svg";

		if (!FileSystem.Data.FileExists(localPath))
		{
			Log.Info($"Cache miss for icon '{iconPath}', fetching from API...");

			var directory = Path.GetDirectoryName(localPath);
			FileSystem.Data.CreateDirectory(directory);

			var remotePath = $"https://api.iconify.design/{pack}/{name}.svg";
			var response = await Http.RequestAsync("GET", remotePath);
			var iconContents = await response.Content.ReadAsStringAsync();
			iconContents = iconContents.Replace(" width=\"1em\" height=\"1em\"", ""); // HACK

			// this API doesn't actually return a 404 status code, so check the document for '404' itself...
			if ( iconContents == "404" )
			{
				Log.Error($"Failed to fetch icon {iconPath}");
				return "";
			}

			FileSystem.Data.WriteAllText(localPath, iconContents);
		}

		return localPath;
	}

	private void SetIcon()
	{
		if (!_dirty)
			return;

		_svgTexture = Texture.White;

		FetchIconAsync(Icon).ContinueWith( task =>
		{
			var basePath = task.Result;
			Log.Info($"Fetched {basePath}");

			var color = Parent?.ComputedStyle?.FontColor?.Hex ?? "#ffffff";
			var width = Box.Rect.Width;
			var height = Box.Rect.Height;
			var pathParams = $"?color={color}&w={width}&h={height}";

			var path = basePath + pathParams;
			_svgTexture = Texture.Load(FileSystem.Data, path);

			_dirty = false;
		});
	}
}