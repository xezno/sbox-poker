using Sandbox;
using Sandbox.UI;

namespace Poker.UI;

[UseTemplate]
public partial class InputHint : Panel
{
	// @ref
	public Image Glyph { get; set; }
	public string Name { get; set; }
	public string Text { get; set; }
	public Label ActionLabel { get; set; }

	public InputHint()
	{
		BindClass( "noaction", () => string.IsNullOrEmpty( Text ) );
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "btn" )
		{
			SetButton( value );
		}
	}

	public void SetButton( string name )
	{
		Name = name;
	}

	public override void SetContent( string value )
	{
		base.SetContent( value );

		ActionLabel.SetText( value );
		Text = value;
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
	}

	public override void Tick()
	{
		base.Tick();

		var action = InputLayer.GetAction( Name );
		var button = action.GetDisplayButton();

		SetClass( "active", !action.Evaluate().AlmostEqual( 0.0f, 0.01f ) );

		Texture glyphTexture = Input.GetGlyph( button, InputGlyphSize.Small, GlyphStyle.Knockout.WithSolidABXY().WithNeutralColorABXY() );

		if ( glyphTexture != null )
		{
			Glyph.Style.BackgroundImage = glyphTexture;
			Glyph.Style.Width = glyphTexture.Width;
			Glyph.Style.Height = glyphTexture.Height;
		}
		else
		{
			Glyph.Style.BackgroundImage = Texture.Load( FileSystem.Mounted, "/ui/Input/invalid_glyph.png" );
		}
	}
}
