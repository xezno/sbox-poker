namespace Poker;

public partial class BasePawn : AnimatedEntity
{
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		base.BuildInput();

		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;

		var clampedAngles = new Angles(
			viewAngles.pitch.Clamp( -45, 45 ),
			viewAngles.yaw,
			0
		);


		ViewAngles = clampedAngles.Normal;
	}
}
