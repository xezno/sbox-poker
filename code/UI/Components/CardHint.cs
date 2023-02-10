using Sandbox.UI;

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

		if ( Game.LocalPawn is not Player player )
			return;

		if ( player.Hand == null )
			return;

		IEnumerable<string> cards;

		if ( yourCardsButton )
			cards = player.Hand.Select( x => x.ToString() );
		else if ( communityCardsButton )
			cards = PokerGame.Instance.CommunityCards.Select( x => x.ToString() );
		else
			return;

		CardHintLabel.Text = string.Join( ", ", cards );
	}
}
