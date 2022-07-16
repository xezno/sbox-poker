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

	protected bool IsSet = false;

	public InputHint()
	{
		BindClass( "noaction", () => string.IsNullOrEmpty( Text ) );
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		Log.Trace( (name, value) );
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

		// if ( !IsSet )
		{
			InputButton button = InputButton.Slot0;

			var action = InputLayer.GetAction( Name );

			if ( action is Action1D action1D )
			{
				button = action1D.PositiveButton;
			}
			else if ( action is ActionBool actionBool )
			{
				button = actionBool.Button;
			}
			else if ( action is ActionAxis actionAxis )
			{
				button = actionAxis.DisplayButton;
			}

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

			IsSet = true;
		}
	}
}
