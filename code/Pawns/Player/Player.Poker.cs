namespace Poker;
partial class Player
{
	[Net] public float Money { get; set; }
	[Net] public float LastBet { get; set; }
	[Net] public Move LastMove { get; set; }
	[Net] public bool IsMyTurn { get; set; }
	[Net] public bool HasFolded { get; set; }

	[Net] public CardEntity LeftCard { get; set; }
	[Net] public CardEntity RightCard { get; set; }

	public List<Card> Hand { get; set; }
	public string StatusText { get; set; } = "TODO.. this needs filling";
	public SeatEntity Seat => Entity.All.OfType<SeatEntity>().First( x => x.Player == this );

	[ClientRpc]
	public void RpcSetHand( byte[] cardData )
	{
		var cards = RpcUtils.Decompress<Card[]>( cardData );
		Hand = cards.ToList();
	}

	[ClientRpc]
	public void RpcSetStatus( string status )
	{
		StatusText = status;
	}

	public void Reset()
	{
		HasFolded = false;
		LastBet = 0;
		LastMove = Move.Bet;
		Hand = null;
	}
}
