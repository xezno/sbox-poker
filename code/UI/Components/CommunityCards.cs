using Sandbox.UI;

namespace Poker.UI;

[UseTemplate]
internal class CommunityCards : Panel
{
	private TimeSince timeSinceLastRefresh = 0;

	public override void Tick()
	{
		base.Tick();

		if ( timeSinceLastRefresh < 1 )
			return;

		var cardPanels = Children.OfType<Card>().ToList();
		Game instance;

		if ( (instance = Game.Instance) == null )
		{
			cardPanels.ForEach( x => x.Reset() );
			return;
		}

		for ( int i = 0; i < 5; ++i )
		{
			var cardPanel = cardPanels[i];

			if ( instance.CommunityCards.Count <= i )
			{
				cardPanel.Reset();
			}
			else
			{
				var card = instance.CommunityCards[i];
				cardPanel.SetCard( card.Suit, card.Value );
			}
		}

		timeSinceLastRefresh = 0;
	}
}
