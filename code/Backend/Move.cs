namespace Poker.Backend;

public enum Move
{
	Fold = 0,
	Check = 1,
	Call = 2,

	/// <summary>
	/// Also raise, open
	/// </summary>
	Bet = 3,
}
