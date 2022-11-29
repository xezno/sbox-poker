using Sandbox.UI;
using System;

namespace Poker.UI;

[StyleSheet( "/UI/Components/NameTag.scss" )]
public class NameTag : WorldPanel
{
	private NameTagPanel NameTagPanel { get; set; }
	public Poker.Player Player
	{
		get => NameTagPanel.Player;
		set => NameTagPanel.Player = value;
	}

	public NameTag( Player player )
	{
		AddClass( "name-tag" );

		NameTagPanel = AddChild<NameTagPanel>();
		Player = player;

		var size = new Vector2( 1024, 1024 );

		PanelBounds = new Rect( size.x * -0.5f, size.y * -0.5f, size.x, size.y );

		float resolutionScale = 2.0f;

		Scale = resolutionScale;
		WorldScale = 1f / resolutionScale;
	}
}

internal class NameTagComponent : EntityComponent<Player>
{
	NameTag NameTag;

	protected override void OnActivate()
	{
		NameTag = new NameTag( Entity );
	}

	protected override void OnDeactivate()
	{
		NameTag?.Delete();
		NameTag = null;
	}

	/// <summary>
	/// Called for every tag, while it's active
	/// </summary>
	[Event.Frame]
	public void FrameUpdate()
	{
		var tx = Entity.GetAttachment( "hat" ) ?? Entity.Transform;
		var dir = (tx.Position - CurrentView.Position).Normal;
		var rot = Rotation.LookAt( dir );

		var screenPos = (Vector2)tx.Position.ToScreen();
		var center = screenPos - new Vector2( 0.5f, 0.5f );

		tx.Position += rot.Right * 20.0f;
		tx.Position += rot.Down * 16.0f;
		tx.Rotation = Rotation.LookAt( -CurrentView.Rotation.Forward );

		var opacity = MathF.Abs( center.x ).LerpInverse( 0.05f, 0f );

		NameTag.SetClass( "visible", opacity > 0f );
		NameTag.Transform = tx;
	}

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[Event.Frame]
	public static void SystemUpdate()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<Player>() )
		{
			if ( player.IsLocalPawn && player.IsFirstPersonMode )
			{
				var c = player.Components.Get<NameTagComponent>();
				c?.Remove();
				continue;
			}

			var shouldRemove = player.Position.Distance( CurrentView.Position ) > 500;
			shouldRemove = shouldRemove || player.LifeState != LifeState.Alive;
			shouldRemove = shouldRemove || player.IsDormant;

			if ( shouldRemove )
			{
				var c = player.Components.Get<NameTagComponent>();
				c?.Remove();
				continue;
			}

			// Add a component if it doesn't have one
			player.Components.GetOrCreate<NameTagComponent>();
		}
	}
}
