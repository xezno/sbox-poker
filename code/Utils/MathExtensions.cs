public static class MathExtensions
{
	public static Rotation LerpTo( this Rotation rotation, Rotation target, float delta )
	{
		return Rotation.Lerp( rotation, target, delta );
	}
}
