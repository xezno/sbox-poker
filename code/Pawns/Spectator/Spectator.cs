namespace Poker;

public class Spectator : BasePawn
{
	private SpectatorCamera Camera { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Camera = new();
	}

	public override void BuildInput()
	{
		base.BuildInput();

		Camera.BuildInput();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Camera.Update();
	}
}
