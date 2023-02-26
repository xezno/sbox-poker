namespace Poker;

partial class Player
{
	static string[] s_audioSamples = new[]
	{
		"ambience.sniff",
		"ambience.throat_clear",
		"ambience.fart"
	};

	public void PlayRandomAudio()
	{
		Game.AssertServer();

		var rand = Game.Random.Next( 0, 1000 );

		if ( rand == 1 )
		{
			var sample = Game.Random.FromArray( s_audioSamples );
			Sound.FromEntity( sample, this );
		}
	}
}
