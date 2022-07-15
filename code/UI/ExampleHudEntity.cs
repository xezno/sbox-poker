using Sandbox;
using Sandbox.UI;

namespace VrExample;

public class ExampleHudEntity : HudEntity<RootPanel>
{
	public ExampleHudEntity()
	{
		if ( IsClient )
		{
			// Just display the HUD on-screen
			RootPanel.SetTemplate( "/Code/UI/ExampleHud.html" );
		}
	}
}
