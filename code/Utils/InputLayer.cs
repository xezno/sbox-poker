using System;

namespace Poker;

public class BaseInputAction
{
	public string Name { get; set; }

	public BaseInputAction( string name )
	{
		Name = name;
	}

	public virtual float Evaluate()
	{
		return 0.0f;
	}

	public virtual string GetDisplayButton()
	{
		return "Slot0";
	}
}

public class HoldAction : BoolAction
{
	private TimeSince timeSinceHeld;

	const float ProcessAfter = 0.01f; // Seconds

	public float Progress => (!Input.Down( Button ) ? 0.0f : timeSinceHeld / ProcessAfter);

	public HoldAction( string name, string button ) : base( name, button )
	{
		Event.Register( this );
	}

	~HoldAction()
	{
		Event.Unregister( this );
	}

	public override string ToString()
	{
		return $"Hold {Name}: {Button} ({Evaluate()}), {Progress}s";
	}

	public override float Evaluate()
	{
		return (Progress >= 1f && Input.Down( Button )) ? 1.0f : 0.0f;
	}

	[GameEvent.Client.Frame]
	public void OnFrame()
	{
		if ( Input.Pressed( Button ) || Input.Released( Button ) )
			timeSinceHeld = 0;
	}
}

public class BoolAction : BaseInputAction
{
	public string Button { get; set; }

	public BoolAction( string name, string button ) : base( name )
	{
		Button = button;
	}

	public override string ToString()
	{
		return $"Boolean {Name}: {Button} ({Evaluate()})";
	}

	public override float Evaluate()
	{
		return Input.Down( Button ) ? 1.0f : 0.0f;
	}

	public override string GetDisplayButton()
	{
		return Button;
	}
}

public class FloatAction : BaseInputAction
{
	public string PositiveButton { get; set; }
	public string NegativeButton { get; set; }

	public FloatAction( string name, string positiveButton, string negativeButton ) : base( name )
	{
		PositiveButton = positiveButton;
		NegativeButton = negativeButton;
	}

	public override string ToString()
	{
		return $"1D {Name}: {PositiveButton}, {NegativeButton} ({Evaluate()})";
	}

	public override float Evaluate()
	{
		if ( Input.Down( PositiveButton ) )
			return 1.0f;

		if ( Input.Down( NegativeButton ) )
			return -1.0f;

		return 0.0f;
	}

	public override string GetDisplayButton()
	{
		return PositiveButton;
	}
}

public class AxisAction : BaseInputAction
{
	public Func<float> Function { get; set; }
	public string DisplayButton { get; set; }

	public AxisAction( string name, Func<float> function, string displayButton ) : base( name )
	{
		Function = function;
		DisplayButton = displayButton;
	}

	public override string ToString()
	{
		return $"Axis {Name}: {DisplayButton} ({Evaluate()})";
	}

	public override float Evaluate()
	{
		return Function?.Invoke() ?? 0.0f;
	}

	public override string GetDisplayButton()
	{
		return DisplayButton;
	}
}

public class InputLayer
{
	[SkipHotload]
	public static List<BaseInputAction> ControllerActions = new()
	{
		new HoldAction( "fold", "duck"),
		new HoldAction( "raise", "jump" ),
		new HoldAction( "check", "use" ),
		new BoolAction( "community_cards", "attack1" ),

		new BoolAction( "emote.middle_finger", "slot1" ),
		new BoolAction( "emote.thumbs_up", "slot2" ),
		new BoolAction( "emote.thumbs_down", "slot3" ),
		new BoolAction( "emote.pump", "slot4" ),

		new BoolAction( "raise_inc", "slot2" ),
		new BoolAction( "raise_dec", "slot4" ),

		new BoolAction( "raise_max", "slotnext" ),
		new BoolAction( "raise_min", "SlotPrev" ),

		new BoolAction( "show_emotes", "score" )
	};

	[SkipHotload]
	public static List<BaseInputAction> PCActions = new()
	{
		new HoldAction( "fold", "Flashlight" ),
		new HoldAction( "raise", "Reload" ),
		new HoldAction( "check", "View" ),
		new BoolAction( "community_cards", "Duck" ),

		new BoolAction( "emote.middle_finger", "slot1" ),
		new BoolAction( "emote.thumbs_up", "slot2" ),
		new BoolAction( "emote.thumbs_down", "slot3" ),
		new BoolAction( "emote.pump", "slot4" ),

		new BoolAction( "raise_inc", "forward" ),
		new BoolAction( "raise_dec", "backward" ),

		new BoolAction( "raise_max", "slotnext" ),
		new BoolAction( "raise_min", "SlotPrev" ),

		new BoolAction( "show_emotes", "score" )
	};

	public static List<BaseInputAction> Actions
	{
		get
		{
			if ( Input.UsingController )
				return ControllerActions;

			return PCActions;
		}
	}

	public static BaseInputAction GetAction( string name )
	{
		return Actions.FirstOrDefault( x => x.Name == name );
	}

	public static float EvaluateRaw( string name )
	{
		return GetAction( name ).Evaluate();
	}

	public static bool Evaluate( string name )
	{
		return GetAction( name ).Evaluate() > 0.5f;
	}

	[Event.Debug.Overlay( "inputlayer", "Input Layer", "sports_esports" )]
	public static void OnDrawHud()
	{
		if ( !Game.IsClient )
			return;

		OverlayUtils.BoxWithText( new Vector2( 45, 175 ),
			"Input Layer",
			$"{string.Join( "\n", Actions )}" );
	}
}
