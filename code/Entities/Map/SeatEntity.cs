namespace Poker;

[EditorModel( "models/editor/playerstart.vmdl" )]
[Library( "info_seat" )]
[Title( "Seat" ), Category( "Poker" )]
[HammerEntity]
public partial class SeatEntity : Entity
{
	[Property]
	public int SeatNumber { get; set; }

	[Property]
	public string SeatName { get; set; }

	[Net] public Player Player { get; set; }

	public bool IsOccupied => Player.IsValid();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Never;
	}

	public void SetOccupiedBy( Player player )
	{
		Host.AssertServer();

		player.Transform = this.Transform;
		this.Player = player;
	}
}
