namespace Poker;

[Library( "info_community_card_spawn" )]
[EditorModel( "models/card/card.vmdl" )]
[Title( "Community Card Spawn" ), Category( "Poker" )]
[HammerEntity]
public class CommunityCardSpawn : Entity
{
	private List<CardEntity> Cards { get; set; } = new();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		for ( int i = 0; i < 5; ++i )
		{
			var card = new CardEntity();

			float distance = 4f;
			var origin = Rotation.Right * -(distance * 2f);
			origin += Vector3.Down * 0.475f;

			card.SetParent( this );
			card.LocalPosition = origin + (Rotation.Right * (i * distance));

			Cards.Add( card );
		}
	}

	[PokerEvent.CommunityCardsUpdated]
	public void OnCommunityCardsUpdated()
	{
		for ( int i = 0; i < 5; ++i )
		{
			var card = Cards[i];

			if ( i < PokerGame.Instance.CommunityCards.Count )
			{
				card.RpcSetCard( PokerGame.Instance.CommunityCards[i] );
				card.EnableDrawing = true;
			}
			else
			{
				card.RpcClearCard();
				card.EnableDrawing = false;
			}
		}
	}
}
