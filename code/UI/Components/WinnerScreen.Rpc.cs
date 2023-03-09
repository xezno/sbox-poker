namespace Poker.UI;

public partial class WinnerScreen
{
	[ConCmd.Client( "poker_winner_screen", CanBeCalledFromServer = true )]
	public static void OnWin( string winnerName, long winnerId, int winAmount, HandRank winRank )
	{
		Log.Trace( "OnWin" );
		_ = WinnerScreen.Instance.ShowWinnerScreen( winnerName, winnerId, winAmount, winRank );
	}
}
