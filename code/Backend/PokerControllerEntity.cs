using Poker.UI;
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Poker.Backend;

/// <summary>
/// Poker game FSM
/// </summary>
public partial class PokerControllerEntity : Entity
{
	public static PokerControllerEntity Instance { get; set; }
	[Net] public IList<Card> CommunityCards { get; set; }
	[Net] public float Pot { get; set; }
	[Net] public float MinimumBet { get; set; }

	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	private Queue<Player> PlayerTurnQueue { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Instance = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Instance = this;
	}

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

	public void Run()
	{
		// Instantiate everything
		Deck = new();
		PlayerTurnQueue = new();
		CommunityCards.Clear();
		Round = Rounds.Preflop;

		// Determine dealer
		Dealer = Players[0];

		// Determine small blind & big blind
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
		Bet( blind * 0.5f, SmallBlind );
		Bet( blind, BigBlind );

		// Give each player two hole cards
		Players.ForEach( player =>
		{
			player.Hand = Deck.CreateHand();
			player.RpcSetHand( To.Single( player ), player.Hand[0], player.Hand[1] );

			player.LeftCard.RpcSetCard( To.Single( player ), player.Hand[0] );
			player.RightCard.RpcSetCard( To.Single( player ), player.Hand[1] );
		} );

		// Reset players
		Players.ForEach( player =>
		{
			player.HasFolded = false;
		} );

		// Delete chips
		Entity.All.OfType<ChipEntity>().ToList().ForEach( x => x.Delete() );
		Entity.All.OfType<ChipStackEntity>().ToList().ForEach( x => x.Delete() );

		// Spawn chips
		Players.ForEach( player =>
		{
			var seat = player.Seat;
			var chipSpawn = Entity.All.OfType<PlayerChipSpawn>().First( x => x.SeatNumber == seat.SeatNumber );
			float spacing = 3.5f;

			// ChipEntity.CreateStack( 32, chipSpawn.Position );
			var chipOffsets = new (int, float, Vector3)[]
			{
				new( 10, 500, Vector3.Zero ),
				new( 20, 250, new Vector3( 0, spacing )),
				new( 30, 100, new Vector3( spacing, 0 )),
				new( 40, 50, new Vector3( spacing, spacing ))
			};

			foreach ( var offset in chipOffsets )
			{
				ChipStackEntity.CreateStack( offset.Item1, offset.Item2, chipSpawn.Position + offset.Item3 );
			}
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
		PokerChatBox.AddInformation( To.Everyone, $"Dealt community cards: {cardsStr}" );

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
		Players.ForEach( player =>
		{
			if ( player.Money < 0 )
				player.Client.Kick();
		} );

		PlayerTurnQueue.Dequeue();

		if ( PlayerTurnQueue.Count == 0 )
		{
			// Reached the end of this round; let's move to the next one
			Log.Trace( "Reached the end of the round. Run()" );
			StartNextRound();
			return;
		}

		if ( PlayerTurnQueue.Peek().HasFolded )
		{
			MoveToNextPlayer();
		}

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

	[DebugOverlay( "poker_debug", "Poker Debug", "style" )]
	private static void DebugOverlay()
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerControllerEntity.Instance;
		if ( instance == null )
			return;

		if ( instance.Deck == null )
			return;

		var communityCards = string.Join( ", ", instance.CommunityCards.Select( card => card.ToString() ) );

		OverlayUtils.BoxWithText( Render.Draw2D, new Rect( 45, 200, 400, 100 ), "SV: Poker Controller",
			  $"Current turn: {instance.PlayerTurnQueue.Peek().Client.Name}\n" +
			  $"Current round: {instance.Round}\n" +
			  $"Community cards: {communityCards}" );
	}

	[ConCmd.Server( "poker_force_next_player" )]
	public static void ForceNextPlayer()
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerControllerEntity.Instance;
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
	public static void SubmitMove( Move move, float parameter )
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerControllerEntity.Instance;
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
		EventFeed.AddEvent( To.Everyone, $"{player.Client.Name} folds" );

		player.HasFolded = true;
	}

	private void Bet( float parameter, Player player )
	{
		parameter = parameter.Clamp( 0, player.Money );

		EventFeed.AddEvent( To.Everyone, $"{player.Client.Name} bets", parameter );

		if ( MinimumBet < parameter )
			MinimumBet = parameter;

		Pot += parameter;
		player.Money -= parameter;
	}

	public bool IsTurn( Player player )
	{
		return PlayerTurnQueue.Peek() == player;
	}

	public HandRank RankPlayerHand( Player player, out int score )
	{
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
}
