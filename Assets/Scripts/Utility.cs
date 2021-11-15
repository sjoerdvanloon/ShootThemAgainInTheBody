using System.Collections;
using System.Collections.Generic;

public static class Utility 
{
   public static T[] Shuffle<T>(T[] array, int seed = 0)
    {
        
        // Fisher-Yates shuffle
        var rand = new System.Random(seed);

        for (int i = 0; i < array.Length -1; i++) // -1 because ignore last item
        {
            var randomIndex = rand.Next(i, array.Length );
            var tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;

    }
}
