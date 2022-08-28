namespace Poker;

public class SpectatorCamera : CameraMode
{
	private Angles LookAngles;
	private Vector3 MoveInput;

	private Vector3 TargetPos;
	private Rotation TargetRot;
	private float TargetFOV;

	private float FlySpeed => 150f;
	private float moveMul = 1.0f;

	public SpectatorCamera()
	{
		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;
		TargetFOV = 90f;

		Position = TargetPos;
		Rotation = TargetRot;
		FieldOfView = TargetFOV;

		LookAngles = Rotation.Angles();
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( !player.IsValid() )
			return;

		Viewer = null;

		Move();
	}

	public override void BuildInput( InputBuilder input )
	{
		MoveInput = input.AnalogMove;

		moveMul = 1;

		if ( input.Down( InputButton.Run ) )
			moveMul = 5;
		if ( input.Down( InputButton.Duck ) )
			moveMul = 0.2f;

		LookAngles += input.AnalogLook;
		LookAngles.roll = 0;

		TargetFOV -= input.MouseWheel * 4f;
		TargetFOV = TargetFOV.Clamp( 30f, 100f );

		input.ClearButton( InputButton.PrimaryAttack );
		input.StopProcessing = true;
	}

	void Move()
	{
		var mv = MoveInput.Normal * FlySpeed * RealTime.Delta * Rotation * moveMul;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta );
		FieldOfView = FieldOfView.LerpTo( TargetFOV, 10 * RealTime.Delta );
	}
}
