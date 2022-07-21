using Poker.UI;
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Backend;

/// <summary>
/// Poker game FSM
/// </summary>
public class PokerControllerEntity : Entity
{
	public static PokerControllerEntity Instance { get; set; }

	public List<Card> CommunityCards { get; set; }
	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	private Queue<Player> PlayerTurnQueue { get; set; }

	private float Pot { get; set; }
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
		CommunityCards = new();
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
		Players.ForEach( player => player.Hand = Deck.CreateHand() );

		// Start pre-flop
		StartNextRound();
	}

	private void StartNextRound()
	{
		if ( Round == Rounds.Showdown )
		{
			Log.Info( "Game over" );
			Run();
			return;
		}

		StartRound( Round++ );
	}

	private void AddCommunityCards( int count )
	{
		var cards = Deck.Draw( count );
		var cardsStr = string.Join( ", ", cards.Select( x => x.ToShortString() ) );
		PokerChatBox.AddInformation( To.Everyone, $"Dealed community cards: {cardsStr}" );

		CommunityCards.AddRange( cards );
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
				AddCommunityCards( 1 );
				break;
			case Rounds.River:
				AddCommunityCards( 1 );
				break;
		}

		Players.ForEach( player => PlayerTurnQueue.Enqueue( player ) );
	}

	private void MoveToNextPlayer()
	{
		PlayerTurnQueue.Dequeue();

		if ( PlayerTurnQueue.Count == 0 )
		{
			// Reached the end of this round; let's move to the next one
			Log.Trace( "Reached the end of the round. Run()" );
			StartNextRound();
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

		if ( instance.IsTurn( player ) )
		{
			Log.Trace( $"Player {player.Name} ({caller.Name}) submitted move {move} with param {parameter}" );
			instance.MoveToNextPlayer();
			instance.BetThisRound += parameter;
			instance.Pot += parameter;
			PokerChatBox.AddInformation( To.Everyone, $"{caller.Name} bets {parameter}" );
		}
	}

	public bool IsTurn( Player player )
	{
		return PlayerTurnQueue.Peek() == player;
	}
}
