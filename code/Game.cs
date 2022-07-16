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
			_ = new ExampleHudEntity();
			PokerControllerEntity = new();

			PokerControllerEntity.Run();
		}
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

		MoveToSpawnpoint( player );
		client.Pawn = player;
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		var clientIndex = Client.All.Count - 1;
		var spawnpoint = Entity.All.OfType<Seat>().ToList().First( x => x.SeatNumber == clientIndex );

		Log.Trace( $"Finding spawnpoint for client {clientIndex}" );

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
			return;
		}

		pawn.Transform = spawnpoint.Transform;
	}
}
