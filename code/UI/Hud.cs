using Sandbox.UI;

namespace Poker;

public class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( IsClient )
		{
			RootPanel.SetTemplate( "/Code/UI/Hud.html" );
		}
	}
}
