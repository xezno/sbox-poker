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
		cardEntity.SetCard( backendCard );
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new Player();
		var clothingContainer = new ClothingContainer();

		if ( client.IsBot )
			clothingContainer.LoadRandom();
		else
			clothingContainer.LoadFromClient( client );

		player.AvatarData = clothingContainer.Serialize();
		clothingContainer.DressEntity( player );

		MoveToSeat( player );
		client.Pawn = player;
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
}
