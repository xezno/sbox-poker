namespace Poker;

[Library( "info_community_card_spawn" )]
[EditorModel( "models/card/card.vmdl" )]
[Title( "Community Card Spawn" ), Category( "Poker" )]
[HammerEntity]
public class CommunityCardSpawn : Entity
{
	private List<CardEntity> Cards { get; set; } = new();
	private ChipGroupEntity Chips { get; set; }

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

		Chips = new();
		Chips.SetParent( this );
		Chips.LocalPosition = Vector3.Forward * -8f + Vector3.Down * 2f;
	}

	[PokerEvent.GameStart]
	public void OnGameStart()
	{
		Cards.ForEach( x =>
		{
			x.RpcClearCard();
			x.EnableDrawing = false;
		} );
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

	[PokerEvent.NewRound]
	public void OnNewRound()
	{
		if ( PokerGame.Instance.IsValid() && Chips.IsValid() )
			Chips.Value = PokerGame.Instance.Pot;
	}
}
