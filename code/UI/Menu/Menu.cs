using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker;

public class Menu : HudEntity<RootPanel>
{
	public Menu()
	{
		if ( IsClient )
		{
			RootPanel.SetTemplate( "/UI/Menu/Menu.html" );

			RootPanel.BindClass( "visible", () => Input.Down( InputButton.Menu ) );
		}
	}
}
