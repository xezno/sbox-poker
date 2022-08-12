using System.Collections.Generic;

namespace Poker.Backend;

public class Message
{
	public string Command { get; set; }
	public Dictionary<string, object> Data { get; set; }
}

public class InfoResponseData
{
	public int Balance { get; set; }
}

public class GenericResponseData
{
	public bool Success { get; set; }
}

public partial class Player
{
	public class LoginData
	{
		public string? PlayerName { get; set; }
		public string? PlayerId { get; set; }
	}

	public class PublicData
	{
		public int Money { get; set; }
		public string Status { get; set; }
	}
}

public class MoveData
{
	public Move Type { get; set; }
	public int Value { get; set; }
}

public class MoveResponseData
{
	public bool Success { get; set; }
	public int NewBalance { get; set; }
}

public partial class Table
{
	public class CreateData
	{
	}

	public class CreateResponseData
	{
		public bool Success { get; set; }
	}

	public class InfoUpdateData
	{
		public int Pot { get; set; }
	}
}
