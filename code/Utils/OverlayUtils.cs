namespace Poker;

public static class OverlayUtils
{
    public static void BoxWithText( this Sandbox.Render.Render2D draw, Rect rect, string title, string text )
    {
        var margin = new Sandbox.UI.Margin( 16, 24, 16, 16 );

        //
        // Draw bg box
        //
        draw.Color = new Color( 0.1f, 0.1f, 0.1f, 0.8f );
        draw.Box( rect, new( 4 ) );

        //
        // Draw title
        //
        draw.Color = Color.White.WithAlpha( 0.6f );
        draw.SetFont( "Cascadia Code", 9 );
        draw.DrawText(
            rect.Contract( 8 ),
            title,
            TextFlag.LeftTop );

        //
        // Draw main body
        //
        draw.Color = Color.White.WithAlpha( 0.9f );
        draw.SetFont( "Cascadia Code", 12 );
        draw.DrawText(
            rect.Contract( margin ),
            text,
            TextFlag.LeftTop );
    }

    public static void BoxWithText( this Sandbox.Render.Render2D draw, Vector2 position, string title, string text )
    {
        //
        // Measure text so that we can auto scale a box
        //
        draw.SetFont( "Cascadia Code", 12 );
        var textSize = draw.MeasureText( new Rect( position, 4096 ), text );
        var rect = textSize.Expand( 20 ) + new Vector2( 20 ); // Expand will offset position, so offset it back again

        BoxWithText( draw, rect, title, text );
    }
}