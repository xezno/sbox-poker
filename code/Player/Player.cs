﻿using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.Linq;

namespace Poker;

public partial class Player : AnimatedEntity
{
	public List<Backend.Card> Hand { get; set; }

	[Net] public string AvatarData { get; set; }
	[Net] public float Money { get; set; }
	[Net] public float LastBet { get; set; }
	[Net] public Backend.Move LastMove { get; set; }
	[Net] public bool IsMyTurn { get; set; }
	[Net] public bool HasFolded { get; set; }

	[Net] public CardEntity LeftCard { get; set; }
	[Net] public CardEntity RightCard { get; set; }

	public string StatusText { get; set; }

	public Camera Camera
	{
		get => Components.Get<Camera>();
		set => Components.Add( value );
	}

	public Seat Seat => Entity.All.OfType<Seat>().First( x => x.Player == this );

	public override void Spawn()
	{
		SetModel( "models/citizen/poker_citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = 1000;

		LeftCard = new CardEntity() { Owner = this };
		LeftCard.SetParent( this, "cards" );
		LeftCard.LocalPosition = 0;
		LeftCard.LocalRotation = Rotation.Identity;

		RightCard = new CardEntity() { Owner = this };
		RightCard.SetParent( this, "cards" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsServer )
		{
			IsMyTurn = Backend.PokerControllerEntity.Instance?.IsTurn( this ) ?? false;
		}

		SetEyeTransforms();
		SetAnimProperties();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		SetEyeTransforms();
		SetBodyGroups();
	}

	private void SetBodyGroups()
	{
		if ( IsClient && IsLocalPawn && Camera.GetCameraTarget() != Camera.Targets.ThirdPerson )
		{
			SetBodyGroup( "Head", 1 );
		}
		else
		{
			SetBodyGroup( "Head", 0 );
		}
	}

	public void SetAnimProperties()
	{
		if ( LifeState != LifeState.Alive )
			return;

		SetAnimParameter( "b_showcards", InputLayer.Evaluate( "your_cards" ) );

		// TODO: remove this ( test )
		if ( InputLayer.Evaluate( "emote.middle_finger" ) )
			SetAnimParameter( "action", (int)Action.Emote_MiddleFinger );
		else if ( InputLayer.Evaluate( "emote.thumbs_up" ) )
			SetAnimParameter( "action", (int)Action.Emote_ThumbsUp );
		else if ( InputLayer.Evaluate( "emote.thumbs_down" ) )
			SetAnimParameter( "action", (int)Action.Emote_ThumbsDown );
		else if ( InputLayer.Evaluate( "emote.pump" ) )
			SetAnimParameter( "action", (int)Action.Emote_Pump );
		else
			SetAnimParameter( "action", 0 );

		SetAnimParameter( "sit_pose", 0 );

		Vector3 lookPos = EyePosition + EyeRotation.Forward * 512;
		lookPos.z += 128f;
		Vector3 emoteAimPos = EyePosition + EyeRotation.Forward * 512;
		emoteAimPos = emoteAimPos.WithZ( 128 );

		SetAnimLookAt( "aim_head", lookPos );
		SetAnimLookAt( "aim_emote", emoteAimPos );
		SetAnimParameter( "aim_head_weight", 1.0f );
		SetAnimParameter( "aim_emote_weight", 1.0f );

		LeftCard.LocalPosition = GameSettings.Instance.LeftHandPosition;
		LeftCard.LocalRotation = GameSettings.Instance.LeftHandRotation;

		RightCard.LocalPosition = GameSettings.Instance.RightHandPosition;
		RightCard.LocalRotation = GameSettings.Instance.RightHandRotation;

		if ( IsServer )
		{
			DebugOverlay.ScreenText( $"SV: {emoteAimPos}", 10, 0 );
		}
		else
		{
			DebugOverlay.ScreenText( $"CL: {emoteAimPos}", 20, 0 );
		}
	}

	[DebugOverlay( "poker_debug", "Poker Debug", "style" )]
	public static void OnDebugOverlay()
	{
		if ( !Host.IsClient )
			return;

		var player = Local.Pawn as Player;

		if ( player == null )
			return;

		string handStr = "No hand";
		if ( player.Hand != null )
			handStr = string.Join( ", ", player.Hand );

		OverlayUtils.BoxWithText( Render.Draw2D, new Vector2( 45, 400 ), "CL: Local Player",
			$"Hand: {handStr}\n" +
			$"Is my turn?: {player.IsMyTurn}" );
	}

	private void SetEyeTransforms()
	{
		var eyeTransform = GetAttachment( "eyes", false ) ?? default;
		EyeLocalPosition = eyeTransform.Position + Vector3.Up * 8f - Vector3.Forward * 4f;
		EyeLocalRotation = Input.Rotation;

		Position = Position.WithZ( 6 );
	}

	[ClientRpc]
	public void RpcSetHand( Backend.Card card0, Backend.Card card1 ) // THIS IS SHIT!
	{
		Hand = new() { card0, card1 };
	}

	[ClientRpc]
	public void RpcSetStatus( string status )
	{
		StatusText = status;
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( InputLayer.Evaluate( "your_cards" ) || InputLayer.Evaluate( "community_cards" ) )
		{
			inputBuilder.ViewAngles = inputBuilder.OriginalViewAngles;
			inputBuilder.StopProcessing = true;
			return;
		}

		var inputAngles = inputBuilder.ViewAngles;
		var clampedAngles = new Angles(
			inputAngles.pitch.Clamp( -45, 45 ),
			inputAngles.yaw,
			inputAngles.roll
		);

		inputBuilder.ViewAngles = clampedAngles;
	}
}
