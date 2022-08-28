using Poker.UI;
using Sandbox;

namespace Poker.Backend;

partial class PokerController
{
	[ConCmd.Admin( "poker_debug_forcewin" )]
	public static void ForceWin()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		Log.Trace( $"Forced {player.Client.Name} as winner" );
		PokerController.Instance.ProcessWinner( player );
	}
	
	[ConCmd.Admin( "poker_force_next_player" )]
	public static void ForceNextPlayer()
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerController.Instance;
		if ( instance == null )
			Log.Warning( "Instance was null!" );

		var caller = ConsoleSystem.Caller;
		var player = caller.Pawn as Player;

		if ( player == null )
			Log.Warning( "Player was null!" );

		Log.Trace( $"Forced to next player" );

		instance.Bet( instance.MinimumBet, instance.PlayerTurnQueue.Peek() );
		instance.MoveToNextPlayer();
	}

	[ConCmd.Server( "poker_submit_move" )]
	public static void CmdSubmitMove( Move move, float parameter )
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerController.Instance;
		if ( instance == null )
			Log.Error( "Instance was null!" );

		var caller = ConsoleSystem.Caller;
		var player = caller.Pawn as Player;

		if ( player == null )
			Log.Error( "Player was null!" );

		Log.Trace( $"Player {player.Name} ({caller.Name}) submitted move {move} with param {parameter}" );

		if ( instance.IsTurn( player ) )
		{
			switch ( move )
			{
				case Move.Fold:
					{
						instance.Fold( player );
						break;
					}
				case Move.Bet:
					{
						instance.Bet( parameter, player );
						break;
					}
			}

			instance.MoveToNextPlayer();
		}
	}

	private void Fold( Player player )
	{
		Log.Trace( $"{player.Client.Name} folds" );

		player.HasFolded = true;
	}

	private bool Bet( float parameter, Player player )
	{
		if ( player.Money < parameter )
			return false;

		parameter = parameter.Clamp( 0, int.MaxValue );

		if ( MinimumBet < parameter )
			MinimumBet = parameter;

		Pot += parameter;
		player.LastBet = parameter;
		player.Money -= parameter;

		Log.Trace( $"{player.Client.Name} bets ${parameter}" );
		return true;
	}
}
