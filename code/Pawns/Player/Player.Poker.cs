using System;
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
			UpdateChipEntities();
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

	public async Task SetAction( Actions newAction )
	{
		Game.AssertServer();

		CurrentAction = newAction;
		await Task.Delay( 700 ); // ms

		// Reset action
		CurrentAction = Actions.None;
	}

	[Category( "Poker" )] private List<Entity> PlayerChips { get; } = new();

	private void UpdateChipEntities()
	{
		// TODO: Shouldn't need to delete and create entities, should just update them
		Game.AssertServer();

		PlayerChips.ForEach( x => x.Delete() );
		PlayerChips.Clear();

		// Spawn chips
		var seat = Seat;
		if ( seat == null )
			return;

		var chipSpawn = Entity.All.OfType<PlayerChipSpawn>().First( x => x.SeatNumber == seat.SeatNumber );
		float spacing = 2.5f;

		// Calculate the total value of all chips
		float totalChipValue = 500 + 250 * 5 + 100 * 7 + 50 * 10;

		// Divide the player's money by the total value to determine the factor
		float factor = Money / totalChipValue;

		// Calculate the number of chips of each denomination
		int num500Chips = (int)MathF.Round( 2 * factor );
		int num250Chips = (int)MathF.Round( 5 * factor );
		int num100Chips = (int)MathF.Round( 7 * factor );
		int num50Chips = (int)MathF.Round( 10 * factor );

		// Create a new chip stack for each denomination
		void SpawnChips( PlayerChipSpawn chipSpawn, int count, int value, Vector3 position )
		{
			PlayerChips.Add( ChipStackEntity.CreateStack( count, value, position ) );
		}

		SpawnChips( chipSpawn, num500Chips, 500, chipSpawn.Position );
		SpawnChips( chipSpawn, num250Chips, 250, chipSpawn.Position + new Vector3( 0, spacing ) * Rotation );
		SpawnChips( chipSpawn, num100Chips, 100, chipSpawn.Position + new Vector3( spacing, spacing ) * Rotation );
		SpawnChips( chipSpawn, num50Chips, 50, chipSpawn.Position + new Vector3( 0, spacing * 2 ) * Rotation );
	}
}
