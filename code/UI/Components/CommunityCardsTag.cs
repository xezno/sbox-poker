using Sandbox.UI;
using System;

namespace Poker.UI;

public class CommunityCardsTag : WorldPanel
{
	private CommunityCards Panel { get; set; }
	public Entity Target
	{
		get => Panel.Target;
		set => Panel.Target = value;
	}

	public CommunityCardsTag( Entity target )
	{
		// This is a world panel, and therefore does not inherit the loaded
		// Hud.html stylesheet - so we load one here.
		StyleSheet.Load( "/Code/UI/Styles/CommunityCards.scss" );

		AddClass( "name-tag" );

		Panel = AddChild<CommunityCards>();
		Target = target;
	}

	public override void Tick()
	{
		var size = new Vector2( 1024f, 332f );
		PanelBounds = new Rect( 0, size );

		float renderScale = 2.0f;
		float scale = 1.5f;

		Scale = renderScale * scale;
		WorldScale = 1f / renderScale;
	}
}

internal class CommunityCardsTagComponent : EntityComponent<CommunityCardSpawn>
{
	CommunityCardsTag CommunityCardsTag;

	protected override void OnActivate()
	{
		CommunityCardsTag = new CommunityCardsTag( Entity );
	}

	protected override void OnDeactivate()
	{
		CommunityCardsTag?.Delete();
		CommunityCardsTag = null;
	}

	/// <summary>
	/// Called for every tag, while it's active
	/// </summary>
	[GameEvent.Client.Frame]
	public void FrameUpdate()
	{
		var tx = Entity.Transform;
		var dir = -Camera.Rotation.Forward;
		var rot = Rotation.LookAt( dir );

		var screenPos = (Vector2)tx.Position.ToScreen();
		var center = screenPos - new Vector2( 0.5f, 0.5f );

		// Move to center
		var offset = rot.Right * 13f;
		tx.Position += offset;

		tx.Rotation = Rotation.LookAt( dir );
		tx.Position += rot.Up * 12f;

		var opacity = MathF.Abs( center.x ).LerpInverse( 0.05f, 0f );
		var isInPlay = (PokerGame.Instance is PokerGame { CommunityCards: { Count: > 0 } });

		CommunityCardsTag.SetClass( "visible", opacity > 0f && isInPlay );
		CommunityCardsTag.Transform = tx;
	}

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[GameEvent.Client.Frame]
	public static void SystemUpdate()
	{
		var target = Sandbox.Entity.All.OfType<CommunityCardSpawn>().First();

		var shouldRemove = target.Position.Distance( Camera.Position ) > 500;
		shouldRemove = shouldRemove || target.LifeState != LifeState.Alive;
		shouldRemove = shouldRemove || target.IsDormant;

		if ( shouldRemove )
		{
			var c = target.Components.Get<CommunityCardsTagComponent>();
			c?.Remove();
			return;
		}

		// Add a component if it doesn't have one
		target.Components.GetOrCreate<CommunityCardsTagComponent>();
	}
}
