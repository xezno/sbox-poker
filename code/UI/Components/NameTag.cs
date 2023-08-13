using Sandbox.UI;
using System;

namespace Poker.UI;

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
		// This is a world panel, and therefore does not inherit the loaded
		// Hud.html stylesheet - so we load one here.
		StyleSheet.Load( "/Code/UI/Styles/NameTag.scss" );

		AddClass( "name-tag" );

		NameTagPanel = AddChild<NameTagPanel>();
		Player = player;
	}

	public override void Tick()
	{
		var size = new Vector2( 2048, 256 );
		PanelBounds = new Rect( 0, size );

		float renderScale = 2.0f;
		float scale = 1.5f;

		Scale = renderScale * scale;
		WorldScale = 1f / renderScale;
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
	[GameEvent.Client.Frame]
	public void FrameUpdate()
	{
		var tx = Entity.GetAttachment( "hat" ) ?? Entity.Transform;
		var dir = (tx.Position - Camera.Position).Normal;
		var rot = Rotation.LookAt( dir );

		var screenPos = (Vector2)tx.Position.ToScreen();
		var center = screenPos - new Vector2( 0.5f, 0.5f );

		tx.Rotation = Rotation.LookAt( -Camera.Rotation.Forward );
		tx.Position += rot.Up * 10;

		var opacity = MathF.Abs( center.x ).LerpInverse( 0.05f, 0f );

		NameTag.SetClass( "visible", opacity > 0f || NameTag.Player.IsMyTurn );
		NameTag.Transform = tx;
	}

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[GameEvent.Client.Frame]
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

			var shouldRemove = player.Position.Distance( Camera.Position ) > 500;
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
