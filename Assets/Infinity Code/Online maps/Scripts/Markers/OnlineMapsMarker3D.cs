/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 3D marker class.\n
/// <strong>Can be used only when the source display - Texture or Tileset.</strong>\n
/// To create a new 3D marker use OnlineMapsControlBase3D.AddMarker3D.
/// </summary>
[Serializable]
public class OnlineMapsMarker3D : OnlineMapsMarkerBase
{
    /// <summary>
    /// Specifies whether to use a marker event for 3D markers. \n
    /// Otherwise you will have to create their own events using MonoBehaviour.
    /// </summary>
    public bool allowDefaultMarkerEvents;

    /// <summary>
    /// Altitude (meters).
    /// </summary>
    public float? altitude;

    /// <summary>
    /// Need to check the map boundaries? \n
    /// It allows you to make 3D markers, which are active outside the map.
    /// </summary>
    public bool checkMapBoundaries = true;

    /// <summary>
    /// Reference of 3D control.
    /// </summary>
    public OnlineMapsControlBase3D control;

    /// <summary>
    /// Specifies whether the marker is initialized.
    /// </summary>
    [HideInInspector] 
    public bool inited = false;

    /// <summary>
    /// The instance.
    /// </summary>
    [HideInInspector]
    public GameObject instance;

    /// <summary>
    /// Marker prefab GameObject.
    /// </summary>
    public GameObject prefab;

    private GameObject _prefab;
    private Vector3 _relativePosition;
    private bool _visible = true;

    [SerializeField]
    private float _rotationY = 0;

    /// <summary>
    /// Gets or sets marker enabled.
    /// </summary>
    /// <value>
    /// true if enabled, false if not.
    /// </value>
    public override bool enabled
    {
        set
        {
            if (_enabled != value)
            {
                _enabled = value;

                if (!value) visible = false;
                else Update();

                if (OnEnabledChange != null) OnEnabledChange(this);
            }
        }
    }

    /// <summary>
    /// Returns the position of the marker relative to Texture.
    /// </summary>
    /// <value>
    /// The relative position.
    /// </value>
    public Vector3 relativePosition
    {
        get
        {
            return enabled ? _relativePosition : Vector3.zero;
        }
    }

    /// <summary>
    /// Gets or sets rotation of 3D marker.
    /// </summary>
    public Quaternion rotation
    {
        get { return transform != null? transform.rotation: new Quaternion(); }
        set
        {
            if (transform != null)
            {
                transform.rotation = value;
                _rotationY = value.eulerAngles.y;
            }
        }
    }

    /// <summary>
    /// Y rotation of 3D marker.
    /// </summary>
    public float rotationY
    {
        get { return _rotationY; }
        set
        {
            _rotationY = value;
            rotation = Quaternion.Euler(0, value, 0);
        }
    }

    /// <summary>
    /// Gets the instance transform.
    /// </summary>
    /// <value>
    /// The transform.
    /// </value>
    public Transform transform
    {
        get
        {
            return instance != null? instance.transform: null;
        }
    }

    private bool visible
    {
        get { return _visible; }
        set
        {
            if (_visible == value) return;
            _visible = value;
            instance.SetActive(value);
        }
    }

    /// <summary>
    /// Constructor of 3D marker
    /// </summary>
    public OnlineMapsMarker3D()
    {
        
    }

    /// <summary>
    /// Create 3D marker from an existing GameObject.
    /// </summary>
    /// <param name="instance">GameObject to be used as a 3D marker.</param>
    public OnlineMapsMarker3D(GameObject instance):this()
    {
        prefab = _prefab = instance;
        this.instance = instance;
        instance.AddComponent<OnlineMapsMarker3DInstance>().marker = this;
        Update();
    }

    /// <summary>
    /// Initialises this object.
    /// </summary>
    /// <param name="parent">
    /// The parent transform.
    /// </param>
    public void Init(Transform parent)
    {
        if (instance != null) OnlineMapsUtils.DestroyImmediate(instance);

        if (prefab == null)
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.transform.localScale = Vector3.one;
        }
        else instance = Object.Instantiate(prefab) as GameObject;

        _prefab = prefab;
        
        instance.transform.parent = parent;
        instance.layer = parent.gameObject.layer;
        instance.AddComponent<OnlineMapsMarker3DInstance>().marker = this;
        visible = false;
        inited = true;

        Update();

        if (OnInitComplete != null) OnInitComplete(this);
    }

    public override void LookToCoordinates(Vector2 coordinates)
    {
        double p1x, p1y, p2x, p2y;
        map.projection.CoordinatesToTile(coordinates.x, coordinates.y, 20, out p1x, out p1y);
        map.projection.CoordinatesToTile(longitude, latitude, 20, out p2x, out p2y);
        rotation = Quaternion.Euler(0, (float)(OnlineMapsUtils.Angle2D(p1x, p1y, p2x, p2y) - 90), 0);
    }

    /// <summary>
    /// Reinitialises this object.
    /// </summary>
    /// <param name="topLeft">
    /// The top left.
    /// </param>
    /// <param name="bottomRight">
    /// The bottom right.
    /// </param>
    /// <param name="zoom">
    /// The zoom.
    /// </param>
    public void Reinit(Vector2 topLeft, Vector2 bottomRight, int zoom)
    {
        Reinit(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y, zoom);
    }

    /// <summary>
    /// Reinitialises this object.
    /// </summary>
    /// <param name="tlx">Top-left longitude of map</param>
    /// <param name="tly">Top-left latitude of map</param>
    /// <param name="brx">Bottom-right longitude of map</param>
    /// <param name="bry">Bottom-right latitude of map</param>
    /// <param name="zoom">Map zoom</param>
    public void Reinit(double tlx, double tly, double brx, double bry, int zoom)
    {
        if (instance)
        {
            Transform parent = instance.transform.parent;
            OnlineMapsUtils.DestroyImmediate(instance);
            Init(parent);
        }
        Update(tlx, tly, brx, bry, zoom);
        if (OnInitComplete != null) OnInitComplete(this);
    }

    public override OnlineMapsXML Save(OnlineMapsXML parent)
    {
        OnlineMapsXML element = base.Save(parent);
        element.Create("Prefab", prefab);
        element.Create("Rotation", rotation.eulerAngles);
        return element;
    }

    public override void Update()
    {
        double tlx, tly, brx, bry;
        map.GetCorners(out tlx, out tly, out brx, out bry);
        Update(tlx, tly, brx, bry, map.zoom);
    }

    /// <summary>
    /// Updates this object.
    /// </summary>
    /// <param name="topLeft">
    /// The top left coordinates.
    /// </param>
    /// <param name="bottomRight">
    /// The bottom right coordinates.
    /// </param>
    /// <param name="zoom">
    /// The zoom.
    /// </param>
    public override void Update(Vector2 topLeft, Vector2 bottomRight, int zoom)
    {
        Update(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y, zoom);
    }

    public override void Update(double tlx, double tly, double brx, double bry, int zoom)
    {
        if (!enabled) return;
        if (instance == null) Init(map.transform);  
        if (control == null) control = OnlineMapsControlBase3D.instance;

        if (!range.InRange(zoom)) visible = false;
        else if (checkMapBoundaries)
        {
            if (latitude > tly || latitude < bry) visible = false;
            else if (tlx < brx && (longitude < tlx || longitude > brx)) visible = false;
            else if (tlx > brx && longitude < tlx && longitude > brx) visible = false;
            else visible = true;
        }
        else visible = true;

        if (!visible) return;

        if (_prefab != prefab) Reinit(tlx, tly, brx, bry, zoom);

        double mx, my;
        map.projection.CoordinatesToTile(longitude, latitude, zoom, out mx, out my);

        double ttlx, ttly, tbrx, tbry;
        map.projection.CoordinatesToTile(tlx, tly, zoom, out ttlx, out ttly);
        map.projection.CoordinatesToTile(brx, bry, zoom, out tbrx, out tbry);

        int maxX = 1 << zoom;

        Bounds bounds = control.cl.bounds;

        double sx = tbrx - ttlx;
        double mpx = mx - ttlx;
        if (sx < 0) sx += maxX;
        if (checkMapBoundaries)
        {
            if (mpx < 0) mpx += maxX;
        }
        else
        {
            double dx1 = Math.Abs(mpx - ttlx);
            double dx2 = Math.Abs(mpx - tbrx);
            double dx3 = Math.Abs(mpx - tbrx + maxX);
            if (dx1 > dx2 && dx1 > dx3) mpx += maxX;
        }

        double px = mpx / sx;
        double pz = (ttly - my) / (ttly - tbry);

        _relativePosition = new Vector3((float)px, 0, (float)pz);

        OnlineMapsTileSetControl tsControl = OnlineMapsTileSetControl.instance;

        if (tsControl != null)
        {
            px = -map.tilesetSize.x / 2 - (px - 0.5) * map.tilesetSize.x;
            pz = map.tilesetSize.y / 2 + (pz - 0.5) * map.tilesetSize.y;
        }
        else
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            px = center.x - (px - 0.5) * size.x / map.transform.lossyScale.x - map.transform.position.x;
            pz = center.z + (pz - 0.5) * size.z / map.transform.lossyScale.z - map.transform.position.z;
        }

        Vector3 oldPosition = instance.transform.localPosition;
        float y = 0;

        if (altitude.HasValue)
        {
            y = altitude.Value * control.GetBestElevationYScale(tlx, tly, brx, bry);
            if (tsControl != null)
            {
                if (tsControl.elevationBottomMode == OnlineMapsTileSetControl.ElevationBottomMode.minValue) y -= tsControl.elevationMinValue;
                y *= tsControl.elevationScale;
            }
        }
        else if (tsControl != null)
        {
            y = tsControl.GetElevationValue((float)px, (float)pz, tsControl.GetBestElevationYScale(tlx, tly, brx, bry), tlx, tly, brx, bry);
        }

        Vector3 newPosition = new Vector3((float)px, y, (float)pz);

        if (oldPosition != newPosition)
        {
            instance.transform.localPosition = newPosition;
            //if (OnPositionChanged != null) OnPositionChanged(this);
        }
    }

    public void Update(OnlineMaps map, OnlineMapsControlBase3D control, Bounds bounds, double tlx, double tly, double brx, double bry, int zoom, double ttlx, double ttly, double tbrx, double tbry, float bestYScale)
    {
        if (!enabled) return;
        if (instance == null) Init(map.transform);

        if (!range.InRange(zoom)) visible = false;
        else if (checkMapBoundaries)
        {
            if (latitude > tly || latitude < bry) visible = false;
            /*else if (Math.Abs(brx - tlx) < 1)
            {
                brx += 360;
                tbrx += 1 << zoom;
                visible = true;
            }*/
            else if (tlx < brx && (longitude < tlx || longitude > brx)) visible = false;
            else if (tlx > brx && longitude < tlx && longitude > brx) visible = false;
            else visible = true;
        }
        else visible = true;

        if (!visible) return;

        if (_prefab != prefab) Reinit(tlx, tly, brx, bry, zoom);

        double mx, my;
        map.projection.CoordinatesToTile(longitude, latitude, zoom, out mx, out my);

        int maxX = 1 << zoom;

        double sx = tbrx - ttlx;
        double mpx = mx - ttlx;
        if (sx < 0) sx += maxX;
        //else if (sx > maxX) sx -= maxX;

        if (checkMapBoundaries)
        {
            if (mpx < 0) mpx += maxX;
            else if (mpx > maxX) mpx -= maxX;
        }
        else
        {
            double dx1 = Math.Abs(mpx - ttlx);
            double dx2 = Math.Abs(mpx - tbrx);
            double dx3 = Math.Abs(mpx - tbrx + maxX);
            if (dx1 > dx2 && dx1 > dx3) mpx += maxX;
        }

        double px = mpx / sx;
        double pz = (ttly - my) / (ttly - tbry);

        _relativePosition = new Vector3((float)px, 0, (float)pz);

        OnlineMapsTileSetControl tsControl = control as OnlineMapsTileSetControl;

        if (tsControl != null)
        {
            px = -map.tilesetSize.x / 2 - (px - 0.5) * map.tilesetSize.x;
            pz = map.tilesetSize.y / 2 + (pz - 0.5) * map.tilesetSize.y;
        }
        else
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            px = center.x - (px - 0.5) * size.x / map.transform.lossyScale.x - map.transform.position.x;
            pz = center.z + (pz - 0.5) * size.z / map.transform.lossyScale.z - map.transform.position.z;
        }

        Vector3 oldPosition = instance.transform.localPosition;
        float y = 0;

        if (altitude.HasValue)
        {
            y = altitude.Value * bestYScale;
            if (tsControl != null)
            {
                if (tsControl.elevationBottomMode == OnlineMapsTileSetControl.ElevationBottomMode.minValue) y -= tsControl.elevationMinValue;
                y *= tsControl.elevationScale;
            }
        }
        else if (tsControl != null)
        {
            y = tsControl.GetElevationValue((float)px, (float)pz, bestYScale, tlx, tly, brx, bry);
        }

        Vector3 newPosition = new Vector3((float)px, y, (float)pz);

        if (oldPosition != newPosition)
        {
            instance.transform.localPosition = newPosition;
            //if (OnPositionChanged != null) OnPositionChanged(this);
        }
    }
}
