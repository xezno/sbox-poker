using Sandbox;
using Sandbox.UI;
using System;

namespace Poker.UI;

[UseTemplate]
public partial class InputHint : Panel
{
	// @ref
	public Image Glyph { get; set; }
	public Image GlyphShadow { get; set; }
	public string Name { get; set; }
	public string Text { get; set; }
	public PokerLabel ActionLabel { get; set; }
	public Panel ProgressIndicatorPanel { get; set; }

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

		ActionLabel.Text = value;
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

		SetClass( "using-controller", Input.UsingController );
		SetClass( "active", MathF.Abs( action.Evaluate() ) > 0.5f );

		if ( action is HoldAction holdAction )
		{
			SetClass( "is-hold", true );
			SetClass( "has-progress", holdAction.Progress > 0.001f );
			SetClass( "active", holdAction.Progress > 0.001f );

			float progress = (holdAction.Progress * 100f).Clamp( 0, 99.999f );

			if ( Input.UsingController )
			{
				ProgressIndicatorPanel.Style.Set( $"background: conic-gradient( white 0%, white {progress}%, transparent {progress}%, transparent 100% )" );
				ProgressIndicatorPanel.Style.Width = Length.Auto;
			}
			else
			{
				ProgressIndicatorPanel.Style.Set( $"background: none" );
				ProgressIndicatorPanel.Style.Width = Length.Percent( progress );
			}
		}

		Texture glyphTexture = Input.GetGlyph( button, InputGlyphSize.Small, GlyphStyle.Knockout.WithNeutralColorABXY() );

		if ( glyphTexture != null )
		{
			SetImageTexture( Glyph, glyphTexture );
			SetImageTexture( GlyphShadow, glyphTexture );
		}
	}

	private void SetImageTexture( Image image, Texture texture )
	{
		if ( image == null )
			return;

		image.Style.BackgroundImage = texture;
		image.Style.Width = texture.Width;
		image.Style.Height = texture.Height;
	}
}
