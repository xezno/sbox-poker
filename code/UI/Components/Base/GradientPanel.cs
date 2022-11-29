using Sandbox.UI;

namespace Poker.UI;

[StyleSheet( "/UI/_Theme.scss" )]
public class GradientPanel : Panel
{
	public GradientPanel()
	{
		AddClass( "gradient-panel" );
	}
}
