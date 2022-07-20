using Poker.Backend;
using Sandbox.UI;

namespace Poker.UI;

[UseTemplate]
internal class Controls : Panel
{
	//
	// TODO: this shit probably shouldn't be in here
	//
	private bool submitPressedLastFrame;

	public override void Tick()
	{
		base.Tick();

		bool submitPressed = InputLayer.Evaluate( "submit" ) > 0.1f;
		if ( !submitPressedLastFrame && submitPressed )
		{
			PokerControllerEntity.SubmitMove( Move.Check, 0.0f );
		}

		submitPressedLastFrame = submitPressed;
	}
}
