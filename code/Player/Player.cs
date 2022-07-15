using Sandbox;

namespace VrExample;

partial class Player : AnimatedEntity
{
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

		SetAnimProperties();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	public void SetAnimProperties()
	{
		if ( LifeState != LifeState.Alive )
			return;

		SetAnimParameter( "b_grounded", true );
		SetAnimParameter( "b_sit", true );

		Vector3 aimPos = EyePosition + Rotation.Forward * 256;
		Vector3 lookPos = Input.VR.Head.Position + Input.VR.Head.Rotation.Forward * 256;

		SetAnimLookAt( "aim_eyes", lookPos );
		SetAnimLookAt( "aim_head", lookPos );
		SetAnimLookAt( "aim_body", aimPos );

		SetAnimParameter( "holdtype", 0 );
		SetAnimParameter( "aim_body_weight", 0.5f );
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		// You will probably need to tweak these depending on your use case
		setup.ZNear = 1;
		setup.ZFar = 25000;

		base.PostCameraSetup( ref setup );
	}
}
