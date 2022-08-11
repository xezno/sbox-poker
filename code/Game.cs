using Poker.Backend;
using Sandbox;
using System.Linq;

namespace Poker;

public partial class Game : Sandbox.Game
{
	public PokerControllerEntity PokerControllerEntity { get; set; }

	public Game()
	{
		if ( IsServer )
		{
			_ = new Hud();
		}
	}

	[ConCmd.Server( "poker_start" )]
	public static void StartPokerGame()
	{
		var instance = Game.Current as Game;

		instance.PokerControllerEntity?.Delete();
		instance.PokerControllerEntity = new();
		instance.PokerControllerEntity.Run();
	}

	[ConCmd.Server( "poker_spawn_chip" )]
	public static void SpawnChip()
	{
		Log.Trace( $"Spawning chip" );

		var player = ConsoleSystem.Caller.Pawn as Player;
		var tr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 1024f ).Ignore( player ).Run();

		for ( float x = -8; x < 8; ++x )
		{
			for ( float y = -4; y < 4; ++y )
			{
				var count = Rand.Int( 16, 32 );
				var value = Rand.FromArray( new[] { 50, 100, 250, 500 } );
				ChipStackEntity.CreateStack( count, value, tr.EndPosition + new Vector3( x * 3, y * 3, 0 ) );
			}
		}
	}

	[ConCmd.Server( "poker_spawn_card" )]
	public static void SpawnCard( string suitStr, string valueStr )
	{
		var suit = System.Enum.Parse<Suit>( suitStr, true );
		var value = System.Enum.Parse<Value>( valueStr, true );

		Log.Trace( $"Spawning {suit}, {value}" );

		var player = ConsoleSystem.Caller.Pawn as Player;
		var tr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 1024f ).Ignore( player ).Run();

		var cardEntity = new CardEntity();
		cardEntity.Position = tr.EndPosition;
		cardEntity.Rotation = Rotation.LookAt( -tr.Direction.WithZ( 0 ).Normal );

		var backendCard = new Backend.Card( suit, value );
		cardEntity.RpcSetCard( To.Everyone, backendCard );
	}

	[ConCmd.Client( "poker_ws_connect" )]
	public static void WebsocketConnect()
	{
		WebsocketConnection.Connect();

		if ( Local.Client.IsListenServerHost )
		{
			WebsocketConnection.Send( "table/create/request", new Table.CreateData() { } );
		}

		Log.Trace( "Created WS connection" );
	}

	[ConCmd.Client( "poker_ws_disconnect" )]
	public static void WebsocketDisconnect()
	{
		WebsocketConnection.Disconnect();
		Log.Trace( "Disconnected WS connection" );
	}

	[ClientRpc]
	public void OnPlayerReady()
	{
		// WebsocketConnect();
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new Player();
		//var clothingContainer = new ClothingContainer();

		//if ( client.IsBot )
		//	clothingContainer.LoadRandom();
		//else
		//	clothingContainer.LoadFromClient( client );

		//player.Camera = new Camera();

		//player.AvatarData = clothingContainer.Serialize();
		//clothingContainer.DressEntity( player );

		MoveToSeat( player );
		client.Pawn = player;

		OnPlayerReady( To.Single( client ) );
	}

	private void MoveToSeat( Player player )
	{
		var orderedSeats = Entity.All.OfType<Seat>().OrderBy( x => x.SeatNumber );
		var emptySeats = orderedSeats.Where( x => !x.IsOccupied );

		var firstAvailableSeat = emptySeats.FirstOrDefault();

		if ( firstAvailableSeat == null )
		{
			Log.Error( "Wasn't able to seat player, kicking them" );
			player.Client.Kick();
			return;
		}

		firstAvailableSeat.SetOccupiedBy( player );
	}

	public override void RenderHud()
	{
		return;
		base.RenderHud();

		//
		// scale the screen using a matrix, so the scale math doesn't invade everywhere
		// (other than having to pass the new scale around)
		//

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using ( Render.Draw2D.MatrixScope( matrix ) )
		{
			Render.Draw2D.Color = Color.White;
			Render.Draw2D.FontFamily = "Roboto";

			foreach ( var entity in Entity.All )
			{
				if ( entity is Client )
					continue;

				var screenPos = (Vector2)entity.Transform.Position.ToScreen();
				var rect = new Rect( screenPos * screenSize, 1024f );
				var textRect = Render.Draw2D.MeasureText( rect, entity.Name );
				var drawRect = new Rect( textRect.Position - textRect.Size / 2.0f, textRect.Size );
				var displayInfo = DisplayInfo.For( entity );

				Render.Draw2D.DrawText( drawRect, displayInfo.Name, TextFlag.Center );
			}
		}
	}
}
