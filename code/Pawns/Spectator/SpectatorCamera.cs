namespace Poker;

public class SpectatorCamera
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
		TargetPos = Camera.Position;
		TargetRot = Camera.Rotation;
		TargetFOV = Screen.CreateVerticalFieldOfView( 90f );

		LookAngles = Camera.Rotation.Angles();
	}

	public void Update()
	{
		var player = Local.Client;
		if ( !player.IsValid() )
			return;

		Camera.FirstPersonViewer = null;

		Move();
	}

	public void BuildInput()
	{
		MoveInput = Input.AnalogMove;

		moveMul = 1;

		if ( Input.Down( InputButton.Run ) )
			moveMul = 5;
		if ( Input.Down( InputButton.Duck ) )
			moveMul = 0.2f;

		LookAngles += Input.AnalogLook;
		LookAngles.roll = 0;

		TargetFOV -= Input.MouseWheel * 4f;
		TargetFOV = TargetFOV.Clamp( 30f, 100f );

		Input.ClearButton( InputButton.PrimaryAttack );
		Input.StopProcessing = true;
	}

	void Move()
	{
		var mv = MoveInput.Normal * FlySpeed * RealTime.Delta * Camera.Rotation * moveMul;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Camera.Position = Vector3.Lerp( Camera.Position, TargetPos, 10 * RealTime.Delta );
		Camera.Rotation = Rotation.Slerp( Camera.Rotation, TargetRot, 10 * RealTime.Delta );
		Camera.FieldOfView = Camera.FieldOfView.LerpTo( TargetFOV, 10 * RealTime.Delta );
	}
}
