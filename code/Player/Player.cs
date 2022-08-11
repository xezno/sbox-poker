using Sandbox;
using SandboxEditor;
using System.Linq;

namespace Poker;

public partial class Player : AnimatedEntity
{
	[Net, Local] public Backend.Hand Hand { get; set; }
	[Net] public string AvatarData { get; set; }
	[Net] public float Money { get; set; }
	[Net] public bool IsMyTurn { get; set; }
	[Net] public bool HasFolded { get; set; }

	[Net] public CardEntity LeftCard { get; set; }
	[Net] public CardEntity RightCard { get; set; }

	[Net, Local] public VR.HeadEntity Head { get; set; }
	[Net, Local] public VR.LeftHand LeftHand { get; set; }
	[Net, Local] public VR.RightHand RightHand { get; set; }

	public Camera Camera
	{
		get => Components.Get<Camera>();
		set => Components.Add( value );
	}

	public Seat Seat => Entity.All.OfType<Seat>().First( x => x.Player == this );

	public override void Spawn()
	{
		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = 1000;

		Head = new() { Owner = this };
		LeftHand = new() { Owner = this };
		RightHand = new() { Owner = this };

		LeftHand.Other = RightHand;
		RightHand.Other = LeftHand;

		LeftCard = new CardEntity() { Owner = this, Parent = RightHand };
		RightCard = new CardEntity() { Owner = this, Parent = RightHand };
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

		LeftHand?.Simulate( cl );
		RightHand?.Simulate( cl );
		Head?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		SetEyeTransforms();
		SetAnimProperties();

		LeftHand?.FrameSimulate( cl );
		RightHand?.FrameSimulate( cl );
		Head?.FrameSimulate( cl );
	}

	// TODO: This is all shit and temporary, we need a better way to position hands & cards
	public void SetAnimProperties()
	{
		if ( LifeState != LifeState.Alive )
			return;

		var leftLocal = Transform.ToLocal( LeftHand.Transform );
		var rightLocal = Transform.ToLocal( RightHand.Transform );
		rightLocal = rightLocal.WithRotation( rightLocal.Rotation * Rotation.From( 0, 0, 180 ) );

		LeftCard.LocalPosition = GameSettings.Instance.BaseHoldPosition + GameSettings.Instance.LeftCardOffset;
		LeftCard.LocalRotation = GameSettings.Instance.LeftCardRotation;

		RightCard.LocalPosition = GameSettings.Instance.BaseHoldPosition + GameSettings.Instance.RightCardOffset;
		RightCard.LocalRotation = GameSettings.Instance.RightCardRotation;

		LeftCard.ResetInterpolation();
		RightCard.ResetInterpolation();
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
			handStr = string.Join( ", ", player.Hand.Cards );

		OverlayUtils.BoxWithText( Render.Draw2D, new Vector2( 45, 400 ), "CL: Local Player",
			$"Hand: {handStr}\n" +
			$"Is my turn?: {player.IsMyTurn}" );
	}

	private void SetEyeTransforms()
	{
		EyeLocalPosition = Input.VR.Head.Position;
		EyeLocalRotation = Input.VR.Head.Rotation;

		Position = Position.WithZ( 6 );
	}
}
