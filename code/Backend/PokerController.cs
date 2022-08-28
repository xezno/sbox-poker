﻿using Sandbox;
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

	// TODO: Buy-ins
	// List of players that have bought in
	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

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
		Pot = 0;

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

		// Take blinds
		if ( !Bet( blind * 0.5f, SmallBlind ) )
		{
			/*
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

		if ( !Bet( blind, BigBlind ) )
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

		// OK now everyone needs to pay the ante
		Players.ForEach( player =>
		{
			if ( !Bet( 5, player ) )
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
		Log.Trace( $"Dealt community cards: {cardsStr}" );

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

		PlayerTurnQueue.CreateQueue( Players );

		if ( Round >= Rounds.Showdown || PlayerTurnQueue.Count <= 1 )
		{
			Log.Info( "Game over" );

			var winner = FindWinner();
			ProcessWinner( winner );

			Run(); // TODO: Should move to a different game state instead and wait for players etc.

			return;
		}
	}

	private void ProcessWinner( Player winner )
	{
		winner.Money += Pot;
		Pot = 0;

		GameServices.UpdateLeaderboard( winner.Client.PlayerId, 1, "wins" );
		Players.Where( x => x != winner ).ToList().ForEach( x => GameServices.UpdateLeaderboard( x.Client.PlayerId, -1, "wins" ) );
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

	public bool IsTurn( Player player )
	{
		return PlayerTurnQueue.Peek() == player;
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
