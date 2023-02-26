namespace Poker;

partial class Player
{
	static string[] s_audioSamples = new[]
	{
		"ambience.sniff",
		"ambience.throat_clear",
		"ambience.fart"
	};

	private TimeSince _timeSinceLastAudio = 0;

	public void PlayRandomAudio()
	{
		Game.AssertServer();

		if ( _timeSinceLastAudio < 10 )
			return;

		if ( Client.Voice.CurrentLevel > 0.1f )
			return;

		var rand = Game.Random.Next( 0, 10000 );

		if ( rand == 500 )
		{
			var sample = Game.Random.FromArray( s_audioSamples );
			Sound.FromEntity( sample, this );
			_timeSinceLastAudio = 0;
		}
	}
}
