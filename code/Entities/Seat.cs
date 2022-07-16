using Sandbox;
using SandboxEditor;

namespace Poker;

[EditorModel( "models/editor/playerstart.vmdl" )]
[Library( "info_seat" )]
[Title( "Seat" ), Category( "Poker" )]
[HammerEntity]
public class Seat : Entity
{
	[Property]
	public int SeatNumber { get; set; }

	[Property]
	public string SeatName { get; set; }
}
