/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using UnityEngine;

/// <summary>
/// Controls map using Location Service.\n
/// </summary>
public abstract class OnlineMapsLocationServiceBase : MonoBehaviour
{
    private static OnlineMapsLocationServiceBase _baseInstance;

    public delegate void OnGetLocationDelegate(out float longitude, out float latitude);

    /// <summary>
    /// This event is called when the user rotates the device.
    /// </summary>
    public Action<float> OnCompassChanged;

    public OnGetLocationDelegate OnGetLocation;

    public Action OnFindLocationByIPComplete;

    /// <summary>
    /// This event is called when changed your GPS location.
    /// </summary>
    public Action<Vector2> OnLocationChanged;

    /// <summary>
    /// This event is called when the GPS is initialized (the first value is received) or location by IP is found.
    /// </summary>
    public Action OnLocationInited;

    /// <summary>
    /// Update stop position when user input.
    /// </summary>
    public bool autoStopUpdateOnInput = true;

    /// <summary>
    /// Threshold of compass.
    /// </summary>
    public float compassThreshold = 8;

    /// <summary>
    /// Specifies the need to create a marker that indicates the current GPS coordinates.
    /// </summary>
    public bool createMarkerInUserPosition = false;

    public bool disableEmulatorInPublish = true;

    /// <summary>
    /// Emulated compass trueHeading.\n
    /// Do not use.\n
    /// Use OnlineMapsLocationService.trueHeading.
    /// </summary>
    public float emulatorCompass;

    /// <summary>
    /// Emulated GPS position.\n
    /// Do not use.\n
    /// Use OnlineMapsLocationService.position.
    /// </summary>
    public Vector2 emulatorPosition;

    /// <summary>
    /// Specifies whether to search for a location by IP.
    /// </summary>
    public bool findLocationByIP = false;

    /// <summary>
    /// Tooltip of the marker.
    /// </summary>
    public string markerTooltip;

    /// <summary>
    /// Type of the marker.
    /// </summary>
    public OnlineMapsLocationServiceMarkerType markerType = OnlineMapsLocationServiceMarkerType.twoD;

    /// <summary>
    /// Align of the 2D marker.
    /// </summary>
    public OnlineMapsAlign marker2DAlign = OnlineMapsAlign.Center;

    /// <summary>
    /// Texture of 2D marker.
    /// </summary>
    public Texture2D marker2DTexture;

    /// <summary>
    /// Prefab of 3D marker.
    /// </summary>
    public GameObject marker3DPrefab;

    /// <summary>
    /// The maximum number of stored positions./n
    /// It is used to calculate the speed.
    /// </summary>
    public int maxPositionCount = 3;

    /// <summary>
    /// Current GPS coordinates.\n
    /// <strong>Important: position not available Start, because GPS is not already initialized. \n
    /// Use OnLocationInited event, to determine the initialization of GPS.</strong>
    /// </summary>
    public Vector2 position = Vector2.zero;

    /// <summary>
    /// Use the GPS coordinates after seconds of inactivity.
    /// </summary>
    public int restoreAfter = 10;

    /// <summary>
    /// The heading in degrees relative to the geographic North Pole.\n
    /// <strong>Important: position not available Start, because compass is not already initialized. \n
    /// Use OnCompassChanged event, to determine the initialization of compass.</strong>
    /// </summary>
    public float trueHeading = 0;

    /// <summary>
    /// Specifies whether the script will automatically update the location.
    /// </summary>
    public bool updatePosition = true;

    /// <summary>
    /// Specifies the need for marker rotation.
    /// </summary>
    public bool useCompassForMarker = false;

    /// <summary>
    /// Specifies GPS emulator usage. \n
    /// Works only in Unity Editor.
    /// </summary>
    public bool useGPSEmulator = false;

    private OnlineMaps api;

    private bool _allowUpdatePosition = true;
    private long lastPositionChangedTime;
    private bool lockDisable;
    private bool isPositionInited = false;

    private OnlineMapsMarkerBase _marker;
    protected float _speed = 0;
    private bool started = false;

    /// <summary>
    /// Instance of LocationService base.
    /// </summary>
    public static OnlineMapsLocationServiceBase baseInstance
    {
        get { return _baseInstance; }
    }

    /// <summary>
    /// Instance of marker.
    /// </summary>
    public static OnlineMapsMarkerBase marker
    {
        get { return _baseInstance._marker; }
        set { _baseInstance._marker = value; }
    }

    public bool allowUpdatePosition
    {
        get { return _allowUpdatePosition; }
        set
        {
            if (value == _allowUpdatePosition) return;
            _allowUpdatePosition = value;
            if (value) UpdatePosition();
        }
    }

    /// <summary>
    /// Speed km/h.
    /// Note: in Unity Editor will always be zero.
    /// </summary>
    public float speed
    {
        get { return _speed; }
    }

    private void OnChangePosition()
    {
        if (lockDisable) return;

        lastPositionChangedTime = DateTime.Now.Ticks;
        if (autoStopUpdateOnInput) _allowUpdatePosition = false;
    }

    protected virtual void OnEnable()
    {
        _baseInstance = this;
        if (api != null) api.OnChangePosition += OnChangePosition;
    }

    private void OnFindLocationComplete(OnlineMapsWWW www)
    {
        if (!string.IsNullOrEmpty(www.error)) return;

        string response = www.text;
        if (string.IsNullOrEmpty(response)) return;

        int index = 0;
        string s = "\"loc\": \"";
        float lat = 0, lng = 0;
        bool finded = false;
        for (int i = 0; i < response.Length; i++)
        {
            if (response[i] == s[index])
            {
                index++;
                if (index >= s.Length)
                {
                    i++;
                    int startIndex = i;
                    while (true)
                    {
                        char c = response[i];
                        if (c == ',')
                        {
                            lat = float.Parse(response.Substring(startIndex, i - startIndex));
                            i++;
                            startIndex = i;
                        }
                        else if (c == '"')
                        {
                            lng = float.Parse(response.Substring(startIndex, i - startIndex));
                            finded = true;
                            break;
                        }
                        i++;
                    }
                    break;
                }
            }
            else index = 0;
        }

        if (finded)
        {
            if (useGPSEmulator) emulatorPosition = new Vector2(lng, lat);
            else if (position == Vector2.zero)
            {
                position = new Vector2(lng, lat);
                if (!isPositionInited && OnLocationInited != null)
                {
                    isPositionInited = true;
                    OnLocationInited();
                }
                if (OnLocationChanged != null) OnLocationChanged(position);
            }
            if (OnFindLocationByIPComplete != null) OnFindLocationByIPComplete();
        }
    }

    private void Start()
    {
        api = OnlineMaps.instance;
        api.OnChangePosition += OnChangePosition;

        if (findLocationByIP)
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            OnlineMapsWWW findByIPRequest = OnlineMapsUtils.GetWWW("https://ipinfo.io/json");
#else
            OnlineMapsWWW findByIPRequest = OnlineMapsUtils.GetWWW("http://service.infinity-code.com/getlocation.php");
#endif
            findByIPRequest.OnComplete += OnFindLocationComplete;
        }
    }

    private void Update()
    {
        if (api == null)
        {
            api = OnlineMaps.instance;
            if (api == null) return;
        }

        try
        {
            if (!started)
            {
#if !UNITY_EDITOR
                Input.compass.enabled = true;
                if(!TryStartLocationService())
                    return;
#endif
                started = true;
            }

#if !UNITY_EDITOR
            if (disableEmulatorInPublish) useGPSEmulator = false;
#endif
            bool positionChanged = false;

            if (createMarkerInUserPosition && _marker == null && (useGPSEmulator || position != Vector2.zero)) UpdateMarker();

            if (!useGPSEmulator && !IsLocationServiceRunning()) return;

            bool compassChanged = false;

            if (useGPSEmulator)
            {
                UpdateCompassFromEmulator(ref compassChanged);
                if (!isPositionInited) positionChanged = true;
            }
            else UpdateCompassFromInput(ref compassChanged);

            UpdateSpeed();

            if (useGPSEmulator) UpdatePositionFromEmulator(ref positionChanged);
            else UpdatePositionFromInput(ref positionChanged);

            if (positionChanged)
            {
                if (!isPositionInited)
                {
                    isPositionInited = true;
                    if (OnLocationInited != null) OnLocationInited();
                }
                if (OnLocationChanged != null) OnLocationChanged(position);
            }

            if (createMarkerInUserPosition && (positionChanged || compassChanged)) UpdateMarker();

            if (updatePosition)
            {
                if (_allowUpdatePosition)
                {
                    UpdatePosition();
                }
                else if (restoreAfter > 0 && DateTime.Now.Ticks > lastPositionChangedTime + OnlineMapsUtils.second * restoreAfter)
                {
                    _allowUpdatePosition = true;
                    UpdatePosition();
                }
            }
        }
        catch /*(Exception exception)*/
        {
            //errorMessage = exception.Message + "\n" + exception.StackTrace;
        }
    }

    private void UpdateCompassFromEmulator(ref bool compassChanged)
    {
        if (Math.Abs(trueHeading - emulatorCompass) > float.Epsilon)
        {
            compassChanged = true;
            trueHeading = emulatorCompass;
            if (OnCompassChanged != null) OnCompassChanged(trueHeading / 360);
        }
    }

    private void UpdateCompassFromInput(ref bool compassChanged)
    {
        float heading = Input.compass.trueHeading;
        float offset = trueHeading - heading;

        if (offset > 360) offset -= 360;
        else if (offset < -360) offset += 360;

        if (Mathf.Abs(offset) > compassThreshold)
        {
            compassChanged = true;
            trueHeading = heading;
            if (OnCompassChanged != null) OnCompassChanged(trueHeading / 360);
        }
    }

    private void UpdateMarker()
    {
        if (_marker == null)
        {
            if (markerType == OnlineMapsLocationServiceMarkerType.twoD)
            {
                _marker = OnlineMaps.instance.AddMarker(position, marker2DTexture, markerTooltip);
                (_marker as OnlineMapsMarker).align = marker2DAlign;
            }
            else
            {
                OnlineMapsControlBase3D control = OnlineMapsControlBase3D.instance;
                if (control == null)
                {
                    Debug.LogError("You must use the 3D control (Texture or Tileset).");
                    createMarkerInUserPosition = false;
                    return;
                }
                _marker = control.AddMarker3D(position, marker3DPrefab);
                _marker.label = markerTooltip;
            }
        }
        else
        {
            _marker.position = position;
        }

        if (useCompassForMarker)
        {
            if (markerType == OnlineMapsLocationServiceMarkerType.twoD) (_marker as OnlineMapsMarker).rotation = trueHeading / 360;
            else (_marker as OnlineMapsMarker3D).rotation = Quaternion.Euler(0, trueHeading, 0);
        }

        api.Redraw();
    }

    /// <summary>
    /// Sets map position using GPS coordinates.
    /// </summary>
    public void UpdatePosition()
    {
        if (!useGPSEmulator && position == Vector2.zero) return;
        if (api == null) return;

        lockDisable = true;

        Vector2 p = api.position;
        bool changed = false;

        if (Math.Abs(p.x - position.x) > float.Epsilon)
        {
            p.x = position.x;
            changed = true;
        }
        if (Math.Abs(p.y - position.y) > float.Epsilon)
        {
            p.y = position.y;
            changed = true;
        }
        if (changed)
        {
            api.position = p;
            api.Redraw();
        }

        lockDisable = false;
    }

    private void UpdatePositionFromEmulator(ref bool positionChanged)
    {
        if (Math.Abs(position.x - emulatorPosition.x) > float.Epsilon)
        {
            position.x = emulatorPosition.x;
            positionChanged = true;
        }
        if (Math.Abs(position.y - emulatorPosition.y) > float.Epsilon)
        {
            position.y = emulatorPosition.y;
            positionChanged = true;
        }
    }

    private void UpdatePositionFromInput(ref bool positionChanged)
    {
        float longitude;
        float latitude;

        if (OnGetLocation != null) OnGetLocation(out longitude, out latitude);
        else
        {
            GetLocation(out longitude, out latitude);
        }

        if (Math.Abs(position.x - longitude) > float.Epsilon)
        {
            position.x = longitude;
            positionChanged = true;
        }
        if (Math.Abs(position.y - latitude) > float.Epsilon)
        {
            position.y = latitude;
            positionChanged = true;
        }
    }

    public abstract void UpdateSpeed();

    public abstract bool TryStartLocationService();

    public abstract void StopLocationService();

    public abstract bool IsLocationServiceRunning();

    public abstract void GetLocation(out float longitude, out float latitude);
}
