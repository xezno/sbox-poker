namespace Poker;

public static class PokerUtils
{
	public static string GetMoveName( int currentBet, int playerBet )
	{
		if ( playerBet == 0 )
			return "Check";
		else if ( playerBet == currentBet )
			return "Call";
		else
			return "Raise";
	}
}
