/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// The base class of queries to Google API.
/// </summary>
public abstract class OnlineMapsGoogleAPIQuery: OnlineMapsTextWebService
{
    /// <summary>
    /// Event that occurs when the current request instance is disposed.
    /// </summary>
    public new Action<OnlineMapsGoogleAPIQuery> OnDispose;

    /// <summary>
    /// Event that occurs after OnComplete, when the response from Google API processed.
    /// </summary>
    public new Action<OnlineMapsGoogleAPIQuery> OnFinish;

    /// <summary>
    /// Converts Polyline to point list.
    /// </summary>
    /// <param name="encodedPoints">
    /// The encoded polyline.
    /// </param>
    /// <returns>
    /// A List of Vector2 points;
    /// </returns>
    [Obsolete("OnlineMapsGoogleAPIQuery.DecodePolylinePoints is obsolete. Use OnlineMapsUtils.DecodePolylinePoints.")]
    public static List<Vector2> DecodePolylinePoints(string encodedPoints)
    {
        return OnlineMapsUtils.DecodePolylinePoints(encodedPoints);
    }

    /// <summary>
    /// Converts XMLNode coordinates from Google Maps into Vector2.
    /// </summary>
    /// <param name="node">XMLNode coordinates from Google Maps.</param>
    /// <returns>Coordinates as Vector2.</returns>
    [Obsolete("OnlineMapsGoogleAPIQuery.GetVector2FromNode is obsolete. Use OnlineMapsXML.GetVector2FromNode.")]
    public static Vector2 GetVector2FromNode(OnlineMapsXML node)
    {
        return OnlineMapsXML.GetVector2FromNode(node);
    }
}