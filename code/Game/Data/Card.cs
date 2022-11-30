namespace Poker;

public struct Card
{
	public Suit Suit { get; set; }

	public Value Value { get; set; }

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
		var valueLUT = new Dictionary<Value, string>()
		{
			{ Value.Ace, "A" },
			{ Value.King, "K" },
			{ Value.Queen, "Q" },
			{ Value.Jack, "J" },
			{ Value.Ten, "10" },
			{ Value.Nine, "9" },
			{ Value.Eight, "8" },
			{ Value.Seven, "7" },
			{ Value.Six, "6" },
			{ Value.Five, "5" },
			{ Value.Four, "4" },
			{ Value.Three, "3" },
			{ Value.Two, "2" },
		};

		var valueString = valueLUT[Value];

		var suitLUT = new Dictionary<Suit, string>()
		{
			{ Suit.Diamonds, "♦️" },
			{ Suit.Hearts, "♥️" },
			{ Suit.Spades, "♠️" },
			{ Suit.Clubs, "♣️" },
		};

		var suitString = suitLUT[Suit];

		return $"{suitString}{valueString}";
	}
}
