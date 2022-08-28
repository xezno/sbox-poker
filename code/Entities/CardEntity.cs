namespace Poker;

[Title( "Playing Card" ), Category( "Poker" )]
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
	private void RpcSetCard( Backend.Card card )
	{
		Host.AssertClient();

		texture = Texture.Load( FileSystem.Mounted, card.GetFilename() );
		Log.Trace( $"Set card texture to {texture.ResourcePath}" );

		material = Material.Load( "materials/card/card.vmat" ).CreateCopy();
		SetMaterialOverride( material, "isCardTarget" );
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( texture?.IsLoaded ?? false )
			material.OverrideTexture( "Color", texture );
	}
}
