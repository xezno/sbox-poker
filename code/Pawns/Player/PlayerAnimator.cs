﻿namespace Poker;

public class PlayerAnimator : PawnAnimator
{
	public override void Simulate()
	{
		if ( Pawn.LifeState != LifeState.Alive )
			return;

		SetAnimParameter( "b_showcards", InputLayer.Evaluate( "your_cards" ) );

		// TODO: remove this ( test )
		if ( InputLayer.Evaluate( "emote.middle_finger" ) )
			SetAnimParameter( "action", (int)Action.Emote_MiddleFinger );
		else if ( InputLayer.Evaluate( "emote.thumbs_up" ) )
			SetAnimParameter( "action", (int)Action.Emote_ThumbsUp );
		else if ( InputLayer.Evaluate( "emote.thumbs_down" ) )
			SetAnimParameter( "action", (int)Action.Emote_ThumbsDown );
		else if ( InputLayer.Evaluate( "emote.pump" ) )
			SetAnimParameter( "action", (int)Action.Emote_Pump );
		else
			SetAnimParameter( "action", 0 );

		SetAnimParameter( "sit_pose", 0 );

		Vector3 lookPos = Pawn.EyePosition + EyeRotation.Forward * 512;
		Vector3 emoteAimPos = Pawn.EyePosition + EyeRotation.Forward * 512;
		emoteAimPos = emoteAimPos.WithZ( 128 );

		SetLookAt( "aim_head", lookPos );
		SetLookAt( "aim_emote", emoteAimPos );
		SetAnimParameter( "aim_head_weight", 1.0f );
		SetAnimParameter( "aim_emote_weight", 1.0f );

		if ( Host.IsClient && Client.IsValid() )
		{
			SetAnimParameter( "voice", Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f );
			DebugOverlay.ScreenText( Client.VoiceLevel.ToString() );
		}
	}
}