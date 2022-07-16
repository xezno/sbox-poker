namespace Poker.Backend;

public class Card
{
	/// <summary>
	/// This is used to display a 'blank' card, i.e. if we don't have any info from the server yet.
	/// Mainly used to hide other players' cards from each other before they're supposed to be revealed.
	/// </summary>
	public bool IsEmpty { get; }

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
}
