using Poker.Backend;
using Sandbox;
using SandboxEditor;

namespace Poker;

public partial class Player : AnimatedEntity
{
	[Net, Local] public Hand Hand { get; set; }
	[Net] public string AvatarData { get; set; }
	[Net] public float Money { get; set; }
	[Net] public bool IsMyTurn { get; set; }
	[Net] public bool HasFolded { get; set; }

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = 1000;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsServer )
		{
			IsMyTurn = PokerControllerEntity.Instance?.IsTurn( this ) ?? false;
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

		SetAnimParameter( "left_hand_ik.position", new Vector3( 10, 6, 32 ) );
		SetAnimParameter( "left_hand_ik.rotation", new Angles( 0, -45, 60 ).ToRotation() );

		SetAnimParameter( "right_hand_ik.position", new Vector3( 8, -6, 32 ) );
		SetAnimParameter( "right_hand_ik.rotation", new Angles( 0, 45, 120 ).ToRotation() );

		SetAnimParameter( "holdtype", 0 );
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
