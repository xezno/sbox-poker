using System;

public static class GraphicsX
{
	public static List<Vertex> VertexList { get; } = new();

	public static void AddVertex( in Vertex v )
	{
		VertexList.Add( v );
	}

	public static void AddVertex( in Vector2 v, in Color color )
	{
		Vertex v2 = new Vertex
		{
			Position = v,
			Color = color
		};

		AddVertex( in v2 );
	}

	public static void AddVertex( in Vector2 v, in Color color, in Vector2 uv )
	{
		Vertex v2 = new Vertex
		{
			Position = v,
			Color = color,
			TexCoord0 = uv
		};

		AddVertex( in v2 );
	}

	public static void MeshStart()
	{
		VertexList.Clear();
	}

	public static void MeshEnd( RenderAttributes _attributes = null )
	{
		RenderAttributes attributes = _attributes ?? new RenderAttributes();

		attributes.Set( "Texture", Texture.White );
		Span<Vertex> vertices = VertexList.ToArray();
		Graphics.Draw( vertices, VertexList.Count, Material.UI.Basic, attributes );
		VertexList.Clear();
	}

	public static void Circle( in Vector2 center, in Color color, float radius, int points = 32 )
	{
		Circle( in center, radius, 0f, in color, points );
	}

	public static void Ring( in Vector2 center, float radius, in Color color, float hole_radius, int points = 32 )
	{
		Circle( in center, radius, hole_radius, in color, points );
	}

	public static void Circle( in Vector2 center, float outer, float inner, in Color color, int points = 32, float startAngle = 0f, float endAngle = 360f, float uv = 0f )
	{
		MeshStart();
		float twoPI = MathF.PI * 2f;
		startAngle = startAngle.NormalizeDegrees().DegreeToRadian();
		for ( endAngle = endAngle.NormalizeDegrees().DegreeToRadian(); endAngle <= startAngle; endAngle += twoPI )
		{
		}

		float num2 = (endAngle - startAngle) % (twoPI + 0.01f);
		if ( num2 <= 0f )
		{
			return;
		}

		float num3 = twoPI / (float)points;

		for ( float num4 = startAngle; num4 < endAngle; num4 += num3 )
		{
			float num5 = num4;
			float num6 = num4 + num3;

			if ( num6 > endAngle )
			{
				num6 = endAngle;
			}

			num5 += MathF.PI;
			num6 += MathF.PI;

			Vector2 vector = new Vector2( MathF.Sin( 0f - num5 ), MathF.Cos( 0f - num5 ) );
			Vector2 vector2 = new Vector2( MathF.Sin( 0f - num6 ), MathF.Cos( 0f - num6 ) );
			Vector2 uv2 = vector / 2f + 0.5f;
			Vector2 uv3 = vector2 / 2f + 0.5f;
			Vector2 uv4 = vector * inner / outer / 2f + 0.5f;
			Vector2 uv5 = vector2 * inner / outer / 2f + 0.5f;

			if ( uv > 0f )
			{
				uv2 = new Vector2( (num5 - MathF.PI - startAngle) * uv / twoPI, 0f );
				uv3 = new Vector2( (num6 - MathF.PI - startAngle) * uv / twoPI, 0f );
				uv4 = uv2.WithY( 1f );
				uv5 = uv3.WithY( 1f );
			}

			Vector2 v = center + vector * outer;
			AddVertex( in v, in color, in uv2 );

			v = center + vector2 * outer;
			AddVertex( in v, in color, in uv3 );

			v = center + vector * inner;
			AddVertex( in v, in color, in uv4 );

			if ( inner > 0f )
			{
				v = center + vector2 * outer;
				AddVertex( in v, in color, in uv3 );

				v = center + vector2 * inner;
				AddVertex( in v, in color, in uv5 );

				v = center + vector * inner;
				AddVertex( in v, in color, in uv4 );
			}
		}

		MeshEnd();
	}

	public static void Line( in Color color, in float thickness0, in Vector2 pos0, in float thickness1, in Vector2 pos1 )
	{
		MeshStart();

		Vector2 vector = pos1 - pos0;
		Vector2 vector2 = vector.Perpendicular.Normal * -0.5f;
		Vector2 v = pos0 + vector2 * thickness0;
		Vector2 v2 = pos0 + vector + vector2 * thickness1;
		Vector2 v3 = pos0 + vector - vector2 * thickness1;
		Vector2 v4 = pos0 - vector2 * thickness0;

		Vector2 uv = new Vector2( 0f, 0f );
		AddVertex( in v, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in v2, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in v4, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in v2, in color, in uv );

		uv = new Vector2( 1f, 1f );
		AddVertex( in v3, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in v4, in color, in uv );

		MeshEnd();
	}

	public static void Line( in Color color, in float t0, in Vector2 p0, in Vector2 p1 )
	{
		Line( in color, in t0, in p0, in t0, in p1 );
	}
}
