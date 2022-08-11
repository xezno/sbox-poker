using Sandbox;

namespace Poker.VR;

public partial class HeadEntity : AnimatedEntity
{
	public override void Spawn()
	{
		SetModel( "models/vr/head/vrhead.vmdl" );

		Transmit = TransmitType.Always;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Transform = Input.VR.Head;
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Transform = Input.VR.Head;
	}
}
