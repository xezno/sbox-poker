using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Poker.UI;

public class Avatar : Panel
{
	public Client Client
	{
		get => Scene.Client;
		set => Scene.Client = value;
	}

	private AvatarScene Scene { get; set; }

	public Avatar()
	{
		StyleSheet.Load( "/UI/Components/Avatar.scss" );
		SetClass( "avatar", true );

		Scene = AddChild<AvatarScene>();
	}

	class AvatarScene : ScenePanel
	{
		private List<SceneModel> Models { get; set; }
		private SceneModel Citizen { get; set; }
		private ClothingContainer ClothingContainer { get; set; }

		public Client Client { get; set; }

		private bool IsDirty { get; set; } = true;

		public AvatarScene()
		{
			SetClass( "avatar-scene", true );
		}

		[Event.Hotload]
		private void SetupWorld()
		{
			World?.Delete();
			World = new SceneWorld();
			AmbientColor = Color.White * 0.5f;

			_ = new SceneLight( World, Vector3.Up * 128 + Vector3.Forward * 64f, 512f, Color.White * 50f );
			Citizen = new SceneModel( World, "models/citizen/citizen.vmdl", Transform.Zero );
			Citizen.SetAnimGraph( "animgraphs/citizen_portrait.vanmgrph" );

			Models = new() { Citizen };

			Dress();

			DisablePostProcessing = true;
			IsDirty = false;
			RenderOnce = true;
		}

		private void Dress()
		{
			var player = Client.Pawn as Poker.Player;
			var avatarData = player.AvatarData;

			ClothingContainer = new ClothingContainer();
			ClothingContainer.Deserialize( avatarData );
			Models.AddRange( ClothingContainer.DressSceneObject( Citizen ) );
		}

		public override void Tick()
		{
			base.Tick();

			if ( IsDirty )
			{
				SetupWorld();
			}

			foreach ( var model in Models )
			{
				model.Update( RealTime.Delta );
			}

			BuildCamera();
		}

		private void BuildCamera()
		{
			var eyePosition = new Vector3( 0, 0, 64 );

			CameraPosition = eyePosition + new Vector3( 64, 0, 0 );
			CameraRotation = Rotation.LookAt( (eyePosition - CameraPosition).Normal, Vector3.Up );
			FieldOfView = 20f;
			ZNear = 0.1f;
			ZFar = 512;
		}
	}
}
