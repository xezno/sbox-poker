﻿using System;
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

	public static PlayerList Instance { get; private set; }

	public PlayerList()
	{
		Instance = this;
	}

	public void OnVoicePlayed( long steamId, float level )
	{
		var entry = PlayerEntries.FirstOrDefault( x => x.Client.PlayerId == steamId );
		entry?.UpdateVoiceLevel( level );
	}
	
	public override void Tick()
	{
		base.Tick();

		foreach ( var client in Client.All )
		{
			if ( !PlayerEntries.Any( x => x.Client == client ) && client.Pawn is Player )
			{
				var playerEntry = new PlayerEntry( client )
				{
					Parent = WrapperPanel
				};

				PlayerEntries.Add( playerEntry );
			}
		}

		foreach ( var playerEntry in PlayerEntries.ToArray() )
		{
			if ( playerEntry.Client.IsDormant || playerEntry == null )
			{
				playerEntry.Delete();
				PlayerEntries.Remove( playerEntry );
			}
		}
	}

	class PlayerEntry : Panel
	{
		private float VoiceLevel = 0.5f;
		private float TargetVoiceLevel = 0.0f;

		public Client Client
		{
			get => Avatar.Client;
			set => Avatar.Client = value;
		}

		private Player Player => Client.Pawn as Player;

		private Avatar Avatar { get; set; }

		private PokerLabel MoneyLabel { get; set; }
		private PokerLabel StatusLabel { get; set; }

		public PlayerEntry( Client client )
		{
			Avatar = AddChild<Avatar>();
			Avatar.Client = client;

			var right = Add.Panel( "right" );
			MoneyLabel = right.Add.PokerLabel( "MONEY", "money" );
			StatusLabel = right.Add.PokerLabel( "STATUS", "status" );
			StatusLabel.BindClass( "my-turn", () => Player.IsMyTurn );

			Add.Label( client.Name, "player-name" );

			SetClass( "player", true );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !Player.IsValid() )
				Delete();

			if ( IsDeleting )
				return;

			TickInfo();
			TickVoice();
		}
		
		public void UpdateVoiceLevel( float level )
		{
			TargetVoiceLevel = level;
		}

		private void TickInfo()
		{
			MoneyLabel.Text = $"${Player?.Money.CeilToInt() ?? 0}";
			StatusLabel.Text = $"{Player?.StatusText ?? ""}";

			SetClass( "expand", InputLayer.Evaluate( "list_players" ) );
		}

		private void TickVoice()
		{
			base.Tick();

			VoiceLevel = VoiceLevel.LerpTo( TargetVoiceLevel, 50f * Time.Delta );
			Style.Left = VoiceLevel * 64f;
		}
	}
}
