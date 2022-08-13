using Sandbox;
using SandboxEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker;

public class InputAction
{
	public string Name { get; set; }

	public InputAction( string name )
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

public class ActionBool : InputAction
{
	public InputButton Button { get; set; }

	public ActionBool( string name, InputButton button ) : base( name )
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

public class Action1D : InputAction
{
	public InputButton PositiveButton { get; set; }
	public InputButton NegativeButton { get; set; }

	public Action1D( string name, InputButton positiveButton, InputButton negativeButton ) : base( name )
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

public class ActionAxis : InputAction
{
	public Func<float> Function { get; set; }
	public InputButton DisplayButton { get; set; }

	public ActionAxis( string name, Func<float> function, InputButton displayButton ) : base( name )
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
	public static List<InputAction> ControllerActions => new()
	{
		new ActionBool( "fold", InputButton.Use ),
		new ActionAxis( "adjust_amount", () => Input.Forward, InputButton.Run ),
		new ActionBool( "submit", InputButton.Jump ),
		new ActionBool( "your_cards", InputButton.SecondaryAttack ),
		new ActionBool( "community_cards", InputButton.PrimaryAttack ),
		new ActionBool( "list_players", InputButton.Score )
	};

	public static List<InputAction> PCActions => new()
	{
		new ActionBool( "fold", InputButton.Flashlight ),
		new Action1D( "adjust_amount", InputButton.Forward, InputButton.Back ),
		new ActionBool( "submit", InputButton.Jump ),
		new ActionBool( "your_cards", InputButton.Run ),
		new ActionBool( "community_cards", InputButton.Duck ),
		new ActionBool( "list_players", InputButton.Score )
	};

	public static List<InputAction> Actions
	{
		get
		{
			if ( Input.UsingController )
				return ControllerActions;

			return PCActions;
		}
	}

	public static InputAction GetAction( string name )
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
