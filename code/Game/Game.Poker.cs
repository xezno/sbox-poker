using Poker.UI;

namespace Poker;
partial class PokerGame
{
	// TODO: Buy-ins
	// List of players that have bought in
	private List<Player> Players => Entity.All.OfType<Player>().Where( x => !x.HasFolded ).ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	[ConVar.Server( "poker_sv_blind_amount" )]
	public static float BlindAmount { get; set; } = 50f;

	[Net] public bool IsRunning { get; set; }

	private TimeUntil _timeUntilNextRound = 0;

	public enum Rounds
	{
		Preflop,
		Flop,
		Turn,
		River,
		Showdown,

		End
	}

	public Rounds Round { get; private set; } = Rounds.Preflop;

	private void Reset()
	{
		// Instantiate everything
		Deck = new();
		PlayerTurnQueue = new();
		CommunityCards.Clear();
		Round = Rounds.Preflop;
		Pot = 0;
		IsRunning = true;

		// Reset players
		Entity.All.OfType<Player>().ToList().ForEach( player => player.Reset() );
	}

	public void Run()
	{
		Game.AssertServer();

		if ( _timeUntilNextRound > 0 )
		{
			Log.Trace( _timeUntilNextRound );
			return;
		}

		/*
		 * TODO:
		 * - Buy-in
		 */
		// Reset all
		Reset();

		// Determine dealer
		Dealer = Players[0];

		// Determine small blind & big blind
		// TODO: This shouldn't count spectators or players that haven't bought in yet
		if ( Players.Count < 2 )
		{
			Log.Error( "Not enough players." );
			return;
		}
		else if ( Players.Count == 2 )
		{
			// Small blind is dealer, big blind is other player
			SmallBlind = Dealer;
			BigBlind = Players[1];
		}
		else
		{
			// Small blind is clockwise to dealer, big blind is clockwise to SB
			SmallBlind = Players[1];
			BigBlind = Players[2];
		}

		Log.Trace( $"Selected:" );
		Log.Trace( $"\t- {Dealer} as the dealer" );
		Log.Trace( $"\t- {SmallBlind} as small blind" );
		Log.Trace( $"\t- {BigBlind} as big blind" );

		// Take blinds
		if ( !Bet( BlindAmount * 0.5f, SmallBlind ) )
		{
			/*
			 * TODO:
				When a player's stack is less than the amount of the small blind, they are 
				automatically considered all-in in the next hand they play, regardless of position.
				If the player's stack is larger than the small blind but smaller than the big blind,
				they will be considered all-in in any position other than the small blind, assuming
				they fold for their option.
				When all-in, the player can only win the amount of their stack, plus that same 
				amount from all of the callers and blinds. If the person has less than the big 
				blind, they can only win the portion of the blind equal to that of their stack.
			*/
		}

		if ( !Bet( BlindAmount, BigBlind ) )
		{
			// (same as above)
		}

		// Give each player two hole cards
		Players.ForEach( player =>
		{
			player.Hand = Deck.CreateHand();

			var cardData = RpcUtils.Compress( player.Hand );
			player.RpcSetHand( To.Single( player ), cardData );

			player.LeftCard.RpcSetCard( To.Single( player ), player.Hand[0] );
			player.RightCard.RpcSetCard( To.Single( player ), player.Hand[1] );
		} );

		// Kick broke people
		Players.ForEach( player =>
		{
			if ( player.Money <= 0 )
				PokerGame.Instance.CreateSpectatorFor( player.Client );
		} );

		// OK now everyone needs to pay the ante
		Players.ForEach( player =>
		{
			if ( !Bet( 5, player ) )
				PokerGame.Instance.CreateSpectatorFor( player.Client );
		} );

		// We're ready, broadcast event
		Event.Run( PokerEvent.GameStart.Name );

		// Start pre-flop
		StartNextRound();
	}

	private void StartNextRound()
	{
		StartRound( Round++ );
	}

	private void AddCommunityCards( int count )
	{
		var cards = Deck.Draw( count );
		var cardsStr = string.Join( ", ", cards.Select( x => x.ToShortString() ) );
		Log.Trace( $"Dealt community cards: {cardsStr}" );

		foreach ( var card in cards )
			CommunityCards.Add( card );

		Event.Run( PokerEvent.CommunityCardsUpdated.Name );
	}

	private void StartRound( Rounds round )
	{
		switch ( round )
		{
			case Rounds.Preflop:
				break;
			case Rounds.Flop:
				MinimumBet = 0;
				AddCommunityCards( 3 );
				break;
			case Rounds.Turn:
			case Rounds.River:
				MinimumBet = 0;
				AddCommunityCards( 1 );
				break;
		}

		PlayerTurnQueue.CreateQueue( Players );

		Event.Run( PokerEvent.NewRound.Name );

		if ( Round >= Rounds.Showdown || PlayerTurnQueue.Count <= 1 )
		{
			Round = Rounds.End;
			IsRunning = false;
			_timeUntilNextRound = 10;

			var winner = FindWinner( out var rank );
			ProcessWinner( winner );

			WinnerScreen.OnWin( To.Everyone, winner.Client.Name, winner.Client.SteamId, Pot.CeilToInt(), rank );
			Event.Run( PokerEvent.GameOver.Name );
		}
	}

	private void ProcessWinner( Player winner )
	{
		winner.Money += Pot;
	}

	private void MoveToNextPlayer()
	{
		PlayerTurnQueue.Pop();

		if ( PlayerTurnQueue.Count == 0 )
		{
			// Reached the end of this round; let's move to the next one
			StartNextRound();
			return;
		}

		// TODO: Should probably be removing players from the turn queue if they fold.
		if ( PlayerTurnQueue.Peek().HasFolded )
		{
			MoveToNextPlayer();
			return;
		}

		Event.Run( PokerEvent.TurnChange.Name );
	}

	public bool IsTurn( Player player )
	{
		return IsRunning && Round != Rounds.End && PlayerTurnQueue?.Peek() == player;
	}
}
