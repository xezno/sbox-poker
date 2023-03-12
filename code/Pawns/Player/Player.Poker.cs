using System.Threading.Tasks;

namespace Poker;
partial class Player
{
	[Category( "Poker" ), Net] private float NetMoney { get; set; }

	[Category( "Poker" )]
	public float Money
	{
		get => NetMoney; set
		{
			NetMoney = value;

			if ( Chips.IsValid() )
				Chips.Value = value;
		}
	}

	[Category( "Poker" ), Net] public float LastBet { get; set; }
	[Category( "Poker" ), Net] public Move LastMove { get; set; }
	[Category( "Poker" ), Net] public bool IsMyTurn { get; set; }
	[Category( "Poker" ), Net] public bool HasFolded { get; set; }

	[Category( "Poker" ), Net] public CardEntity LeftCard { get; set; }
	[Category( "Poker" ), Net] public CardEntity RightCard { get; set; }

	[Category( "Poker" )] public List<Card> Hand { get; set; }
	[Category( "Poker" )] public SeatEntity Seat => Entity.All.OfType<SeatEntity>().FirstOrDefault( x => x.Player == this );
	[Category( "Poker" )] public ChipGroupEntity Chips { get; set; }

	[ClientRpc]
	public void RpcSetHand( byte[] cardData )
	{
		Hand = RpcUtils.Decompress<List<Card>>( cardData );
	}

	public void Reset()
	{
		HasFolded = false;
		LastBet = 0;
		LastMove = Move.Bet;
		Hand = null;
	}

	private bool CanSubmitMove()
	{
		Game.AssertClient();

		if ( !IsMyTurn )
			return false;

		return true;
	}

	public void Raise( float amount )
	{
		if ( !CanSubmitMove() )
			return;

		PokerGame.CmdSubmitMove( Move.Bet, amount );
	}

	public void Check()
	{
		if ( !CanSubmitMove() )
			return;

		PokerGame.CmdSubmitMove( Move.Bet, 0f );
	}

	public void Fold()
	{
		if ( !CanSubmitMove() )
			return;

		PokerGame.CmdSubmitMove( Move.Fold, 0f );
	}

	public void Emote( Emote emote )
	{
		PokerGame.CmdEmote( emote );
	}

	public async Task SetAction( Actions newAction, int delay = 500 )
	{
		Game.AssertServer();

		CurrentAction = newAction;
		await Task.Delay( delay ); // ms

		// Reset action
		CurrentAction = Actions.None;
	}
}
