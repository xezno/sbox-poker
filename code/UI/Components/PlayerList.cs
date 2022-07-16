using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Poker.UI;

[UseTemplate]
public class PlayerList : Panel
{
	// @ref
	public Panel WrapperPanel { get; set; }

	private List<PlayerEntry> PlayerEntries { get; set; } = new();

	public PlayerList()
	{
	}

	public override void Tick()
	{
		base.Tick();

		foreach ( var client in Client.All )
		{
			if ( !PlayerEntries.Any( x => x.Client == client ) )
			{
				var playerEntry = new PlayerEntry( client )
				{
					Parent = WrapperPanel
				};

				PlayerEntries.Add( playerEntry );
			}
		}
	}

	class PlayerEntry : Panel
	{
		public Client Client
		{
			get => Avatar.Client;
			set => Avatar.Client = value;
		}

		private Avatar Avatar { get; set; }

		public PlayerEntry( Client client )
		{
			Avatar = AddChild<Avatar>();
			Avatar.Client = client;

			var right = Add.Panel( "right" );
			right.Add.Label( "$500", "money" );
			right.Add.Label( "High Card", "status" );

			SetClass( "player", true );
		}
	}
}
