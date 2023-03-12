using System;

namespace Poker;

public partial class ChipGroupEntity : Entity
{
	[Category( "Poker" ), Net] private float NetMoney { get; set; }
	[Category( "Poker" )] private List<Entity> PlayerChips { get; } = new();

	[Category( "Poker" )]
	public float Value
	{
		get => NetMoney; set
		{
			NetMoney = value;
			UpdateChipEntities();
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	private void UpdateChipEntities()
	{
		Game.AssertServer();

		PlayerChips.ForEach( x => x.Delete() );
		PlayerChips.Clear();

		float spacing = 2.5f;

		// Calculate the total value of all chips
		float totalChipValue = 200 + 100 * 5 + 50 * 7 + 10 * 10;

		// Divide the player's money by the total value to determine the factor
		float factor = Value / totalChipValue;

		// Calculate the number of chips of each denomination
		int num500Chips = (int)MathF.Round( 2 * factor );
		int num250Chips = (int)MathF.Round( 5 * factor );
		int num100Chips = (int)MathF.Round( 7 * factor );
		int num50Chips = (int)MathF.Round( 10 * factor );

		// Create a new chip stack for each denomination
		void SpawnChips( int count, int value, Vector3 position )
		{
			var stack = ChipStackEntity.CreateStack( count, value, this, position );
			stack.SetParent( this );
			PlayerChips.Add( stack );
		}

		SpawnChips( num500Chips, 200, Vector3.Zero );
		SpawnChips( num250Chips, 100, new Vector3( spacing, 0 ) );
		SpawnChips( num100Chips, 50, new Vector3( spacing, spacing ) );
		SpawnChips( num50Chips, 10, new Vector3( 0, spacing ) );
	}
}
