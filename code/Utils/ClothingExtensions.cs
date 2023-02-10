namespace Poker;

public static class ClothingExtensions
{
	public static void LoadRandom( this Poker.ClothingContainer clothingContainer )
	{
		var clothingItems = ResourceLibrary.GetAll<Clothing>().GroupBy( x => x.Category ).ToList();
		var randomClothingItems = clothingItems.Select( x => Game.Random.FromArray( x.ToArray() ) );

		foreach ( var clothingItem in randomClothingItems )
		{
			// Random chance to not fill this slot (10%)
			if ( Game.Random.Int( 0, 9 ) == 0 )
				continue;

			// Check if we have anything we can't wear with this
			if ( clothingContainer.Clothing.Where( x => !x.CanBeWornWith( clothingItem ) ).Any() )
				continue;

			clothingContainer.Clothing.Add( clothingItem );
		}
	}
}
