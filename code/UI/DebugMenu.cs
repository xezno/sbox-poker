using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker;

public class DebugMenu : Panel
{
	[ConVar.Replicated( "poker_debug_enabled" )]
	public static bool Enabled { get; set; } = false;

	public DebugMenu()
	{
		SetClass( "debug-menu", true );

		_ = Add.Label( "Poker Debug", "title" );
		var buttons = Add.Panel( "buttons" );

		{
			buttons.Add.ButtonWithIcon( "Game test macro", "vial", "button",
				() => GameTestMacro() );

			buttons.Add.ButtonWithIcon( "Force next player", "angles-right", "button",
				() => ConsoleSystem.Run( "poker_force_next_player" ) );

			buttons.Add.ButtonWithIcon( "Force Win", "trophy", "button",
				() => ConsoleSystem.Run( "poker_debug_forcewin" ) );
		}

		BindClass( "visible", () => Input.Down( InputButton.Menu ) && Enabled );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.Menu ) )
		{
			CreateEvent( "onopen" );
		}
	}

	[ConCmd.Admin( "poker_game_test" )]
	public static void GameTestMacro()
	{
		if ( !DebugMenu.Enabled )
			return;

		ConsoleSystem.Run( "bot_add 0 0" );
		ConsoleSystem.Run( "poker_start" );
	}
}
