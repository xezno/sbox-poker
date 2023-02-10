namespace Poker;

public class PlayerCamera
{
	private float UserFOV = 70f;
	private float fac = 1.0f;

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

	public void Update()
	{
		var pawn = Game.LocalPawn as Player;
		if ( pawn == null ) return;

		Camera.ZNear = 1;
		Camera.ZFar = 5000;

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
					targetFOV = Screen.CreateVerticalFieldOfView( 50f );

					fac = fac.LerpTo( 1.0f, 50f * Time.Delta );

					Camera.FirstPersonViewer = pawn;
					break;
				}
			default:
				{
					targetPosition = pawn.EyePosition;
					targetRotation = pawn.EyeRotation;
					targetFOV = Screen.CreateVerticalFieldOfView( UserFOV );

					fac = fac.LerpTo( 0.0f, .5f * Time.Delta );

					Camera.FirstPersonViewer = pawn;
					break;
				}
		}

		Camera.Position = Camera.Position.LerpTo( targetPosition, 10f * Time.Delta );
		Camera.Rotation = Camera.Rotation.LerpTo( targetRotation, 10f * Time.Delta );

		Camera.Position = Camera.Position.LerpTo( pawn.EyePosition, 1.0f - fac );
		Camera.Rotation = Camera.Rotation.LerpTo( pawn.EyeRotation, 1.0f - fac );

		Camera.FieldOfView = Camera.FieldOfView.LerpTo( targetFOV, 10f * Time.Delta );
	}

	public void BuildInput()
	{
		if ( Input.StopProcessing )
			return;

		UserFOV -= Input.MouseWheel * 10f;
		UserFOV = UserFOV.Clamp( 10, 90 );
	}
}
