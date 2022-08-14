using Sandbox;
using SandboxEditor;
using System;
using System.Collections.Generic;
using System.Linq;

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

	[Event.Frame]
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
	public static List<BaseInputAction> ControllerActions = new()
	{
		new HoldAction( "fold", InputButton.Use ),
		new AxisAction( "adjust_amount", () => Input.Forward, InputButton.Run ),
		new HoldAction( "submit", InputButton.Jump ),
		new BoolAction( "your_cards", InputButton.SecondaryAttack ),
		new BoolAction( "community_cards", InputButton.PrimaryAttack ),
		new BoolAction( "list_players", InputButton.Score ),
		new HoldAction( "all_in", InputButton.Duck )
	};

	public static List<BaseInputAction> PCActions = new()
	{
		new HoldAction( "fold", InputButton.Flashlight ),
		new FloatAction( "adjust_amount", InputButton.Forward, InputButton.Back ),
		new HoldAction( "submit", InputButton.Jump ),
		new BoolAction( "your_cards", InputButton.Run ),
		new BoolAction( "community_cards", InputButton.Duck ),
		new BoolAction( "list_players", InputButton.Score ),
		new HoldAction( "all_in", InputButton.Use )
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

	public static float Evaluate( string name )
	{
		return GetAction( name ).Evaluate();
	}

	[DebugOverlay( "inputlayer", "Input Layer", "sports_esports" )]
	public static void OnDrawHud()
	{
		if ( !Host.IsClient )
			return;

		OverlayUtils.BoxWithText( Render.Draw2D, new Vector2( 45, 175 ),
			"Input Layer",
			$"{string.Join( "\n", Actions )}" );
	}
}
