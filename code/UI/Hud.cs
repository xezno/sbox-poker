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

			LoadStyleSheet();
		}
	}

	private void LoadStyleSheet()
	{
		var fileName = "/Code/UI/Styles/Hud.scss";
		var variables = new List<(string key, string value)>()
		{
			( "accent", "#ff00ff" ),
			( "background", "#00ff00" ),
			( "background-dark", "#00ff00" ),
			( "background-gradient", "linear-gradient(to top, rgba( #ff00ff, 0.75 ), rgba( #ff00ff, 0.0 ))" ),
		};

		RootPanel.StyleSheet.Add( StyleSheet.FromFile( fileName, variables ) );
	}
}
