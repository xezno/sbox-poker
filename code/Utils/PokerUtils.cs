namespace Poker;

public static class PokerUtils
{
	public static string GetMoveName( int playerBet )
	{
		if ( playerBet == 0 )
			return "Check";
		else if ( playerBet == Game.Instance.MinimumBet )
			return "Call";
		else
			return "Raise";
	}

	public static int FindBestHand( Card[][] hands, out HandRank[] ranks )
	{
		ranks = new HandRank[hands.Length];

		int winner = 0;
		var winnerRank = HandRank.HighCard;

		for ( int i = 0; i < hands.Length; i++ )
		{
			ranks[i] = RankPokerHand( hands[i], out _ );

			if ( ranks[i] > winnerRank )
			{
				winner = i;
				winnerRank = ranks[i];
			}
		}

		return winner;
	}

	public static HandRank RankPokerHand( Card[] cards, out int score )
	{
		var straightCards = cards.Select( x => x.Value ).OrderBy( x => x ).ToArray();
		var cardGroups = cards.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ).ToArray();

		bool isFlush;
		bool isStraight;
		bool isStraightFlush;

		var cardScores = cards.Select( x => (int)x.Value );
		score = cardScores.Sum();

		if ( isFlush = cards.GroupBy( x => x.Suit ).Any( x => x.Count() == 5 ) )
			return HandRank.Flush;

		if ( isStraight = IsStraight( straightCards ) )
			return HandRank.Straight;

		if ( isStraightFlush = isFlush && IsStraight( straightCards ) )
			return HandRank.StraightFlush;

		if ( isStraightFlush && straightCards.Last() == Value.Ace )
			return HandRank.RoyalFlush;

		if ( cardGroups.First().Count() == 4 )
			return HandRank.FourOfAKind;

		if ( cardGroups.First().Count() == 3 && cardGroups.Last().Count() == 2 )
			return HandRank.FullHouse;

		if ( cardGroups.First().Count() == 3 )
			return HandRank.ThreeOfAKind;

		if ( cardGroups.First().Count() == 2 && cardGroups.Last().Count() == 2 )
			return HandRank.TwoPair;

		if ( cardGroups.First().Count() == 2 )
			return HandRank.Pair;

		return HandRank.HighCard;
	}

	private static bool IsStraight( Value[] values )
	{
		if ( values.Length != 5 )
			return false;

		var diff = values.Zip( values.Skip( 1 ), ( x, y ) => y - x ).Distinct().ToArray();
		return diff.Length == 1;
	}

	public static string ToDisplayString( this HandRank rank )
	{
		var res = "";
		string str = rank.ToString();

		for ( int i = 0; i < str.Length; i++ )
		{
			char c = str[i];

			if ( char.IsUpper( c ) && i != 0 )
				res += " ";

			res += c;
		}

		return res;
	}
}
