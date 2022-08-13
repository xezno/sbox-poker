using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker.UI;

[UseTemplate]
public partial class EventFeed : Panel
{
	private static EventFeed Instance { get; set; }
	public EventFeed()
	{
		Instance = this;
	}

	[ConCmd.Client( "poker_add_event", CanBeCalledFromServer = true )]
	public static void AddEvent( string text, float money = float.NaN )
	{
		Host.AssertClient();

		if ( float.IsNaN( money ) )
			Instance.AddChild( new EventEntry( text ) );
		else
			Instance.AddChild( new EventEntry( text, money ) );
	}
}

class EventEntry : Panel
{
	TimeSince timeSinceCreated;

	public EventEntry( string text )
	{
		timeSinceCreated = 0;

		AddClass( "event-entry" );
		Add.Label( text );
	}

	public EventEntry( string text, float value )
	{
		timeSinceCreated = 0;

		AddClass( "event-entry" );
		Add.Label( text );
		Add.Money( $"{value}" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( timeSinceCreated > 3 )
			Delete();
	}
}
