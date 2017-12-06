using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Extends lists to add extension method. 
 * 
 * @author https://forum.unity.com/members/smooth-p.132555/
 */
namespace CustomExtensions {

    /// <summary>
    /// Extends the List class and adds a convenience shuffle method.
    /// </summary>
	public static class ListExtensions {

        /// <summary>
        /// Implements a shuffle method to randomize a list in place.
        /// </summary>
        /// 
        /// <param name="inputList">The input list to be shuffled.</param>
        /// <typeparam name="T">The type of item in the list being shuffled.</typeparam>
        /// 
        /// <returns>The shuffled list..</returns>
        public static void Shuffle<T>(this IList<T> inputList) {
			
            // for use in determining number of iterations.
            var count = inputList.Count;
			var last = count - 1;

            // Go through the list and randomize the list.
			for (var i = 0; i < last; ++i) {

                // Grab the list items to swap. 
				var r = UnityEngine.Random.Range(i, count);
				var tmp = inputList[i];

                // Swap the list items. 
				inputList[i] = inputList[r];
				inputList[r] = tmp;
			}
		}
	}
}
