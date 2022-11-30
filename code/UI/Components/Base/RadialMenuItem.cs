using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Poker.UI;

[Alias( "radialmenuitem" )]
public class RadialMenuItem : Panel
{
	private Label IconLabel { get; set; }
	private Panel ActionPanel { get; set; }

	public Action Callback { get; set; }

	public string Name { get; set; }

	public RadialMenuItem()
	{
		AddClass( "icon" );

		IconLabel = Add.Label( "", "icon" );
		ActionPanel = Add.Panel( "action" );
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "icon" )
		{
			IconLabel.Text = value;
		}
		else if ( name == "action" )
		{
			Log.Trace( $"TODO: set action to {value}" );
		}
		else if ( name == "name" )
		{
			Name = value;
		}
	}
}
