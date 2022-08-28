namespace Poker;

[Library( "info_community_card_spawn" )]
[EditorModel( "models/card/card.vmdl" )]
[Title( "Community Card Spawn" ), Category( "Poker" )]
[HammerEntity]
public class CommunityCardSpawn : Entity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
