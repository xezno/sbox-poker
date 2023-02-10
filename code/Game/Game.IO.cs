namespace Poker;
partial class PokerGame
{
	[ConCmd.Admin( "poker_debug_forcewin" )]
	public static void ForceWin()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		Log.Trace( $"Forced {player.Client.Name} as winner" );
		Instance.ProcessWinner( player );
	}

	[ConCmd.Admin( "poker_force_next_player" )]
	public static void ForceNextPlayer()
	{
		if ( !Game.IsServer )
			return;

		var instance = Instance;
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
		if ( !Game.IsServer )
			return;

		var instance = Instance;
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

		switch ( move )
		{
			case Move.Fold:
				_ = player.SetAction( Actions.Game_Fold );

				break;
			case Move.Bet:
				if ( parameter == 0 )
					_ = player.SetAction( Actions.Game_Check );
				else
					_ = player.SetAction( Actions.Game_Bet );

				break;
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

		Log.Trace( $"{player} bets ${parameter}" );
		return true;
	}
}
