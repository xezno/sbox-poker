namespace Poker;
public class PokerEvent
{
	/// <summary>
	/// Only runs on server
	/// </summary>
	public class TurnChange : EventAttribute
	{
		public const string Name = "PokerEvent.TurnChange";
		public TurnChange() : base( Name ) { }
	}

	/// <summary>
	/// Only runs on server
	/// </summary>
	public class NewRound : EventAttribute
	{
		public const string Name = "PokerEvent.NewRound";
		public NewRound() : base( Name ) { }
	}

	/// <summary>
	/// Only runs on server
	/// </summary>
	public class CommunityCardsUpdated : EventAttribute
	{
		public const string Name = "PokerEvent.CommunityCardsUpdated";
		public CommunityCardsUpdated() : base( Name ) { }
	}

	/// <summary>
	/// Only runs on server
	/// </summary>
	public class GameStart : EventAttribute
	{
		public const string Name = "PokerEvent.GameStart";
		public GameStart() : base( Name ) { }
	}

	/// <summary>
	/// Only runs on server
	/// </summary>
	public class GameOver : EventAttribute
	{
		public const string Name = "PokerEvent.GameOver";
		public GameOver() : base( Name ) { }
	}
}
