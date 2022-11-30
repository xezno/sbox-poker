using Sandbox.UI;
using System;

namespace Poker.UI;

public class RadialMenu : Panel
{
	private Panel Hovered { get; set; }

	public string HoverText { get; set; } = "";
	public bool IsActive { get; set; }

	[Alias( "radialmenu" )]
	public override void Tick()
	{
		base.Tick();

		var children = Children.ToList();

		float angleIncrement = 360f / children.Count;
		var distance = 48f * ScaleToScreen;
		var angleOffset = (children.Count % 2 == 0) ? 0f : 30f;

		for ( int i = 0; i < children.Count; i++ )
		{
			var child = children[i];

			var angle = i * angleIncrement;
			angle -= angleOffset;

			var position = new Vector2(
				MathF.Cos( angle.DegreeToRadian() ) * distance,
				MathF.Sin( angle.DegreeToRadian() ) * distance
			);

			position += Box.Rect.Width / 2f;

			child.Style.Left = position.x * ScaleFromScreen;
			child.Style.Top = position.y * ScaleFromScreen;
		}

		Hovered = children.FirstOrDefault( x => x.HasHovered );

		if ( HasHovered && Hovered != null && Hovered is RadialMenuItem item )
		{
			HoverText = item.Name;
			IsActive = true;
		}
		else
		{
			IsActive = false;
		}
	}
}
