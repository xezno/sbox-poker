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

	public virtual InputButton GetDisplayButton()
	{
		return InputButton.Slot0;
	}
}

public class HoldAction : BoolAction
{
	private TimeSince timeSinceHeld;

	const float ProcessAfter = 1.5f; // Seconds

	public float Progress => (!Input.Down( Button ) ? 0.0f : timeSinceHeld / ProcessAfter);

	public HoldAction( string name, InputButton button ) : base( name, button )
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

	[Event.Client.Frame]
	public void OnFrame()
	{
		if ( Input.Pressed( Button ) || Input.Released( Button ) )
			timeSinceHeld = 0;
	}
}

public class BoolAction : BaseInputAction
{
	public InputButton Button { get; set; }

	public BoolAction( string name, InputButton button ) : base( name )
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

	public override InputButton GetDisplayButton()
	{
		return Button;
	}
}

public class FloatAction : BaseInputAction
{
	public InputButton PositiveButton { get; set; }
	public InputButton NegativeButton { get; set; }

	public FloatAction( string name, InputButton positiveButton, InputButton negativeButton ) : base( name )
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

	public override InputButton GetDisplayButton()
	{
		return PositiveButton;
	}
}

public class AxisAction : BaseInputAction
{
	public Func<float> Function { get; set; }
	public InputButton DisplayButton { get; set; }

	public AxisAction( string name, Func<float> function, InputButton displayButton ) : base( name )
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

	public override InputButton GetDisplayButton()
	{
		return DisplayButton;
	}
}

public class InputLayer
{
	[SkipHotload]
	public static List<BaseInputAction> ControllerActions = new()
	{
		new HoldAction( "fold", InputButton.Duck ),
		new HoldAction( "raise", InputButton.Jump ),
		new HoldAction( "check", InputButton.Use ),
		new BoolAction( "community_cards", InputButton.PrimaryAttack ),

		new BoolAction( "emote.middle_finger", InputButton.Slot1 ),
		new BoolAction( "emote.thumbs_up", InputButton.Slot2 ),
		new BoolAction( "emote.thumbs_down", InputButton.Slot3 ),
		new BoolAction( "emote.pump", InputButton.Slot4 ),

		new BoolAction( "raise_inc", InputButton.Slot2 ),
		new BoolAction( "raise_dec", InputButton.Slot4 ),

		new BoolAction( "show_emotes", InputButton.Score )
	};

	[SkipHotload]
	public static List<BaseInputAction> PCActions = new()
	{
		new HoldAction( "fold", InputButton.Flashlight ),
		new HoldAction( "raise", InputButton.Reload ),
		new HoldAction( "check", InputButton.View ),
		new BoolAction( "community_cards", InputButton.Duck ),

		new BoolAction( "emote.middle_finger", InputButton.Slot1 ),
		new BoolAction( "emote.thumbs_up", InputButton.Slot2 ),
		new BoolAction( "emote.thumbs_down", InputButton.Slot3 ),
		new BoolAction( "emote.pump", InputButton.Slot4 ),

		new BoolAction( "raise_inc", InputButton.Forward ),
		new BoolAction( "raise_dec", InputButton.Back ),

		new BoolAction( "show_emotes", InputButton.Score )
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
