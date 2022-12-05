using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Poker.UI;

[Alias( "radialmenuitem" )]
public class RadialMenuItem : Panel
{
	private Label IconLabel { get; set; }
	private InputHint ActionPanel { get; set; }

	public Action Callback { get; set; }

	public string Name { get; set; }

	public RadialMenuItem()
	{
		AddClass( "item" );

		IconLabel = Add.Label( "", "icon" );
		ActionPanel = AddChild<InputHint>( "action" );
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
			ActionPanel.SetButton( value );
		}
		else if ( name == "name" )
		{
			Name = value;
		}
	}
}
