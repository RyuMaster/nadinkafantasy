/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleDirections.
/// </summary>
[Obsolete("OnlineMapsFindDirectionAdvanced is obsolete. Use OnlineMapsGoogleDirections.")]
public class OnlineMapsFindDirectionAdvanced:OnlineMapsGoogleDirections
{
    public static OnlineMapsGoogleAPIQuery Find(
        Vector2 origin, 
        Vector2 destination, 
        Mode mode = Mode.driving, 
        string[] waypoints = null, 
        bool alternatives = false, 
        Avoid avoid = Avoid.none, 
        Units units = Units.metric, 
        string region = null, 
        long departure_time = -1, 
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin.y + "," + origin.x, destination.y + "," + destination.x, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    public static OnlineMapsGoogleAPIQuery Find(
        Vector2 origin,
        string destination,
        Mode mode = Mode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        Avoid avoid = Avoid.none,
        Units units = Units.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin.y + "," + origin.x, destination, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    public static OnlineMapsGoogleAPIQuery Find(
        string origin,
        Vector2 destination,
        Mode mode = Mode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        Avoid avoid = Avoid.none,
        Units units = Units.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin, destination.y + "," + destination.x, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    public static OnlineMapsGoogleAPIQuery Find(
        string origin,
        string destination,
        Mode mode = Mode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        Avoid avoid = Avoid.none,
        Units units = Units.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        return Find(new Params(origin, destination)
        {
            mode = mode,
            waypoints = waypoints != null? waypoints.Cast<object>().ToArray(): null,
            alternatives = alternatives,
            avoid = avoid,
            units = units,
            region = region,
            departure_time = departure_time > 0? (object)departure_time: null,
            arrival_time = arrival_time > 0? (long?)arrival_time: null,
            language = language
        });
    }

    public new static OnlineMapsFindDirectionResult GetResult(string response)
    {
        try
        {
            OnlineMapsXML xml = OnlineMapsXML.Load(response);
            return new OnlineMapsFindDirectionResult(xml);
        }
        catch { }

        return null;
    }

}