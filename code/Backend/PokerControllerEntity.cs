using Poker.UI;
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Backend;

/// <summary>
/// Poker game FSM
/// </summary>
public partial class PokerControllerEntity : Entity
{
	public static PokerControllerEntity Instance { get; set; }
	[Net] public IList<Card> CommunityCards { get; set; }
	[Net] public float Pot { get; set; }

	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	private Queue<Player> PlayerTurnQueue { get; set; }
	private float BetThisRound { get; set; }

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

		// Give each player two hole cards
		Players.ForEach( player =>
		{
			player.Hand = Deck.CreateHand();
			player.LeftCard.RpcSetCard( To.Single( player ), player.Hand.Cards[0] );
			player.RightCard.RpcSetCard( To.Single( player ), player.Hand.Cards[1] );
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
				AddCommunityCards( 3 );
				break;
			case Rounds.Turn:
			case Rounds.River:
				AddCommunityCards( 1 );
				break;
		}

		Players.ForEach( player =>
		{
			if ( !player.HasFolded )
				PlayerTurnQueue.Enqueue( player );
		} );

		if ( Round == Rounds.Showdown || PlayerTurnQueue.Count <= 1 )
		{
			Log.Info( "Game over" );
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
			Log.Trace( "Reached the end of the round. Run()" );
			StartNextRound();
			return;
		}

		if ( PlayerTurnQueue.Peek().HasFolded )
		{
			MoveToNextPlayer();
		}
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
		PokerChatBox.AddInformation( To.Everyone, $"{instance.PlayerTurnQueue.Peek().Client.Name} took too long!" );
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
						instance.MoveToNextPlayer();
						PokerChatBox.AddInformation( To.Everyone, $"{caller.Name} folds" );

						player.HasFolded = true;

						break;
					}
				case Move.Bet:
					{
						instance.MoveToNextPlayer();
						instance.BetThisRound += parameter;
						instance.Pot += parameter;
						player.Money -= parameter;
						PokerChatBox.AddInformation( To.Everyone, $"{caller.Name} bets ${parameter}" );

						break;
					}
			}
		}
	}

	public bool IsTurn( Player player )
	{
		return PlayerTurnQueue.Peek() == player;
	}
}
