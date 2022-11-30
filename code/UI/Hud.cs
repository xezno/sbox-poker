using Sandbox.UI;

namespace Poker;

public class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( IsClient )
		{
			RootPanel.SetTemplate( "/Code/UI/Hud.html" );

			RootPanel.BindClass( "is-spectator", () =>
			{
				return Local.Pawn is Spectator;
			} );

			RootPanel.BindClass( "is-player", () =>
			{
				return Local.Pawn is Player;
			} );
		}
	}
}
