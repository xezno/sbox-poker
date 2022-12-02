namespace Poker;

public partial class Player : AnimatedEntity
{
	[Net] public string AvatarData { get; set; }
	[Net] public float VoiceLevel { get; set; }
	[Net] public PlayerAnimator Animator { get; set; }

	[ConVar.Server( "poker_sv_starting_cash" )]
	public static float StartingCash { get; set; } = 1000f;

	public Camera Camera
	{
		get => Components.Get<Camera>();
		set => Components.Add( value );
	}

	public override void Spawn()
	{
		SetModel( "models/citizen/poker_citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = StartingCash;

		LeftCard = new CardEntity() { Owner = this };
		LeftCard.SetParent( this, "cards" );
		LeftCard.LocalPosition = 0;
		LeftCard.LocalRotation = Rotation.Identity;

		RightCard = new CardEntity() { Owner = this };
		RightCard.SetParent( this, "cards" );
		RightCard.LocalPosition = 0;
		RightCard.LocalRotation = Rotation.Identity;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Animator.Simulate( cl, this, null );

		if ( IsServer )
		{
			IsMyTurn = Game.Instance?.IsTurn( this ) ?? false;
		}

		SetEyeTransforms();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Animator.FrameSimulate( cl, this, null );

		SetEyeTransforms();
		SetBodyGroups();
	}

	private void SetBodyGroups()
	{
		if ( IsClient && IsLocalPawn )
		{
			SetBodyGroup( "Head", 1 );
		}
		else
		{
			SetBodyGroup( "Head", 0 );
		}
	}

	private void SetEyeTransforms()
	{
		var eyeTransform = GetAttachment( "eyes", false ) ?? default;
		EyeLocalPosition = eyeTransform.Position + Vector3.Up * 2f - Vector3.Forward * 4f;
		EyeLocalRotation = Input.Rotation;

		Position = Position.WithZ( 8 );
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( InputLayer.Evaluate( "community_cards" ) )
		{
			inputBuilder.ViewAngles = inputBuilder.OriginalViewAngles;
			inputBuilder.StopProcessing = true;
			return;
		}

		var inputAngles = inputBuilder.ViewAngles;
		var clampedAngles = new Angles(
			inputAngles.pitch.Clamp( -45, 45 ),
			inputAngles.yaw,
			inputAngles.roll
		);

		inputBuilder.ViewAngles = clampedAngles;
	}

	public override string ToString()
	{
		return $"Player '{Client.Name}'";
	}
}
