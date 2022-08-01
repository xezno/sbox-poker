using Sandbox;
using System.Linq;

namespace Poker;

partial class Player
{
	private Vector3 CameraPosition;
	private Rotation CameraRotation;
	private float CameraFOV;

	private enum CameraTargets
	{
		FirstPerson,
		ThirdPerson,
		CommunityCards,
		YourCards,
	}

	private void SetEyeTransforms()
	{
		EyeLocalPosition = Vector3.Up * 58f;
		EyeLocalRotation = Input.Rotation;

		Position = Position.WithZ( 3 );
	}

	private CameraTargets GetCameraTarget()
	{
		CameraTargets cameraTarget = CameraTargets.FirstPerson;
		if ( InputLayer.Evaluate( "community_cards" ) > 0.1f )
			cameraTarget = CameraTargets.CommunityCards;
		else if ( InputLayer.Evaluate( "your_cards" ) > 0.1f )
			cameraTarget = CameraTargets.YourCards;

		return cameraTarget;
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		setup.ZNear = 1;
		setup.ZFar = 5000;

		float targetFOV;
		Vector3 targetPosition;
		Rotation targetRotation;

		switch ( GetCameraTarget() )
		{
			case CameraTargets.CommunityCards:
				{
					var communityCardSpawn = Entity.All.OfType<CommunityCardSpawn>().FirstOrDefault();
					var communityCardSpawnPos = communityCardSpawn?.Position ?? default;
					var lookDir = (communityCardSpawnPos - EyePosition).Normal;
					targetRotation = Rotation.LookAt( lookDir );
					targetPosition = EyePosition + CameraRotation.Forward * 8f;
					targetFOV = 40f;

					setup.Viewer = this;
					break;
				}
			case CameraTargets.YourCards:
				{
					targetRotation = Rotation * Rotation.From( 60, 0, 0 );
					targetPosition = EyePosition + CameraRotation.Forward * 8f;
					targetFOV = 60f;

					setup.Viewer = this;
					break;
				}
			case CameraTargets.ThirdPerson:
				{
					targetPosition = EyePosition + EyeRotation.Backward * 96f + EyeRotation.Right * 8f;
					targetRotation = EyeRotation;
					targetFOV = 60f;

					setup.Viewer = null;
					break;
				}
			default:
				{
					targetPosition = EyePosition;
					targetRotation = EyeRotation;
					targetFOV = 80f;

					setup.Viewer = this;
					break;
				}
		}

		CameraPosition = CameraPosition.LerpTo( targetPosition, 10f * Time.Delta );
		CameraRotation = CameraRotation.LerpTo( targetRotation, 10f * Time.Delta );
		CameraFOV = CameraFOV.LerpTo( targetFOV, 10f * Time.Delta );

		setup.Position = CameraPosition;
		setup.Rotation = CameraRotation;
		setup.FieldOfView = CameraFOV;

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
