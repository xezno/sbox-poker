using Sandbox.UI;

namespace Poker.UI;

[UseTemplate]
internal class Pots : Panel
{
	public PokerLabel PotValueLabel { get; set; }

	public override void Tick()
	{
		base.Tick();

		Game instance;

		if ( (instance = Game.Instance) == null )
			return;

		var potValue = instance.Pot;
		PotValueLabel.Text = $"${potValue}";
	}
}
