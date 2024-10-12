using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.Helper
{
    public static class CollectionHelper
    {
        public static List<T> Shuffle<T>(List<T> list)
        {
            var rand = new System.Random();

            int count = list.Count;
            for (int i = count - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }
    }
}