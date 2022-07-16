using Sandbox;
using System.Collections.Generic;

namespace Poker.Backend;

/// <summary>
/// Poker game FSM
/// </summary>
public class PokerControllerEntity : Entity
{
	private List<Player> Players { get; set; } = new();
	private Deck Deck { get; set; }

	public PokerControllerEntity() { }

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
}
