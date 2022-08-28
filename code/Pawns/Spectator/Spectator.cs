namespace Poker;

public class Spectator : Entity
{
	public override void Spawn()
	{
		base.Spawn();

		var camera = Components.Create<SpectatorCamera>();
	}
}
