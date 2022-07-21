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
