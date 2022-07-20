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

	private List<Player> Players => Entity.All.OfType<Player>().ToList();
	private Deck Deck { get; set; }

	private Player Dealer { get; set; }
	private Player SmallBlind { get; set; }
	private Player BigBlind { get; set; }

	public PokerControllerEntity()
	{
		Instance = this;

		//
		// Determine dealer
		//
		Dealer = Players[0];

		//
		// Determine small blind & big blind
		//
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

		//
		// Give each player two hole cards
		//
		foreach ( var player in Players )
		{
			// TODO
		}

		//
		// Pre-flop
		//
	}

	public enum Streets
	{
		Preflop,
		Flop,
		Turn,
		River
	}

	/// <summary>
	/// Main game loop
	/// </summary>
	public void Run()
	{
		Setup();

		PlayRound();
	}

	/// <summary>
	/// Initialize game
	/// </summary>
	/// <returns>Whether setup was successful or not</returns>
	private void Setup()
	{
		Log.Info( "\n========================= Start Poker Game ==========================" );

		// Deck setup
		Deck = new Deck();
		Deck.Shuffle();
	}

	/// <summary>
	/// Play a round of Poker
	/// </summary>
	private void PlayRound()
	{
		Log.Info( "\n========================= Start Poker Round =========================" );
	}

	[DebugOverlay( "poker_overlay", "Poker Controller", "style" )]
	private static void DebugOverlay()
	{
		if ( !Host.IsServer )
			return;

		var instance = PokerControllerEntity.Instance;
		if ( instance == null )
			Log.Error( "Instance was null!" );

		OverlayUtils.BoxWithText( Render.Draw2D, new Vector2( 45, 200 ), "Poker Controller",
			  "Hello, World!" );
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
	}
}
