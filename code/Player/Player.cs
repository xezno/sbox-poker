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

	public Camera Camera
	{
		get => Components.Get<Camera>();
		set => Components.Add( value );
	}

	public Seat Seat => Entity.All.OfType<Seat>().First( x => x.Player == this );

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = 1000;

		LeftCard = new CardEntity() { Owner = this, Parent = this };
		RightCard = new CardEntity() { Owner = this, Parent = this };
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

	// TODO: This is all shit and temporary, we need a better way to position hands & cards
	public void SetAnimProperties()
	{
		if ( LifeState != LifeState.Alive )
			return;

		SetAnimParameter( "b_grounded", true );
		SetAnimParameter( "sit", 1 );
		SetAnimParameter( "sit_pose", GameSettings.Instance.SitPose );
		SetAnimParameter( "sit_offset_height", GameSettings.Instance.SitHeight );

		Vector3 aimPos = EyePosition + Rotation.Forward * 512;
		Vector3 lookPos = EyePosition + EyeRotation.Forward * 512;

		SetAnimLookAt( "aim_eyes", lookPos );
		SetAnimLookAt( "aim_head", lookPos );
		SetAnimLookAt( "aim_body", aimPos );

		SetAnimParameter( "b_vr", true );

		SetAnimParameter( "left_hand_ik.position", GameSettings.Instance.LeftHandPosition );
		SetAnimParameter( "left_hand_ik.rotation", GameSettings.Instance.LeftHandRotation );

		LeftCard.LocalPosition = GameSettings.Instance.BaseHoldPosition + GameSettings.Instance.LeftCardOffset;
		LeftCard.LocalRotation = GameSettings.Instance.LeftCardRotation;

		SetAnimParameter( "right_hand_ik.position", GameSettings.Instance.RightHandPosition );
		SetAnimParameter( "right_hand_ik.rotation", GameSettings.Instance.RightHandRotation );

		RightCard.LocalPosition = GameSettings.Instance.BaseHoldPosition + GameSettings.Instance.RightCardOffset;
		RightCard.LocalRotation = GameSettings.Instance.RightCardRotation;

		SetAnimParameter( "holdtype", 4 );
		SetAnimParameter( "aim_body_weight", 0.5f );
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
		EyeLocalPosition = Vector3.Up * 60f;
		EyeLocalRotation = Input.Rotation;

		Position = Position.WithZ( 6 );
	}
}
