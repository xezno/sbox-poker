using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker.UI;

public partial class PokerChatBox : Panel
{
	static PokerChatBox Current;

	public Panel Canvas { get; protected set; }
	public TextEntry Input { get; protected set; }

	private TimeSince timeSinceActive = 0;

	public PokerChatBox()
	{
		Current = this;

		SetClass( "chat-box", true );
		StyleSheet.Load( "/UI/Components/PokerChatBox.scss" );

		Canvas = Add.Panel( "chat-canvas" );

		Input = Add.TextEntry( "" );
		Input.AddEventListener( "onsubmit", () => Submit() );
		Input.AddEventListener( "onblur", () => Close() );
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;
	}

	void Open()
	{
		AddClass( "open" );
		Input.Focus();

		timeSinceActive = 0;
	}

	void Close()
	{
		RemoveClass( "open" );
		Input.Blur();

		timeSinceActive = 0;
	}

	void Submit()
	{
		Close();

		var msg = Input.Text.Trim();
		Input.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddEntry( string name, string message, string avatar, string lobbyState = null, string className = null )
	{
		var e = Canvas.AddChild<ChatEntry>();

		e.Message.Text = message;
		e.NameLabel.Text = name;
		e.Avatar.SetTexture( avatar );

		e.SetClass( "noname", string.IsNullOrEmpty( name ) );
		e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

		if ( lobbyState == "ready" || lobbyState == "staging" )
		{
			e.SetClass( "is-lobby", true );
		}

		e.AddClass( className );

		timeSinceActive = 0;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Sandbox.Input.Pressed( InputButton.Chat ) )
			Open();

		SetClass( "fade", timeSinceActive > 5 && !HasClass( "open" ) );

		if ( !HasClass( "open" ) )
			Canvas.TryScrollToBottom();
	}

	[ConCmd.Client( "poker_chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string avatar = null, string lobbyState = null )
	{
		Current?.AddEntry( name, message, avatar, lobbyState );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ConCmd.Client( "poker_chat_addinfo", CanBeCalledFromServer = true )]
	public static void AddInformation( string message, string avatar = null )
	{
		Current?.AddEntry( null, message, avatar, className: "info" );
	}

	[ConCmd.Server( "poker_chat_say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.PlayerId}" );
	}
}
