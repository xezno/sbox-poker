﻿global using Editor;
global using Sandbox;
global using System.Collections.Generic;
global using System.Linq;
using Poker.UI;
using System;

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

		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}

		PrecacheAssets();
	}

	private void PrecacheAssets()
	{
		// Precache all cards
		foreach ( var suit in Enum.GetValues( typeof( Suit ) ) )
		{
			foreach ( var value in Enum.GetValues( typeof( Value ) ) )
			{
				var card = new Card( (Suit)suit, (Value)value );

				Log.Trace( $"Precache: {card.GetFilename()}" );
				Precache.Add( card.GetFilename() );
			}
		}
	}

	public override void OnVoicePlayed( IClient cl )
	{
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

	[GameEvent.Tick.Server]
	public void OnServerTick()
	{
		if ( !IsRunning && Game.Clients.Count >= 2 )
			Run();
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
		if ( client.Pawn is Spectator )
			return;

		PokerChatBox.AddInformation( To.Everyone, $"{client.Name} was moved to spectators." );

		client.Pawn?.Delete();

		var spectatorPawn = new Spectator();
		client.Pawn = spectatorPawn;
	}

	public void CreatePlayerFor( IClient client )
	{
		if ( client.Pawn is Player )
			return;

		PokerChatBox.AddInformation( To.Everyone, $"{client.Name} has joined the table." );

		client.Pawn?.Delete();

		var clothingContainer = new ClothingContainer();

		if ( client.IsBot )
			clothingContainer.LoadRandom();
		else
			clothingContainer.LoadFromClient( client );

		var player = PrefabLibrary.Spawn<Player>( "prefabs/poker.prefab" );
		player.AvatarData = clothingContainer.Serialize();
		clothingContainer.DressEntity( player );

		client.Pawn = player;

		if ( MoveToSeat( client ) )
		{
			player.CreateChips();
		}
	}

	private bool MoveToSeat( IClient client )
	{
		var orderedSeats = Entity.All.OfType<SeatEntity>().OrderBy( x => x.SeatNumber );
		var emptySeats = orderedSeats.Where( x => !x.IsOccupied );

		var firstAvailableSeat = emptySeats.FirstOrDefault();

		if ( firstAvailableSeat == null )
		{
			Log.Error( "Wasn't able to seat player, moving to spectator" );
			PokerGame.Instance.CreateSpectatorFor( client );
			return false;
		}

		firstAvailableSeat.SetOccupiedBy( client.Pawn as Player );
		return true;
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
