using Poker.Backend;
using Sandbox;
using Sandbox.UI;
using System;

namespace Poker.UI;

[UseTemplate]
internal class Controls : Panel
{
	//
	// TODO: this shit probably shouldn't be in here
	//
	private bool submitPressedLastFrame;
	private bool foldPressedLastFrame;
	private bool allInPressedLastFrame;
	private float rawBet;
	private int roundedBet;

	public InputHint ActionHint { get; set; }
	public Panel PlayControlsPanel { get; set; }

	private float incrementRate => 50f;
	private int snapRate => 10;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		PlayControlsPanel.SetClass( "visible", player.IsMyTurn );

		if ( !player.IsMyTurn )
		{
			rawBet = 0;
			return;
		}

		ProcessInputs( out var submitPressed, out var foldPressed, out var betDelta, out var allInPressed );
		ProcessSubmitInput( submitPressed );
		ProcessFoldInput( foldPressed );
		ProcessBetInput( betDelta );
		ProcessAllInInput( allInPressed );

		submitPressedLastFrame = submitPressed;
	}

	private void ProcessAllInInput( bool allInPressed )
	{
		if ( Local.Pawn is not Player player )
			return;

		if ( !allInPressedLastFrame && allInPressed )
			PokerControllerEntity.SubmitMove( Move.Bet, player.Money );
	}

	private void ProcessSubmitInput( bool submitPressed )
	{
		if ( !submitPressedLastFrame && submitPressed )
			PokerControllerEntity.SubmitMove( Move.Bet, roundedBet );
	}

	private void ProcessFoldInput( bool foldPressed )
	{
		if ( !foldPressedLastFrame && foldPressed )
			PokerControllerEntity.SubmitMove( Move.Fold, 0 );
	}

	private void ProcessBetInput( float betDelta )
	{
		if ( MathF.Abs( betDelta ) > 0.5f )
			rawBet += betDelta * Time.Delta * incrementRate;

		rawBet = rawBet.Clamp( PokerControllerEntity.Instance.MinimumBet, 5000 );
		roundedBet = snapRate * (rawBet.CeilToInt() / snapRate);

		var action = PokerUtils.GetMoveName( roundedBet );
		ActionHint.ActionLabel.Text = $"{action} (${roundedBet})";
	}

	private void ProcessInputs( out bool submitPressed, out bool foldPressed, out float betDelta, out bool allInPressed )
	{
		submitPressed = InputLayer.Evaluate( "submit" );
		foldPressed = InputLayer.Evaluate( "fold" );
		betDelta = InputLayer.EvaluateRaw( "adjust_amount" );
		allInPressed = InputLayer.Evaluate( "all_in" );
	}
}
