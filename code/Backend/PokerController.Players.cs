﻿using System.Collections.Generic;
using System.Linq;

namespace Poker.Backend;

partial class PokerController
{
	class PlayerQueue
	{
		private Queue<Player> InternalQueue { get; set; }

		public void CreateQueue( List<Player> players )
		{
			InternalQueue = new( 
				players.Where( x =>
				{
					return !x.HasFolded;
				} )
			);
		}

		public Player Pop()
		{
			return InternalQueue.Dequeue();
		}

		public Player Peek()
		{
			return InternalQueue.Peek();
		}

		public int Count => InternalQueue.Count;
	}

	private PlayerQueue PlayerTurnQueue { get; set; }

}