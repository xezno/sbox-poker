using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker.UI
{

	[Alias( "p", "ptext", "plabel" )]
	public class PokerLabel : Panel
	{
		private string text;
		public string Text
		{
			get => text;
			set
			{
				if ( text != value )
				{
					text = value;

					Update();
				}
			}
		}

		public PokerLabel()
		{
			AddClass( "poker-label" );
			StyleSheet.Load( "/UI/Components/PokerLabel.scss" );
		}

		public void Update()
		{
			DeleteChildren( true );
			string curr = "";

			for ( int i = 0; i < Text.Length; i++ )
			{
				char c = Text[i];
				if ( c == '$' )
				{
					Add.Label( curr );
					Add.Icon( "coins", "solid" );
					curr = "";
				}
				else
				{
					curr += c;
				}

				if ( i == Text.Length - 1 )
				{
					Add.Label( curr );
				}
			}
		}
	}
}

namespace Sandbox.UI
{
	namespace Construct
	{
		public static class PokerLabelConstructor
		{
			public static Poker.UI.PokerLabel PokerLabel( this PanelCreator self, string text, string classname = null )
			{
				var element = new Poker.UI.PokerLabel();
				element.Text = text;
				element.Parent = self.panel;

				if ( classname != null )
					element.AddClass( classname );

				return element;
			}
		}
	}
}

