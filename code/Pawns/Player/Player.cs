namespace Poker;

[Prefab, Title( "Player" ), Category( "Poker" )]
public partial class Player : BasePawn
{
	[Net] public string AvatarData { get; set; }
	[Net] public float VoiceLevel { get; set; }
	[Net] public Actions CurrentAction { get; set; }

	[ConVar.Server( "poker_sv_starting_cash" )]
	public static float StartingCash { get; set; } = 1000f;

	private Vector3[] _offsets = new Vector3[]
	{
		new Vector3( 10, 0, -3 ),
		new Vector3( 10, 0, -4 ),
		new Vector3( 10, 0, -6 ),
		new Vector3( 10, 0, -2 ),
		new Vector3( 10, -1, -3 ),
		new Vector3( 10, 0, -2 )
	};

	public override Ray AimRay
	{
		get
		{
			var eyeTransform = GetAttachment( "eyes" ) ?? default;
			var position = eyeTransform.Position + Vector3.Up * 2f + Vector3.Right * 4f;
			var forward = ViewAngles.Forward;

			if ( Client.IsBot )
			{
				var offset = _offsets[Seat.SeatNumber];

				var lookPoint = Position + (Rotation * offset);
				forward = (lookPoint - Position).Normal;
			}

			return new Ray(
				position,
				forward
			);
		}
	}

	public Vector3 EyePosition => AimRay.Position;
	public Rotation EyeRotation => Rotation.LookAt( AimRay.Forward );

	private TimeSince timeSinceTurnChange;

	private PlayerCamera Camera { get; set; }

	public override void Spawn()
	{
		SetModel( "models/citizen/poker_citizen.vmdl" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Money = StartingCash;

		// WHAT THE FUCK!!! TAGS R SHIT??
		// WHY IS IT NOT "card,leftcard"!?!?!?!?
		LeftCard = Children.OfType<CardEntity>().First( x => x.Tags.Has( "card leftcard" ) );
		RightCard = Children.OfType<CardEntity>().First( x => x.Tags.Has( "card rightcard" ) );

		if ( LeftCard == null || RightCard == null )
		{
			Log.Error( "No cards for you, fuck off" );
		}
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
			var wasMyTurn = IsMyTurn;
			IsMyTurn = PokerGame.Instance?.IsTurn( this ) ?? false;

			if ( IsMyTurn != wasMyTurn )
			{
				timeSinceTurnChange = 0;
			}
		}

		// Hacky bot behaviour, just check after 1 second
		if ( Client.IsBot && Game.IsServer && IsMyTurn && timeSinceTurnChange > 1f )
		{
			Log.Info( $"{Client.IsBot} && {Game.IsClient} && {IsMyTurn}" );
			PokerGame.SubmitMove( this, Move.Bet, 0f );
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

	float _aimCardsWeight = 0.0f;
	float _aimEmoteWeight = 0.0f;
	float _aimHeadWeight = 1.0f;

	private void Animate()
	{
		SetAnimParameter( "action", (int)CurrentAction );

		_aimEmoteWeight = _aimEmoteWeight.LerpTo( (CurrentAction >= Actions.Emote_MiddleFinger) ? 1.0f : 0.0f, Time.Delta * 10f );

		SetAnimParameter( "sit_pose", 0 );

		Vector3 lookPos = EyePosition + EyeRotation.Forward * 512;
		Vector3 emoteAimPos = EyePosition + EyeRotation.Forward * 512;
		emoteAimPos = emoteAimPos.WithZ( 128 );

		Vector3 cardsAimPos = EyePosition + EyeRotation.Forward * 512;
		cardsAimPos = cardsAimPos.WithZ( 128 );

		SetAnimLookAt( "aim_head", EyePosition, lookPos );
		SetAnimLookAt( "aim_emote", EyePosition, emoteAimPos );
		SetAnimLookAt( "aim_cards", EyePosition, cardsAimPos );
		SetAnimParameter( "aim_head_weight", _aimHeadWeight );
		SetAnimParameter( "aim_emote_weight", _aimEmoteWeight );
		SetAnimParameter( "aim_cards_weight", _aimCardsWeight );

		if ( Game.IsClient && Client.IsValid )
		{
			SetAnimParameter( "voice", Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f );
		}
	}
}
