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
				text = value;
				Update();
			}
		}

		public PokerLabel()
		{
			AddClass( "poker-label" );
			StyleSheet.Load( "/UI/Components/PokerLabel.scss" );
		}

		private void Update()
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
				var money = new Poker.UI.PokerLabel();
				money.SetContent( text );
				money.Parent = self.panel;

				if ( classname != null )
					money.AddClass( classname );

				return money;
			}
		}
	}
}

