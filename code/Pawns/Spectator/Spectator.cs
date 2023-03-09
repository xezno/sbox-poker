namespace Poker;

public class Spectator : BasePawn
{
	private SpectatorCamera Camera { get; set; }

	private int RoundsLeft { get; set; } = 5;

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

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Camera.Update();
	}

	[PokerEvent.NewRound]
	public void OnNewRound()
	{
		if ( !Game.IsServer )
			return;

		RoundsLeft--;

		if ( RoundsLeft <= 0 )
		{
			PokerGame.Instance.CreatePlayerFor( Client );
		}
	}
}
