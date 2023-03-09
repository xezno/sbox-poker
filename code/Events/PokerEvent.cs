namespace Poker;
public class PokerEvent
{
	public class TurnChange : EventAttribute
	{
		private const string Name = "PokerEvent.TurnChange";
		public TurnChange() : base( Name ) { }
	}

	public class NewRound : EventAttribute
	{
		private const string Name = "PokerEvent.NewRound";
		public NewRound() : base( Name ) { }
	}

	public class Client
	{
		public class LocalTurn : EventAttribute
		{
			private const string Name = "PokerEvent.Client.LocalTurn";
			public LocalTurn() : base( Name ) { }
		}
	}

	public class Server
	{

	}
}
