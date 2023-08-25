using System;

namespace Sandbox.UI;

[Alias( "iconify", "iconify-icon" )]
public class IconifyPanel : Panel
{
	public static readonly BaseFileSystem DefaultCache;

	private Texture _svgTexture;

	private bool _dirty = false;
	private string _icon = "";

	public string Icon
	{
		get => _icon;
		set
		{
			if ( _icon == value )
				return;

			_icon = value;
			_dirty = true;
		}
	}

	static IconifyPanel()
	{
		if ( FileSystem.Data is null )
			return;

		FileSystem.Data.CreateDirectory( "iconify" );
		DefaultCache = FileSystem.Data.CreateSubSystem( "iconify" );
	}

	public IconifyPanel()
	{
		StyleSheet.Parse( """
		IconifyPanel, iconify, iconify-icon {
			height: 16px;
			aspect-ratio: 1;
			align-self: center;
		    padding: 0 8px;
		}

		IconifyPanel:first-child, iconify:first-child {
		    padding-left: 0;
		}

		IconifyPanel:last-child, iconify:last-child {
		    padding-right: 0;
		}
		""" );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		SetIcon();
	}

	public override void OnLayout( ref Rect layoutRect )
	{
		_dirty = true;
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name.Equals( "icon", StringComparison.OrdinalIgnoreCase ) || name.Equals( "name", StringComparison.OrdinalIgnoreCase ) )
			Icon = value;
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		Graphics.Attributes.Set( "LayerMat", Matrix.Identity );
		Graphics.Attributes.Set( "Texture", _svgTexture );
		Graphics.Attributes.SetCombo( "D_BLENDMODE", BlendMode.Normal );
		Graphics.DrawQuad( Box.Rect, Material.UI.Basic, Color.White );
	}

	private void SetIcon()
	{
		if ( !_dirty )
			return;

		_dirty = false;
		_svgTexture = Texture.White;

		var icon = new IconifyIcon( _icon );
		var rect = Box.Rect;
		var tintColor = ComputedStyle?.FontColor;
		
		icon.LoadTextureAsync( rect, tintColor ).ContinueWith( ( task ) =>
		{
			_svgTexture = task.Result;
		} );
	}
}
