using System;
using System.Text.Json.Serialization;

namespace Poker;

[Obsolete]
[GameResource( "Poker Game Settings", "poker", "Game settings for Poker" )]
public class GameSettings : GameResource
{
	[JsonIgnore]
	private static GameSettings instance;

	private static void LoadSettings()
	{
		instance = ResourceLibrary.Get<GameSettings>( "data/settings.poker" );
	}

	[JsonIgnore]
	public static GameSettings Instance
	{
		get
		{
			if ( instance == null )
				LoadSettings();

			return instance;
		}
	}

	[Category( "Hands" )]
	public Vector3 LeftHandPosition { get; set; }
	[Category( "Hands" )]
	public Rotation LeftHandRotation { get; set; }
	[Category( "Hands" )]
	public Vector3 RightHandPosition { get; set; }
	[Category( "Hands" )]
	public Rotation RightHandRotation { get; set; }

	[Category( "Cards" )]
	public Vector3 BaseHoldPosition { get; set; }
	[Category( "Cards" )]
	public Vector3 LeftCardOffset { get; set; }
	[Category( "Cards" )]
	public Rotation LeftCardRotation { get; set; }
	[Category( "Cards" )]
	public Vector3 RightCardOffset { get; set; }
	[Category( "Cards" )]
	public Rotation RightCardRotation { get; set; }

	[Category( "Player Sitting" )]
	public float SitHeight { get; set; }
	[Category( "Player Sitting" )]
	public float SitPose { get; set; }
}
