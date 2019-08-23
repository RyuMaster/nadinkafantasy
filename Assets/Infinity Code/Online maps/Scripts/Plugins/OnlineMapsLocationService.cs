/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls map using GPS.\n
/// Online Maps Location Service is a wrapper for Unity Location Service.\n
/// http://docs.unity3d.com/ScriptReference/LocationService.html
/// </summary>
[Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Location Service")]
public class OnlineMapsLocationService : OnlineMapsLocationServiceGenericBase<OnlineMapsLocationService>
{
    /// <summary>
    /// Desired service accuracy in meters. 
    /// </summary>
    public float desiredAccuracy = 10;

    /// <summary>
    ///  The minimum distance (measured in meters) a device must move laterally before location is updated.
    /// </summary>
    public float updateDistance = 10;

    private List<LastPositionItem> lastPositions;
    private double lastLocationInfoTimestamp;

    public OnlineMapsXML Save(OnlineMapsXML parent)
    {
        OnlineMapsXML element = parent.Create("LocationService");
        element.Create("DesiredAccuracy", desiredAccuracy);
        element.Create("UpdatePosition", updatePosition);
        element.Create("AutoStopUpdateOnInput", autoStopUpdateOnInput);
        element.Create("RestoreAfter", restoreAfter);

        element.Create("CreateMarkerInUserPosition", createMarkerInUserPosition);

        if (createMarkerInUserPosition)
        {
            element.Create("MarkerType", (int)markerType);

            if (markerType == OnlineMapsLocationServiceMarkerType.twoD)
            {
                element.Create("Marker2DAlign", (int)marker2DAlign);
                element.Create("Marker2DTexture", marker2DTexture);
            }
            else element.Create("Marker3DPrefab", marker3DPrefab);

            element.Create("MarkerTooltip", markerTooltip);
            element.Create("UseCompassForMarker", useCompassForMarker);
        }

        element.Create("UseGPSEmulator", useGPSEmulator);
        if (useGPSEmulator)
        {
            element.Create("EmulatorPosition", emulatorPosition);
            element.Create("EmulatorCompass", emulatorCompass);
        }

        return element;
    }

    public override void UpdateSpeed()
    {
        LocationInfo lastData = Input.location.lastData;
        if (Math.Abs(lastLocationInfoTimestamp - lastData.timestamp) < double.Epsilon) return;

        float longitude = lastData.longitude; 
        float latitude = lastData.latitude;
        if (OnGetLocation != null) OnGetLocation(out longitude, out latitude);

        lastLocationInfoTimestamp = lastData.timestamp;

        if (lastPositions == null) lastPositions = new List<LastPositionItem>();

        lastPositions.Add(new LastPositionItem(longitude, latitude, lastData.timestamp));
        while (lastPositions.Count > maxPositionCount) lastPositions.RemoveAt(0);

        if (lastPositions.Count < 2)
        {
            _speed = 0;
            return;
        }

        LastPositionItem p1 = lastPositions[0];
        LastPositionItem p2 = lastPositions[lastPositions.Count - 1];

        double dx, dy;
        OnlineMapsUtils.DistanceBetweenPoints(p1.lng, p1.lat, p2.lng, p2.lat, out dx, out dy);
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = (p2.timestamp - p1.timestamp) / 3600;
        _speed = Mathf.Abs((float) (distance / time));
    }

    /// <summary>
    /// Starts location service updates. Last location coordinates could be.
    /// </summary>
    /// <param name="desiredAccuracyInMeters">
    /// Desired service accuracy in meters. \n
    /// Using higher value like 500 usually does not require to turn GPS chip on and thus saves battery power. \n
    /// Values like 5-10 could be used for getting best accuracy. Default value is 10 meters.</param>
    /// <param name="updateDistanceInMeters">
    /// The minimum distance (measured in meters) a device must move laterally before Input.location property is updated. \n
    /// Higher values like 500 imply less overhead.
    /// </param>
    public void StartLocationService(float? desiredAccuracyInMeters = null, float? updateDistanceInMeters = null)
    {
        if (!desiredAccuracyInMeters.HasValue) desiredAccuracyInMeters = desiredAccuracy;
        if (!updateDistanceInMeters.HasValue) updateDistanceInMeters = updateDistance;

        Input.location.Start(desiredAccuracyInMeters.Value, updateDistanceInMeters.Value);
    }

    public override bool TryStartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        { 
            return false;
        }
        else
        {
            StartLocationService();
            return true;
        }
    }

    public override bool IsLocationServiceRunning()
    {
        return Input.location.status == LocationServiceStatus.Running;
    }

    public override void GetLocation(out float longitude, out float latitude)
    {
        LocationInfo data = Input.location.lastData;
        longitude = data.longitude;
        latitude = data.latitude;
    }

    public override void StopLocationService()
    {
        Input.location.Stop();
    }

    internal struct LastPositionItem
    {
        public float lat;
        public float lng;
        public double timestamp;

        public LastPositionItem(float longitude, float latitude, double timestamp)
        {
            lng = longitude;
            lat = latitude;
            this.timestamp = timestamp;
        }
    }
}