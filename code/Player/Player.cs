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
		if ( IsClient && IsLocalPawn && GetCameraTarget() != CameraTargets.ThirdPerson )
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
		SetAnimParameter( "sit_pose", 1 );
		SetAnimParameter( "sit_offset_height", 10.0f );

		Vector3 aimPos = EyePosition + Rotation.Forward * 512;
		Vector3 lookPos = EyePosition + EyeRotation.Forward * 512;

		SetAnimLookAt( "aim_eyes", lookPos );
		SetAnimLookAt( "aim_head", lookPos );
		SetAnimLookAt( "aim_body", aimPos );

		SetAnimParameter( "b_vr", true );

		SetAnimParameter( "left_hand_ik.position", new Vector3( 6, 10, 32 ) );
		SetAnimParameter( "left_hand_ik.rotation", new Angles( 0, -0, 180 ).ToRotation() );

		var basePos = new Vector3( 11f, 0.5f, 34 );

		LeftCard.LocalPosition = basePos + new Vector3( 0, 1.25f, 0 );
		LeftCard.LocalRotation = Rotation.From( 130, 20, 180 + 20 );

		SetAnimParameter( "right_hand_ik.position", new Vector3( 8, -6, 32 ) );
		SetAnimParameter( "right_hand_ik.rotation", new Angles( 0, 45, 120 ).ToRotation() );

		RightCard.LocalPosition = basePos + new Vector3( 0.1f, -0.5f, 0 );
		RightCard.LocalRotation = Rotation.From( 130, -10, 180 );

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
}
