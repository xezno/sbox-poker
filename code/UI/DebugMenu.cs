using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker;

public class DebugMenu : Panel
{
	public DebugMenu()
	{
		StyleSheet.Load( "/UI/DebugMenu.scss" );
		SetClass( "debug-menu", true );

		_ = Add.Label( "Poker Debug", "title" );
		var buttons = Add.Panel( "buttons" );

		{
			buttons.Add.ButtonWithIcon( "Game test macro", "precision_manufacturing", "button",
				() => GameTestMacro() );
			buttons.Add.ButtonWithIcon( "Force start", "sports_esports", "button",
				() => ConsoleSystem.Run( "poker_start" ) );
			buttons.Add.ButtonWithIcon( "Force next player", "refresh", "button",
				() => ConsoleSystem.Run( "poker_force_next_player" ) );
		}

		BindClass( "visible", () => Input.Down( InputButton.View ) );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.View ) )
		{
			CreateEvent( "onopen" );
		}
	}

	[ConCmd.Server( "poker_game_test" )]
	public static void GameTestMacro()
	{
		ConsoleSystem.Run( "bot_add 0 0" );
		ConsoleSystem.Run( "poker_start" );
	}
}
