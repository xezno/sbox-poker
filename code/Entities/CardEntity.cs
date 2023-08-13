namespace Poker;

[Prefab, Title( "Playing Card" ), Category( "Poker" )]
public partial class CardEntity : ModelEntity
{
	private Texture texture;
	private Material material;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/card/card.vmdl" );
		Transmit = TransmitType.Always;
	}

	[ClientRpc]
	public void RpcSetCard( Card card )
	{
		Game.AssertClient();

		texture = Texture.Load( FileSystem.Mounted, card.GetFilename() );
		material = Material.Load( "materials/card/card.vmat" ).CreateCopy();
		SetMaterialOverride( material, "isCardTarget" );
	}

	[ClientRpc]
	public void RpcClearCard()
	{
		Game.AssertClient();

		ClearMaterialOverride();
	}

	[GameEvent.Client.Frame]
	public void OnFrame()
	{
		if ( texture?.IsLoaded ?? false )
			material.Set( "Color", texture );
	}
}
