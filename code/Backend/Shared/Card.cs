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
		var fileName = $"/ui/cards/{Value}_{Suit}";

		// Get second variant for king/queen/jack
		if ( Value == Value.King || Value == Value.Queen || Value == Value.Jack )
			fileName += "2";

		return $"{fileName}.png";
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

	public Card FromShortString( string shortString )
	{
		string suitString = shortString.Substring( 0, 1 );
		string valueString = shortString.Substring( 1 );

		Suit suit = Suit.Spades;
		Value value = Value.Two;

		switch ( suitString )
		{
			case "♦️":
				suit = Suit.Diamonds;
				break;
			case "♥️":
				suit = Suit.Hearts;
				break;
			case "♠️":
				suit = Suit.Spades;
				break;
			case "♣️":
				suit = Suit.Clubs;
				break;
		}

		switch ( valueString )
		{
			case "A":
				value = Value.Ace;
				break;
			case "K":
				value = Value.King;
				break;
			case "Q":
				value = Value.Queen;
				break;
			case "J":
				value = Value.Jack;
				break;
			case "10":
				value = Value.Ten;
				break;
			case "9":
				value = Value.Nine;
				break;
			case "8":
				value = Value.Eight;
				break;
			case "7":
				value = Value.Seven;
				break;
			case "6":
				value = Value.Six;
				break;
			case "5":
				value = Value.Five;
				break;
			case "4":
				value = Value.Four;
				break;
			case "3":
				value = Value.Three;
				break;
			case "2":
				value = Value.Two;
				break;
		}

		return new Card( suit, value );
	}
}
