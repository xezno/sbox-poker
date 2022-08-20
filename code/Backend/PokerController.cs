using Sandbox;
using SandboxEditor;
using Poker.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Poker.Backend;

public partial class PokerController
{
	private static PokerController instance;

	public static PokerController Instance
	{
		get
		{
			Host.AssertServer();
			return instance;
		}
		set
		{
			Host.AssertServer();
			instance = value;
		}
	}

	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	private Queue<Player> PlayerTurnQueue { get; set; }

	public IList<Card> CommunityCards { get => Game.Instance.CommunityCards; set => Game.Instance.CommunityCards = value; }
	public float Pot { get => Game.Instance.Pot; set => Game.Instance.Pot = value; }
	public float MinimumBet { get => Game.Instance.MinimumBet; set => Game.Instance.MinimumBet = value; }

	public enum Rounds
	{
		Preflop,
		Flop,
		Turn,
		River,
		Showdown,

		End
	}

	private Rounds Round { get; set; } = Rounds.Preflop;

	public PokerController()
	{
		Host.AssertServer();

		Instance = this;

		Event.Register( this );
	}

	~PokerController()
	{
		Event.Unregister( this );
	}

	public void Run()
	{
		/*
		 * TODO:
		 * - Buy-in
		 */

		// Instantiate everything
		Deck = new();
		PlayerTurnQueue = new();
		CommunityCards.Clear();
		Round = Rounds.Preflop;

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

		float blind = 50;

		// Check if blinds can afford it
		// TODO: This should probaly be on Bet() - if they can't afford it, return false, and we'll deal with it where it gets called
		if ( SmallBlind.Money < blind * 0.5f )
		{
			SmallBlind.Client.Kick(); // TODO: Move to spectators
			Run();
		}

		if ( BigBlind.Money < blind )
		{
			BigBlind.Client.Kick(); // TODO: Move to spectators
			Run();
		}

		// Take blinds
		Bet( blind * 0.5f, SmallBlind );
		Bet( blind, BigBlind );

		// Give each player two hole cards
		Players.ForEach( player =>
		{
			player.Hand = Deck.CreateHand();

			var cardData = RpcUtils.Compress( player.Hand );
			player.RpcSetHand( To.Single( player ), cardData );

			player.LeftCard.RpcSetCard( To.Single( player ), player.Hand[0] );
			player.RightCard.RpcSetCard( To.Single( player ), player.Hand[1] );
		} );

		// Reset players.. TODO: should probably be a function on Player itself
		Players.ForEach( player =>
		{
			player.HasFolded = false;
		} );

		// Delete chips
		// TODO: Move this elsewhere
		Entity.All.OfType<ChipEntity>().ToList().ForEach( x => x.Delete() );
		Entity.All.OfType<ChipStackEntity>().ToList().ForEach( x => x.Delete() );

		// Spawn chips
		// TODO: Move this elsewhere and do it properly
		Players.ForEach( player =>
		{
			var seat = player.Seat;
			var chipSpawn = Entity.All.OfType<PlayerChipSpawn>().First( x => x.SeatNumber == seat.SeatNumber );
			float spacing = 2.5f;

			var chipOffsets = new (int, float, Vector3)[]
			{
				// Count, Value, Position
				new( 2, 500, Vector3.Zero ),
				new( 5, 250, new Vector3( 0,  spacing  ) * player.Rotation ),
				new( 7, 100, new Vector3( spacing, spacing ) * player.Rotation ),
				new( 10, 50, new Vector3( 0, spacing*2 ) * player.Rotation )
			};

			foreach ( var offset in chipOffsets )
			{
				ChipStackEntity.CreateStack( offset.Item1, offset.Item2, chipSpawn.Position + offset.Item3 );
			}
		} );

		// Kick broke people
		Players.ForEach( player =>
		{
			if ( player.Money <= 0 )
				player.Client.Kick(); // TODO: Move to spectators
		} );

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
		EventFeed.AddEvent( To.Everyone, $"Dealt community cards: {cardsStr}" );

		foreach ( var card in cards )
			CommunityCards.Add( card );
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

		Players.ForEach( player =>
		{
			if ( !player.HasFolded )
				PlayerTurnQueue.Enqueue( player );
		} );

		if ( Round >= Rounds.Showdown || PlayerTurnQueue.Count <= 1 )
		{
			Log.Info( "Game over" );

			var winner = FindWinner();
			winner.Money += Pot;
			Pot = 0;

			Run();

			return;
		}
	}

	private void MoveToNextPlayer()
	{
		PlayerTurnQueue.Dequeue();

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
		}

		// HACK: This just means the game won't get stuck whenever a bot plays - forces them to do minimum bet
		if ( PlayerTurnQueue.Peek().Client.IsBot )
		{
			_ = BotThink();
		}
	}

	private async Task BotThink()
	{
		await Task.Delay( 1000 );
		Bet( MinimumBet, PlayerTurnQueue.Peek() );
		MoveToNextPlayer();
	}

	private void Fold( Player player )
	{
		EventFeed.AddEvent( To.Everyone, $"{player.Client.Name} folds" );

		player.HasFolded = true;
	}

	private void Bet( float parameter, Player player )
	{
		parameter = parameter.Clamp( 0, player.Money );

		EventFeed.AddEvent( To.Everyone, $"{player.Client.Name} bets ${parameter}" );

		if ( MinimumBet < parameter )
			MinimumBet = parameter;

		Pot += parameter;
		player.LastBet = parameter;
		player.Money -= parameter;
	}

	public bool IsTurn( Player player )
	{
		return PlayerTurnQueue.Peek() == player;
	}

	public HandRank RankPlayerHand( Player player, out int score )
	{
		// TODO
		// For each 5 card combination in (communitycards + playerHand) calculate the rank

		var playerHand = player.Hand;
		var cards = playerHand.ToArray().Concat( CommunityCards ).ToArray();

		return PokerUtils.RankPokerHand( cards, out score );
	}

	public Player FindWinner()
	{
		var rankedPlayers = Players.Select( x => new { player = x, rank = RankPlayerHand( x, out var score ), score } );
		var groupedPlayers = rankedPlayers.GroupBy( x => x.rank );
		var orderedGroupedPlayers = groupedPlayers.OrderBy( x => x.Key );

		var bestPlayers = orderedGroupedPlayers.First();
		Log.Trace( "Best players: " + string.Join( ", ", bestPlayers.Select( x => x.player + " - " + x.rank + " - " + x.score ) ) );

		Player winner;

		if ( bestPlayers.Count() > 1 )
		{
			// Find highest score
			var orderedBestPlayers = bestPlayers.OrderBy( x => x.score );
			winner = orderedBestPlayers.First().player;
		}
		else
		{
			// One winner
			winner = bestPlayers.First().player;
		}

		EventFeed.AddEvent( To.Everyone, $"{winner.Client.Name} wins" );

		return winner;
	}

	[Event.Tick.Server]
	public void UpdateStatuses()
	{
		foreach ( var player in Players )
		{
			if ( player.HasFolded )
			{
				player.RpcSetStatus( To.Everyone, "Folded" );
			}
			else
			{
				player.RpcSetStatus( To.Everyone, $"${player.LastBet}, Bet" );

				if ( player.IsMyTurn )
					player.RpcSetStatus( To.Single( player.Client ), $"$0, Your turn" );
			}
		}
	}
}
