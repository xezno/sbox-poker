namespace Poker;

public partial class Player : BasePawn
{
	[Net] public string AvatarData { get; set; }
	[Net] public float VoiceLevel { get; set; }

	[ConVar.Server( "poker_sv_starting_cash" )]
	public static float StartingCash { get; set; } = 1000f;

	public override Ray AimRay
	{
		get
		{
			var eyeTransform = GetAttachment( "eyes" ) ?? default;

			return new Ray(
				eyeTransform.Position + Vector3.Up * 2f + Vector3.Right * 4f,
				ViewAngles.Forward
			);
		}
	}

	public Vector3 EyePosition => AimRay.Position;
	public Rotation EyeRotation => Rotation.LookAt( AimRay.Forward );

	private PlayerCamera Camera { get; set; }

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
		RightCard.LocalPosition = new Vector3( 0, -2, 0.1f );
		RightCard.LocalRotation = Rotation.From( 0, 15, 0 );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		ViewAngles = Rotation.Angles();

		Camera = new();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Animate();

		if ( Game.IsServer )
		{
			IsMyTurn = PokerGame.Instance?.IsTurn( this ) ?? false;
		}

		SetEyeTransforms();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Animate();
		Camera.Update();

		SetEyeTransforms();
		SetBodyGroups();
	}

	private void SetBodyGroups()
	{
		if ( Game.IsClient && IsLocalPawn )
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
		Position = Position.WithZ( 8 );
	}

	public override void BuildInput()
	{
		base.BuildInput();

		if ( InputLayer.Evaluate( "community_cards" ) )
		{
			Input.StopProcessing = true;
			return;
		}

		Camera.BuildInput();
	}

	public override string ToString()
	{
		return $"Player '{Client.Name}'";
	}

	private void Animate()
	{
		SetAnimParameter( "b_showcards", InputLayer.Evaluate( "your_cards" ) );

		// TODO: remove this ( test )
		if ( InputLayer.Evaluate( "emote.middle_finger" ) )
			SetAnimParameter( "action", (int)Actions.Game_Bet );
		else if ( InputLayer.Evaluate( "emote.thumbs_up" ) )
			SetAnimParameter( "action", (int)Actions.Game_Check );
		else if ( InputLayer.Evaluate( "emote.thumbs_down" ) )
			SetAnimParameter( "action", (int)Actions.Game_Fold );
		else if ( InputLayer.Evaluate( "emote.pump" ) )
			SetAnimParameter( "action", (int)Actions.Emote_Pump );
		else
			SetAnimParameter( "action", 0 );

		SetAnimParameter( "sit_pose", 0 );

		Vector3 lookPos = EyePosition + EyeRotation.Forward * 512;
		Vector3 emoteAimPos = EyePosition + EyeRotation.Forward * 512;
		emoteAimPos = emoteAimPos.WithZ( 128 );

		Vector3 cardsAimPos = EyePosition + EyeRotation.Forward * 512;
		cardsAimPos = cardsAimPos.WithZ( 128 );

		SetAnimLookAt( "aim_head", EyePosition, lookPos );
		SetAnimLookAt( "aim_emote", EyePosition, emoteAimPos );
		SetAnimLookAt( "aim_cards", EyePosition, cardsAimPos );
		SetAnimParameter( "aim_head_weight", 1.0f );
		SetAnimParameter( "aim_emote_weight", 1.0f );
		SetAnimParameter( "aim_cards_weight", 1.0f );

		if ( Game.IsClient && Client.IsValid )
		{
			SetAnimParameter( "voice", Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f );
		}
	}
}
