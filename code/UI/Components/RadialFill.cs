using Sandbox.UI;

namespace Poker.UI;

/*
 * https://gist.github.com/cr4yz/c711fd84ba51d29359589868d190ac0d
 */
public class RadialFill : Panel
{
	public bool Visible { get; set; } = true;
	public float FillAmount { get; set; } = 0.37f;

	public float BorderWidth => 20;
	public float EdgeGap => 2;
	public int Points => 64;
	public Color TrackColor => Color.Black;
	public Color FillColor => Color.White;

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if ( !Visible )
			return;

		var center = Box.Rect.Center;
		var radius = Box.Rect.width * .5f;
		var draw = Render.Draw2D;

		draw.Color = TrackColor;
		draw.CircleEx( center, radius, radius - BorderWidth, Points );

		draw.Color = FillColor;
		draw.CircleEx( center, radius - EdgeGap, radius - BorderWidth + EdgeGap, Points, 0, FillAmount * 360 );
	}
}
