﻿using Sandbox;

namespace Poker.VR;

public partial class HandEntity : AnimatedEntity
{
	protected virtual string ModelPath => "";

	[Net] public HandEntity Other { get; set; }

	public bool GripPressed => InputHand.Grip > 0.5f;
	public bool TriggerPressed => InputHand.Trigger > 0.5f;

	public virtual Input.VrHand InputHand { get; }

	public override void Spawn()
	{
		SetModel( ModelPath );

		Transmit = TransmitType.Always;
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Transform = InputHand.Transform;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Transform = InputHand.Transform;
		Animate();
	}

	private void Animate()
	{
		SetAnimParameter( "Index", InputHand.GetFingerCurl( 1 ) );
		SetAnimParameter( "Middle", InputHand.GetFingerCurl( 2 ) );
		SetAnimParameter( "Ring", InputHand.GetFingerCurl( 3 ) );
		SetAnimParameter( "Thumb", InputHand.GetFingerCurl( 0 ) );
	}
}

public class LeftHand : HandEntity
{
	protected override string ModelPath => "models/vr/hands/handleft.vmdl";
	public override Input.VrHand InputHand => Input.VR.LeftHand;
}

public class RightHand : HandEntity
{
	protected override string ModelPath => "models/vr/hands/handright.vmdl";
	public override Input.VrHand InputHand => Input.VR.RightHand;
}
