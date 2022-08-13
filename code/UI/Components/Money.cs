using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker.UI
{
	public class Money : Panel
	{
		public IconPanel Icon { get; set; }
		public Label Label { get; set; }

		public string Text
		{
			get => Label.Text;
			set => Label.Text = value;
		}

		public Money()
		{
			StyleSheet.Load( "/UI/Components/Money.scss" );

			AddClass( "money" );

			Icon = Add.Icon( "database" );
			Label = Add.Label( "", "money" );
		}

		public override void SetContent( string value )
		{
			base.SetContent( value );

			Label.SetText( value );
		}

		public override void Tick()
		{
			base.Tick();

			Icon.Text = "coins";
		}
	}
}

namespace Sandbox.UI
{
	namespace Construct
	{
		public static class MoneyConstructor
		{
			public static Poker.UI.Money Money( this PanelCreator self, string text, string classname = null )
			{
				var money = new Poker.UI.Money();
				money.SetContent( text );
				money.Parent = self.panel;

				if ( classname != null )
					money.AddClass( classname );

				return money;
			}
		}
	}
}
