namespace Poker;
partial class PokerGame
{
	public HandRank RankPlayerHand( Player player, out int score )
	{
		// TODO
		// For each 5 card combination in (communitycards + playerHand) calculate the rank

		if ( CommunityCards == null || CommunityCards.Count < 3 )
		{
			score = -1;
			return HandRank.None;
		}

		var playerHand = player.Hand;

		if ( playerHand == null )
		{
			score = -1;
			return HandRank.None;
		}

		var cards = playerHand.ToArray().Concat( CommunityCards ).ToArray();

		return PokerUtils.RankPokerHand( cards, out score );
	}

	public Player FindWinner( out HandRank rank )
	{
		var rankedPlayers = Players.Select( x => new { player = x, rank = RankPlayerHand( x, out var score ), score } );
		var groupedPlayers = rankedPlayers.GroupBy( x => x.rank );
		var orderedGroupedPlayers = groupedPlayers.OrderBy( x => x.Key );

		var bestPlayers = orderedGroupedPlayers.First();
		Log.Trace( "Best players: " + string.Join( ", ", bestPlayers.Select( x => x.player + " - " + x.rank + " - " + x.score ) ) );

		rank = HandRank.None;
		Player winner;

		if ( bestPlayers.Count() > 1 )
		{
			// Find highest score
			var orderedBestPlayers = bestPlayers.OrderBy( x => x.score );
			winner = orderedBestPlayers.First().player;
			rank = bestPlayers.First().rank;
		}
		else
		{
			// One winner
			winner = bestPlayers.First().player;
			rank = bestPlayers.First().rank;
		}

		return winner;
	}
}
