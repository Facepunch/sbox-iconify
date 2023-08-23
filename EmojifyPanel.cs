using Sandbox.Razor;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sandbox.UI;

public struct ParsedResult
{
	public bool IsEmoji { get; set; }
	public string Text { get; set; }
}

[Alias( "emojify" )]
public class EmojifyPanel : Panel
{
	private bool _dirty = false;
	private string _prefix = "";

	private string _text = "";

	public string Text
	{
		get => _text;
		set
		{
			if ( _text == value )
				return;

			Log.Info( "Text changed" );

			_text = value;
			_dirty = true;
		}
	}

	public string Prefix
	{
		get => _prefix;
		set
		{
			if ( _prefix == value )
				return;

			Log.Info( "Prefix changed" );

			_prefix = value;
			_dirty = true;
		}
	}

	public EmojifyPanel()
	{
		StyleSheet.Parse( """
		EmojifyPanel, emojify {
			aspect-ratio: 1;
			align-self: center;
		    padding: 0 8px;

			align-items: center;
		}

		EmojifyPanel:first-child, iconify:first-child {
		    padding-left: 0;
		}

		EmojifyPanel:last-child, iconify:last-child {
		    padding-right: 0;
		}
		""" );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		ParseText();
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name.Equals( "prefix", StringComparison.OrdinalIgnoreCase ) )
			Prefix = value;
	}

	private static List<ParsedResult> ParseString( string input )
	{
		List<ParsedResult> results = new();

		var regex = new Regex( @":([\w-]+):|([^:]+)" );

		foreach ( Match match in regex.Matches( input ).Cast<Match>() )
		{
			if ( match.Groups[1].Success )
				results.Add( new ParsedResult { IsEmoji = true, Text = match.Groups[1].Value } );
			else
				results.Add( new ParsedResult { IsEmoji = false, Text = match.Groups[2].Value } );
		}

		return results;
	}

	private void ParseText()
	{
		if ( !_dirty )
			return;
		
		Log.Info( $"{_dirty}" );

		_dirty = false;
		
		DeleteChildren( true );
		var parseResult = ParseString( Text );

		foreach ( var element in parseResult )
		{
			if ( element.IsEmoji )
			{
				Log.Info( $"{Prefix}:{element.Text}" );

				AddChild( new IconifyPanel() { Icon = $"{Prefix}:{element.Text}" } );
			}
			else
			{
				Log.Info( $"{element.Text}" );
				Add.Label( element.Text );
			}
		}
	}
}
