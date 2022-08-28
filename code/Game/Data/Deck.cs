using System;

namespace Poker;

public class Deck
{
	private List<Card> cardList;

	public Deck()
	{
		cardList = new List<Card>();

		for ( var suitIndex = 0; suitIndex < 4; ++suitIndex )
		{
			for ( var valueIndex = 0; valueIndex < 13; ++valueIndex )
			{
				cardList.Add( new Card( (Suit)suitIndex, (Value)valueIndex ) );
			}
		}

		Shuffle();
	}

	public bool IsEmpty()
	{
		return cardList.Count == 0;
	}

	private void Shuffle()
	{
		var random = new Random();
		cardList = cardList.OrderBy( x => Guid.NewGuid() ).ToList();
	}

	public IEnumerable<Card> Draw( int count )
	{
		var cardListClone = new List<Card>( cardList );
		var cards = cardListClone.TakeLast( count );

		cardList.RemoveRange( cardList.Count - count, count );

		return cards;
	}

	public List<Card> CreateHand()
	{
		var cards = Draw( 2 );
		return cards.ToList();
	}
}
