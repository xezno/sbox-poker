global using Editor;
global using Sandbox;
global using System.Collections.Generic;
global using System.Linq;

namespace Poker;

public partial class PokerGame : GameManager
{
	[Net] public IList<Card> CommunityCards { get; set; }
	[Net] public float Pot { get; set; }
	[Net] public float MinimumBet { get; set; }

	public static PokerGame Instance { get; set; }

	public PokerGame()
	{
		Instance = this;

		if ( Game.IsServer )
		{
			_ = new Hud();
		}
	}

	public override void OnVoicePlayed( IClient cl )
	{
		// TODO: Re-implement this in razor
		// Old_PlayerList.Instance?.OnVoicePlayed( cl.PlayerId, cl.VoiceLevel );

		if ( cl.Pawn is Player player )
		{
			player.VoiceLevel = cl.Voice.CurrentLevel;
		}
	}

	[ConCmd.Server( "poker_start" )]
	public static void StartPokerGame()
	{
		var instance = PokerGame.Current as PokerGame;

		Instance.Run();
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
				var count = Game.Random.Int( 16, 32 );
				var value = Game.Random.FromArray( new[] { 50, 100, 250, 500 } );

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

		var card = new Card( suit, value );
		cardEntity.RpcSetCard( To.Everyone, card );
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		if ( Entity.All.OfType<SeatEntity>().Count() < Game.Clients.Count )
			CreateSpectatorFor( client );
		else
			CreatePlayerFor( client );
	}

	public void CreateSpectatorFor( IClient client )
	{
		client.Pawn?.Delete();

		var spectatorPawn = new Spectator();
		client.Pawn = spectatorPawn;
	}

	[ConCmd.Server( "poker_switch" )]
	public static void SwitchPlayer()
	{
		var client = ConsoleSystem.Caller;
		var pawn = client.Pawn;

		if ( pawn is Player )
		{
			PokerGame.Instance.CreateSpectatorFor( client );
		}
		else if ( pawn is Spectator )
		{
			PokerGame.Instance.CreatePlayerFor( client );
		}
	}

	public void CreatePlayerFor( IClient client )
	{
		client.Pawn?.Delete();

		var clothingContainer = new ClothingContainer();

		if ( client.IsBot )
			clothingContainer.LoadRandom();
		else
			clothingContainer.LoadFromClient( client );

		var player = new Player
		{
			AvatarData = clothingContainer.Serialize()
		};

		clothingContainer.DressEntity( player );

		client.Pawn = player;
		MoveToSeat( client );
	}

	private void MoveToSeat( IClient client )
	{
		var orderedSeats = Entity.All.OfType<SeatEntity>().OrderBy( x => x.SeatNumber );
		var emptySeats = orderedSeats.Where( x => !x.IsOccupied );

		var firstAvailableSeat = emptySeats.FirstOrDefault();

		if ( firstAvailableSeat == null )
		{
			Log.Error( "Wasn't able to seat player, kicking them" );
			client.Kick(); // TODO: Move to spectators
			return;
		}

		firstAvailableSeat.SetOccupiedBy( client.Pawn as Player );
	}

	[Event.Debug.Overlay( "poker_debug", "Poker Debug", "style" )]
	private static void DebugOverlay()
	{
		if ( !Game.IsServer )
			return;

		var instance = PokerGame.Instance;
		if ( instance == null )
			return;

		var communityCards = string.Join( ", ", instance.CommunityCards.Select( card => card.ToString() ) );

		OverlayUtils.BoxWithText( new Rect( 45, 200, 400, 100 ),
			  "CL: Poker Controller",
			  $"Pot: {instance.Pot}\n" +
			  $"Minimum bet: {instance.MinimumBet}\n" +
			  $"Community cards: {communityCards}" );
	}
}
