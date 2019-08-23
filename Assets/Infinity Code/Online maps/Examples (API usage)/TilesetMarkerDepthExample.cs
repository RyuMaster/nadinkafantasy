/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to change the sort order of the markers.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetMarkerDepthExample")]
    public class TilesetMarkerDepthExample : MonoBehaviour
    {
        /// <summary>
        /// Defines a new comparer.
        /// </summary>
        public class MarkerComparer : IComparer<OnlineMapsMarker>
        {
            public int Compare(OnlineMapsMarker m1, OnlineMapsMarker m2)
            {
                if (m1.position.y > m2.position.y) return -1;
                if (m1.position.y < m2.position.y) return 1;
                return 0;
            }
        }

        private void Start()
        {
            OnlineMaps map = OnlineMaps.instance;

            // Create markers.
            map.AddMarker(new Vector2(0, 0));
            map.AddMarker(new Vector2(0, 0.01f));
            map.AddMarker(new Vector2(0, -0.01f));

            // Sets a new comparer.
            OnlineMapsTileSetControl.instance.markerComparer = new MarkerComparer();

            // Get the center point and zoom the best for all markers.
            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(map.markers, out center, out zoom);

            // Change the position and zoom of the map.
            map.position = center;
            map.zoom = zoom;
        }
    }
}