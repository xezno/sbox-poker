namespace Poker;

public static class OverlayUtils
{
	public static void BoxWithText( Rect rect, string title, string text )
	{
		var margin = new Sandbox.UI.Margin( 16, 24, 16, 16 );

		//
		// Draw bg box
		//
		Graphics.DrawRoundedRectangle(
			rect,
			new Color( 0.1f, 0.1f, 0.1f, 0.8f ),
			new( 4 )
		);

		//
		// Draw title
		//
		Graphics.DrawText(
			rect.Shrink( 8 ),
			title,
			Color.White.WithAlpha( 0.6f ),
			"Cascadia Code",
			9,
			450,
			TextFlag.LeftTop
		);

		//
		// Draw main body
		//
		Graphics.DrawText(
			rect.Shrink( margin ),
			text,
			Color.White.WithAlpha( 0.9f ),
			"Cascadia Code",
			12,
			450,
			TextFlag.LeftCenter
		);
	}

	public static void BoxWithText( Vector2 position, string title, string text )
	{
		//
		// Measure text so that we can auto scale a box
		//
		var textSize = Graphics.MeasureText( new Rect( position, 4096 ), text, "Cascadia Code", 12 );
		var rect = textSize.Grow( 20 ) + new Vector2( 20 ); // Expand will offset position, so offset it back again

		BoxWithText( rect, title, text );
	}
}
