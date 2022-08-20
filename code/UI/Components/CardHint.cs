using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Poker.UI;

[UseTemplate]
internal class CardHint : Panel
{
	public Label CardHintLabel { get; set; }

	public CardHint()
	{
		BindClass( "visible", () => InputLayer.Evaluate( "your_cards" ) );
	}

	public override void Tick()
	{
		base.Tick();

		bool yourCardsButton = InputLayer.Evaluate( "your_cards" );
		bool communityCardsButton = InputLayer.Evaluate( "community_cards" );

		SetClass( "visible", yourCardsButton || communityCardsButton );

		if ( Local.Pawn is not Player player )
			return;

		if ( player.Hand == null )
			return;

		IEnumerable<string> cards;

		if ( yourCardsButton )
			cards = player.Hand.Select( x => x.ToString() );
		else if ( communityCardsButton )
			cards = Game.Instance.CommunityCards.Select( x => x.ToString() );
		else
			return;

		CardHintLabel.Text = string.Join( ", ", cards );
	}
}
