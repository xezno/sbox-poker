using Sandbox;
using SandboxEditor;

namespace Poker;

[EditorModel( "models/editor/camera.vmdl" )]
[Library( "info_camera_transform" )]
[Title( "Camera Transform" ), Category( "Poker" )]
[HammerEntity]
public partial class CameraTransform : Entity
{
	[Property, Net] public string TransformKey { get; set; }
	[Property, Net, Range( 1f, 179f )] public float FieldOfView { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
