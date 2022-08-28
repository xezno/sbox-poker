using Poker.UI;

namespace Poker;
partial class Game
{
	public HandRank RankPlayerHand( Player player, out int score )
	{
		// TODO
		// For each 5 card combination in (communitycards + playerHand) calculate the rank

		var playerHand = player.Hand;
		var cards = playerHand.ToArray().Concat( CommunityCards ).ToArray();

		return PokerUtils.RankPokerHand( cards, out score );
	}

	public Player FindWinner()
	{
		var rankedPlayers = Players.Select( x => new { player = x, rank = RankPlayerHand( x, out var score ), score } );
		var groupedPlayers = rankedPlayers.GroupBy( x => x.rank );
		var orderedGroupedPlayers = groupedPlayers.OrderBy( x => x.Key );

		var bestPlayers = orderedGroupedPlayers.First();
		Log.Trace( "Best players: " + string.Join( ", ", bestPlayers.Select( x => x.player + " - " + x.rank + " - " + x.score ) ) );

		Player winner;

		if ( bestPlayers.Count() > 1 )
		{
			// Find highest score
			var orderedBestPlayers = bestPlayers.OrderBy( x => x.score );
			winner = orderedBestPlayers.First().player;
		}
		else
		{
			// One winner
			winner = bestPlayers.First().player;
		}

		EventFeed.AddEvent( To.Everyone, $"{winner.Client.Name} wins" );
		Log.Trace( $"{winner.Client.Name} wins" );

		return winner;
	}
}
