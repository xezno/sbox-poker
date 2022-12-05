namespace Poker;

public class Camera : CameraMode
{
	private float UserFOV = 70f;
	private float fac = 1.0f;

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
		CommunityCards
	}

	public Targets GetCameraTarget()
	{
		Targets cameraTarget = Targets.FirstPerson;

		if ( InputLayer.Evaluate( "community_cards" ) )
			cameraTarget = Targets.CommunityCards;

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
					targetPosition = pawn.EyePosition;
					targetFOV = 50f;

					fac = fac.LerpTo( 1.0f, 50f * Time.Delta );

					Viewer = pawn;
					break;
				}
			default:
				{
					targetPosition = pawn.EyePosition;
					targetRotation = pawn.EyeRotation;
					targetFOV = UserFOV;

					fac = fac.LerpTo( 0.0f, .5f * Time.Delta );

					Viewer = pawn;
					break;
				}
		}

		Position = Position.LerpTo( targetPosition, 10f * Time.Delta );
		Rotation = Rotation.LerpTo( targetRotation, 10f * Time.Delta );

		Position = Position.LerpTo( pawn.EyePosition, 1.0f - fac );
		Rotation = Rotation.LerpTo( pawn.EyeRotation, 1.0f - fac );

		FieldOfView = FieldOfView.LerpTo( targetFOV, 10f * Time.Delta );
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( inputBuilder.StopProcessing )
			return;

		UserFOV -= inputBuilder.MouseWheel * 10f;
		UserFOV = UserFOV.Clamp( 10, 90 );
	}
}
