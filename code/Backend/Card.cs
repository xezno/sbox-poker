namespace Poker.Backend;

public struct Card
{
	public Suit Suit { get; }
	public Value Value { get; }

	public Card( Suit suit, Value value )
	{
		Suit = suit;
		Value = value;
	}

	public override string ToString()
	{
		return $"{Value} of {Suit}";
	}

	public string GetFilename()
	{
		return $"/ui/cards/{Value}_{Suit}.png";
	}

	public string ToShortString()
	{
		string suitString = "♠️";
		switch ( Suit )
		{
			case Suit.Diamonds:
				suitString = "♦️";
				break;
			case Suit.Hearts:
				suitString = "♥️";
				break;
			case Suit.Spades:
				suitString = "♠️";
				break;
			case Suit.Clubs:
				suitString = "♣️";
				break;
		}

		string valueString = "";
		switch ( Value )
		{
			case Value.Ace:
				valueString = "A";
				break;
			case Value.King:
				valueString = "K";
				break;
			case Value.Queen:
				valueString = "Q";
				break;
			case Value.Jack:
				valueString = "J";
				break;
			case Value.Ten:
				valueString = "10";
				break;
			case Value.Nine:
				valueString = "9";
				break;
			case Value.Eight:
				valueString = "8";
				break;
			case Value.Seven:
				valueString = "7";
				break;
			case Value.Six:
				valueString = "6";
				break;
			case Value.Five:
				valueString = "5";
				break;
			case Value.Four:
				valueString = "4";
				break;
			case Value.Three:
				valueString = "3";
				break;
			case Value.Two:
				valueString = "2";
				break;
		}

		return $"{suitString}{valueString}";
	}
}
