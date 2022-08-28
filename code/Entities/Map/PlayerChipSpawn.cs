namespace Poker;

[EditorModel( "models/chip/chip.vmdl" )]
[Library( "info_player_chip_spawn" )]
[Title( "Player Chip Spawn" ), Category( "Poker" )]
[HammerEntity]
public partial class PlayerChipSpawn : Entity
{
	[Property]
	public int SeatNumber { get; set; }

	[Net] public Player Player { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Never;
	}
}
