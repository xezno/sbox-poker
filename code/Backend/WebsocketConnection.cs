using Sandbox;

namespace Poker.Backend;

public class WebsocketConnection
{
	private static WebSocket websocket;

	public static void Connect()
	{
		if ( websocket?.IsConnected ?? false ) return;

		websocket = new();
		websocket.Connect( "ws://localhost:6272" );
		websocket.OnMessageReceived += MessageReceived;

		Send( "player/login/request",
			new Player.LoginData()
			{
				PlayerId = Local.PlayerId.ToString(),
				PlayerName = Local.Client.Name
			}
		);
	}

	private static void MessageReceived( string messageStr )
	{
		var message = Serializer.Deserialize<Message>( messageStr );
		Log.Trace( $"Received message {message.Command}" );
	}

	public static void Disconnect()
	{
		Send( "player/quit/request" );
		websocket.Dispose();
	}

	public static void Send<T>( string command, T obj )
	{
		var message = new Message();
		message.Command = command;
		message.Data = Serializer.FromStruct( obj );

		var serializedData = Serializer.Serialize( message );
		websocket?.Send( serializedData );
	}

	public static void Send( string command )
	{
		var message = new Message();
		message.Command = command;

		var serializedData = Serializer.Serialize( message );
		websocket?.Send( serializedData );
	}
}
