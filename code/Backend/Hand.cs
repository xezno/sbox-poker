using Sandbox;
using System.Collections.Generic;

namespace Poker.Backend;

public partial class Hand : BaseNetworkable
{
	[Net] public IList<Card> Cards { get; set; }

	public Hand()
	{

	}
}
