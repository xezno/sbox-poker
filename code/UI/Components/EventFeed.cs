using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Poker.UI;

[UseTemplate]
public partial class EventFeed : Panel
{
	private static EventFeed Instance { get; set; }
	public EventFeed()
	{
		Instance = this;
	}

	[Obsolete()]
	[ConCmd.Client( "poker_add_event", CanBeCalledFromServer = true )]
	public static void AddEvent( string text )
	{
		Host.AssertClient();

		Instance.AddChild( new EventEntry( text ) );
	}
}

class EventEntry : Panel
{
	TimeSince timeSinceCreated;

	public EventEntry( string text )
	{
		timeSinceCreated = 0;

		AddClass( "event-entry" );
		Add.PokerLabel( text );
	}

	public override void Tick()
	{
		base.Tick();

		if ( timeSinceCreated > 3 )
			Delete();
	}
}
