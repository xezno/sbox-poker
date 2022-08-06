using Sandbox;
using System.Linq;

namespace Poker;

public class Camera : CameraMode
{
	private float UserFOV = 60f;

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = pawn.EyePosition;
		Rotation = Rotation.Identity;
	}

	public enum Targets
	{
		FirstPerson,
		ThirdPerson,
		CommunityCards,
		YourCards,
	}

	public Targets GetCameraTarget()
	{
		Targets cameraTarget = Targets.FirstPerson;

		if ( InputLayer.Evaluate( "community_cards" ) > 0.1f )
			cameraTarget = Targets.CommunityCards;
		else if ( InputLayer.Evaluate( "your_cards" ) > 0.1f )
			cameraTarget = Targets.YourCards;

		return cameraTarget;
	}

	public override void Update()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		ZNear = 1;
		ZFar = 5000;

		float targetFOV;
		Vector3 targetPosition;
		Rotation targetRotation;

		switch ( GetCameraTarget() )
		{
			case Targets.CommunityCards:
				{
					var communityCardSpawn = Entity.All.OfType<CommunityCardSpawn>().FirstOrDefault();
					var communityCardSpawnPos = communityCardSpawn?.Position ?? default;
					var lookDir = (communityCardSpawnPos - pawn.EyePosition).Normal;
					targetRotation = Rotation.LookAt( lookDir );
					targetPosition = pawn.EyePosition + Rotation.Forward * 8f;
					targetFOV = 40f;

					Viewer = pawn;
					break;
				}
			case Targets.YourCards:
				{
					targetRotation = pawn.Rotation * Rotation.From( 60, 0, 0 );
					targetPosition = pawn.EyePosition + Rotation.Forward * 8f;
					targetFOV = 60f;

					Viewer = pawn;
					break;
				}
			case Targets.ThirdPerson:
				{
					targetPosition = pawn.EyePosition + pawn.EyeRotation.Backward * 96f + pawn.EyeRotation.Right * 8f;
					targetRotation = pawn.EyeRotation;
					targetFOV = 60f;

					Viewer = null;
					break;
				}
			default:
				{
					targetPosition = pawn.EyePosition;
					targetRotation = pawn.EyeRotation;
					targetFOV = UserFOV;

					Viewer = pawn;
					break;
				}
		}

		Position = Position.LerpTo( targetPosition, 10f * Time.Delta );
		Rotation = Rotation.LerpTo( targetRotation, 10f * Time.Delta );
		FieldOfView = FieldOfView.LerpTo( targetFOV, 10f * Time.Delta );

		DebugOverlay.ScreenText( $"Position: {Position}", 0 );
		DebugOverlay.ScreenText( $"Rotation: {Rotation}", 1 );
		DebugOverlay.ScreenText( $"FOV: {FieldOfView}", 2 );
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		var inputAngles = inputBuilder.ViewAngles;
		var clampedAngles = new Angles(
			inputAngles.pitch.Clamp( -60, 60 ),
			inputAngles.yaw,//.yaw.Clamp( -45, 45 ),
			inputAngles.roll
		);

		inputBuilder.ViewAngles = clampedAngles;

		UserFOV -= inputBuilder.MouseWheel * 10f;
		UserFOV = UserFOV.Clamp( 10, 90 );
	}
}
