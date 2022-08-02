using Sandbox;

namespace Poker;

[Title( "Chip" ), Category( "Poker" )]
public partial class ChipEntity : ModelEntity
{
	private Texture texture;
	private Material material;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/chip/chip.vmdl" );
		Transmit = TransmitType.Always;
	}

	public void SetValue( float value )
	{
		RpcSetValue( To.Everyone, value );
	}

	[ClientRpc]
	private void RpcSetValue( float value )
	{
		Host.AssertClient();

	}

	[Event.Frame]
	public void OnFrame()
	{

	}

	public static void CreateStack( int count, Vector3 position )
	{
		for ( int i = 0; i < count; ++i )
		{
			var chipEntity = new ChipEntity();
			chipEntity.Position = position + Vector3.Up * i * 0.25f;
			chipEntity.Rotation = Rotation.FromYaw( Rand.Float( 0, 360f ) );
		}
	}
}
