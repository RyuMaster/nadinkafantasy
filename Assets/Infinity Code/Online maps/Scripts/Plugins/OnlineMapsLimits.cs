/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEngine;

/// <summary>
/// Class to limit the position and zoom of the map.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Limits")]
[System.Serializable]
public class OnlineMapsLimits : MonoBehaviour
{
    /// <summary>
    /// The minimum zoom value.
    /// </summary>
    public int minZoom = 3;

    /// <summary>
    /// The maximum zoom value. 
    /// </summary>
    public int maxZoom = OnlineMaps.MAXZOOM;

    /// <summary>
    /// The minimum latitude value.
    /// </summary>
    public float minLatitude = -90;

    /// <summary>
    /// The maximum latitude value. 
    /// </summary>
    public float maxLatitude = 90;

    /// <summary>
    /// The minimum longitude value.
    /// </summary>
    public float minLongitude = -180;

    /// <summary>
    /// The maximum longitude value. 
    /// </summary>
    public float maxLongitude = 180;

    /// <summary>
    /// Type of limitation position map.
    /// </summary>
    public OnlineMapsPositionRangeType positionRangeType = OnlineMapsPositionRangeType.center;

    /// <summary>
    /// Flag indicating that need to limit the zoom.
    /// </summary>
    public bool useZoomRange;

    /// <summary>
    /// Flag indicating that need to limit the position.
    /// </summary>
    public bool usePositionRange;

    public void Start()
    {
        if (useZoomRange) OnlineMaps.instance.zoomRange = new OnlineMapsRange(minZoom, maxZoom);
        if (usePositionRange) OnlineMaps.instance.positionRange = new OnlineMapsPositionRange(minLatitude, minLongitude, maxLatitude, maxLongitude, positionRangeType);
    }
}
