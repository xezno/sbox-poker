using Sandbox.UI;

namespace Poker.UI;

[UseTemplate]
internal class Controls : Panel
{
	public Panel CameraControlsPanel { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
		{
			CameraControlsPanel.SetClass( "visible", false );
			return;
		}

		CameraControlsPanel.SetClass( "visible", true );
	}
}
