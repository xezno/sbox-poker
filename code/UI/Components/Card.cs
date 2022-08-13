using System;
using Sandbox.UI;
using Poker.Backend;
using Sandbox;

namespace Poker.UI;
public class Card : Panel
{
	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "type" ) // Value of Suit
		{
			var split = value.Split( " " );

			if ( split.Length < 2 )
				return;

			var cardValueStr = split[0];
			var cardSuitStr = split[2];

			if ( !Enum.TryParse<Suit>( cardSuitStr, true, out var cardSuit ) )
				return;

			if ( !Enum.TryParse<Value>( cardValueStr, true, out var cardValue ) )
				return;

			SetCard( cardSuit, cardValue );
		}
	}

	public Card()
	{
		StyleSheet.Load( "/UI/Components/Card.scss" );
		SetClass( "card", true );
	}

	public void Reset()
	{
		Style.BackgroundImage = null;
	}

	public void SetCard( Suit suit, Value value )
	{
		var card = new Backend.Card( suit, value );
		Style.BackgroundImage = Texture.Load( FileSystem.Mounted, card.GetFilename() );
	}
}
