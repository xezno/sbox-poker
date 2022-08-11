using System.Collections.Generic;
using System.Text.Json;

namespace Poker.Backend;

public static class Serializer
{
	public static string Serialize<T>( T obj )
	{
		return JsonSerializer.Serialize<T>( obj );
	}

	public static T? Deserialize<T>( string serialized )
	{
		return JsonSerializer.Deserialize<T>( serialized );
	}

	public static Dictionary<string, object> FromStruct( object obj )
	{
		var dict = new Dictionary<string, object>();
		var type = obj.GetType();
		var properties = type.GetProperties();

		foreach ( var property in properties )
		{
			var value = property.GetValue( obj );
			dict.Add( property.Name, value );
		}

		return dict;
	}
}
