using Sandbox;

namespace Poker;

partial class Player : AnimatedEntity
{
	[Net] public string AvatarData { get; set; }

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		UpdateEyes();

		if ( IsServer )
			SetAnimProperties();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		UpdateEyes();
	}

	private void UpdateEyes()
	{
		EyeLocalPosition = Vector3.Up * 58f;
		EyeLocalRotation = Input.Rotation;

		Position = Position.WithZ( 3 );
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

		SetAnimParameter( "left_hand_ik.position", new Vector3( 16, 8, 32 ) );
		SetAnimParameter( "left_hand_ik.rotation", new Angles( 0, -45, 60 ).ToRotation() );

		SetAnimParameter( "right_hand_ik.position", new Vector3( 16, -8, 32 ) );
		SetAnimParameter( "right_hand_ik.rotation", new Angles( 0, 45, 120 ).ToRotation() );

		SetAnimParameter( "holdtype", 0 );
		SetAnimParameter( "aim_body_weight", 0.5f );
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		setup.ZNear = 1;
		setup.ZFar = 25000;

		setup.Position = EyePosition;
		setup.Rotation = EyeRotation;

		setup.Viewer = this;

		base.PostCameraSetup( ref setup );
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		var inputAngles = inputBuilder.ViewAngles;
		var clampedAngles = new Angles(
			inputAngles.pitch.Clamp( -45, 45 ),
			inputAngles.yaw,//.yaw.Clamp( -45, 45 ),
			inputAngles.roll
		);

		inputBuilder.ViewAngles = clampedAngles;
	}
}
