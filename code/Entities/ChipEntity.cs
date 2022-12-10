namespace Poker;

[Title( "Chip" ), Category( "Poker" )]
public partial class ChipEntity : ModelEntity
{
	[Net] public float Value { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/chip/chip.vmdl" );
		Transmit = TransmitType.Always;
	}

	public void SetValue( float value )
	{
		Value = value;
	}

	[Event.Client.Frame]
	public void OnFrame()
	{
		SceneObject.ColorTint = Value switch
		{
			500 => Color.Parse( "#991e38" ) ?? Color.Black,
			250 => Color.Parse( "#1f4e99" ) ?? Color.Blue,
			100 => Color.Parse( "#e31a3b" ) ?? Color.Red,
			50 => Color.Parse( "#eb8344" ) ?? Color.Orange,
			_ => Color.Red
		};
	}
}
