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
}
