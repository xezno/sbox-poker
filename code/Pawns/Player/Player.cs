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
		new Vector3( 10, 0, -5 ),
		new Vector3( 10, 0, -6 ),
		new Vector3( 10, 0, -7 ),
		new Vector3( 10, 0, -4 ),
		new Vector3( 10, -1, -5 ),
		new Vector3( 10, 0, -4 )
	};

	public override Ray AimRay
	{
		get
		{
			var eyeTransform = GetAttachment( "eyes" ) ?? default;
			var position = eyeTransform.Position + Rotation.Up * 2f + Rotation.Backward * 4f;
			var forward = ViewAngles.Forward;

			if ( Client.IsBot )
			{
				var offset = _offsets[Seat.SeatNumber];

				if ( _botLookingAway )
				{
					// Look at next seat
					var nextSeat = Seat.SeatNumber + 1;
					if ( nextSeat >= Game.Clients.Count )
						nextSeat = 0;

					var nextSeatEntity = Entity.All.OfType<SeatEntity>().FirstOrDefault( x => x.SeatNumber == nextSeat );
					if ( nextSeatEntity != null )
					{
						var target = nextSeatEntity.Position.WithZ( position.z ) - position;

						float time = (_botAnimateDelay - _timeSinceBotAnimate.Relative);
						time = _botAnimateDelay - time;
						time -= (_botAnimateDelay / 2f);

						float t = time.LerpInverse( 0, 1 );
						target = offset.LerpTo( target, 0.1f * t ).WithZ( 0 );

						offset = offset.LerpTo( target, 0.2f * t );
					}
				}

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

	private bool IsDevCamEnabled()
	{
		var camera = Client.Components.Get<DevCamera>( true );
		return (camera?.Enabled ?? false);
	}

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

			PlayRandomAudio();
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
		if ( Game.IsClient && IsLocalPawn && !IsDevCamEnabled() )
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
		if ( IsDevCamEnabled() )
			return;

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

	TimeSince _timeSinceBotAnimate = 0;
	float _botAnimateDelay = 5;

	bool _botLookingAway = true;

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

		if ( Client.IsBot )
		{
			_aimHeadWeight = _aimHeadWeight.LerpTo( 1.0f, Time.Delta );
			_botLookingAway = false;

			if ( _timeSinceBotAnimate > _botAnimateDelay / 2f )
			{
				// Look somewhere else
				_botLookingAway = true;
			}

			if ( _timeSinceBotAnimate > _botAnimateDelay )
			{
				_timeSinceBotAnimate = 0;
				_botAnimateDelay = Game.Random.Float( 10, 20 );
			}
		}

		if ( Game.IsClient && Client.IsValid )
		{
			SetAnimParameter( "voice", Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f );
		}
	}
}
