/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class control the map for the Tileset.
/// Tileset - a dynamic mesh, created at runtime.
/// </summary>
[Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Controls/Tileset")]
public class OnlineMapsTileSetControl : OnlineMapsControlBase3D
{
    /// <summary>
    /// The event, which occurs when the changed texture tile maps.
    /// </summary>
    public Action<OnlineMapsTile, Material> OnChangeMaterialTexture;

    /// <summary>
    /// Event to manually control the visibility of 2D markers.
    /// </summary>
    public Predicate<OnlineMapsMarker> OnCheckMarker2DVisibility;

    /// <summary>
    /// The event that occurs after draw the tile.
    /// </summary>
    public Action<OnlineMapsTile, Material> OnDrawTile;

    /// <summary>
    /// Event, which intercepts the request to BingMaps Elevation API.
    /// </summary>
    public Action<Vector2, Vector2> OnGetElevation;

    /// <summary>
    /// This event is called when a new elevation value received.
    /// </summary>
    public Action OnElevationUpdated;

    /// <summary>
    /// Event to manually control the order of 2D markers.
    /// </summary>
    public Func<OnlineMapsMarker, float> OnGetFlatMarkerOffsetY;

    /// <summary>
    /// Event that occurs after the map mesh has been updated.
    /// </summary>
    public Action OnMeshUpdated;

    /// <summary>
    /// Event, which occurs when the smooth zoom is started.
    /// </summary>
    public Action OnSmoothZoomBegin;

    /// <summary>
    /// Event, which occurs when the smooth zoom is finish.
    /// </summary>
    public Action OnSmoothZoomFinish;

    /// <summary>
    /// Event, which occurs when the smooth zoom is starts init.
    /// </summary>
    public Action OnSmoothZoomInit;

    /// <summary>
    /// Event, which occurs when the smooth zoom is process.
    /// </summary>
    public Action OnSmoothZoomProcess;

    /// <summary>
    /// Bing Maps API key
    /// </summary>
    public string bingAPI = "";

    /// <summary>
    /// Type of checking 2D markers on visibility.
    /// </summary>
    public OnlineMapsTilesetCheckMarker2DVisibility checkMarker2DVisibility = OnlineMapsTilesetCheckMarker2DVisibility.pivot;

    /// <summary>
    /// Type of collider: box - for performance, mesh - for elevation.
    /// </summary>
    public OnlineMapsColliderType colliderType = OnlineMapsColliderType.fullMesh;

    /// <summary>
    /// Container for drawing elements.
    /// </summary>
    public GameObject drawingsGameObject;

    /// <summary>
    /// Drawing API mode (meshes or overlay).
    /// </summary>
    public OnlineMapsTilesetDrawingMode drawingMode = OnlineMapsTilesetDrawingMode.meshes;

    /// <summary>
    /// Shader of drawing elements.
    /// </summary>
    public Shader drawingShader;

    /// <summary>
    /// Zoom levels, which will be shown the elevations.
    /// </summary>
    public OnlineMapsRange elevationZoomRange = new OnlineMapsRange(11, OnlineMaps.MAXZOOM);

    /// <summary>
    /// Scale of elevation data.
    /// </summary>
    public float elevationScale = 1;

    /// <summary>
    /// Type calculation of the bottom point of the map mesh.
    /// </summary>
    public ElevationBottomMode elevationBottomMode = ElevationBottomMode.zero;

    /// <summary>
    /// Resolution of the elevation map.
    /// </summary>
    public int elevationResolution = 32;

    /// <summary>
    /// The minimum elevation value.
    /// </summary>
    public short elevationMinValue;

    /// <summary>
    /// The maximum elevation value.
    /// </summary>
    public short elevationMaxValue;

    /// <summary>
    /// Specifies whether to lock yScale.\n
    /// If TRUE, then GetBestElevationYScale always returns yScaleValue.
    /// </summary>
    public bool lockYScale = false;

    /// <summary>
    /// IComparer instance for manual sorting of markers.
    /// </summary>
    public IComparer<OnlineMapsMarker> markerComparer;

    /// <summary>
    /// Material that will be used for marker.
    /// </summary>
    public Material markerMaterial;

    /// <summary>
    /// Shader of markers.
    /// </summary>
    public Shader markerShader;

    /// <summary>
    /// Specifies whether to use a smooth touch zoom.
    /// </summary>
    public bool smoothZoom = false;

    /// <summary>
    /// The minimum scale at smooth zoom.
    /// </summary>
    public float smoothZoomMinScale = float.MinValue;

    /// <summary>
    /// The maximum scale at smooth zoom.
    /// </summary>
    public float smoothZoomMaxScale = float.MaxValue;

    /// <summary>
    /// Indicates smooth zoom in process.
    /// </summary>
    public bool smoothZoomStarted = false;

    /// <summary>
    /// Material that will be used for tile.
    /// </summary>
    public Material tileMaterial;

    /// <summary>
    /// Shader of map.
    /// </summary>
    public Shader tilesetShader;

    /// <summary>
    /// Specifies that you want to build a map with the elevetions.
    /// </summary>
    public bool useElevation = false;

    /// <summary>
    /// GetBestElevationYScale returns this value when lockYScale=true.
    /// </summary>
    public float yScaleValue;

    private bool _useElevation;

    private OnlineMapsVector2i _bufferPosition;

    [NonSerialized]
    private OnlineMapsBingMapsElevation elevationRequest;
    private float elevationRequestX1;
    private float elevationRequestY1;
    private float elevationRequestX2;
    private float elevationRequestY2;
    private float elevationRequestW;
    private float elevationRequestH;
    //private Rect elevationRequestRect;
    private short[,] elevationData;
    //private Rect elevationRect;
    private float elevationX1;
    private float elevationY1;
    private float elevationW;
    private float elevationH;
    private MeshCollider meshCollider;
    private bool ignoreGetElevation;
    private Mesh tilesetMesh;
    private int[] triangles;
    private Vector2[] uv;
    private Vector3[] vertices;

    private OnlineMapsVector2i elevationBufferPosition;

    private Vector2 smoothZoomPoint;
    private Vector3 smoothZoomOffset;
    private Vector3 smoothZoomHitPoint;
    private bool firstUpdate = true;
    private List<TilesetFlatMarker> usedMarkers;
    private Color32[] overlayFrontBuffer;
    private bool colliderWithElevation;
    private BoxCollider boxCollider;
    private int elevationDataWidth;
    private int elevationDataHeight;
    private bool waitSetElevationData;
    private int waitMapZoom;
    private bool needRestoreGestureZoom;
    private List<Vector3> markersVertices;

    /// <summary>
    /// Singleton instance of OnlineMapsTileSetControl control.
    /// </summary>
    public new static OnlineMapsTileSetControl instance
    {
        get { return OnlineMapsControlBase.instance as OnlineMapsTileSetControl; }
    }

    private OnlineMapsVector2i bufferPosition
    {
        get
        {
            if (map.buffer.bufferPosition != null) return map.buffer.bufferPosition;

            if (_bufferPosition == null)
            {
                const int s = OnlineMapsUtils.tileSize;
                int countX = map.width / s + 2;
                int countY = map.height / s + 2;

                double px, py;
                map.GetTilePosition(out px, out py);
                _bufferPosition = new OnlineMapsVector2i((int)px, (int)py);
                _bufferPosition.x -= countX / 2;
                _bufferPosition.y -= countY / 2;

                int maxY = 1 << map.zoom;

                if (_bufferPosition.y < 0) _bufferPosition.y = 0;
                if (_bufferPosition.y >= maxY - countY - 1) _bufferPosition.y = maxY - countY - 1;
            }
            return _bufferPosition;
        }
    }

    /// <summary>
    /// Mode of smooth zoom.
    /// </summary>
    [Obsolete("Use zoomMode.")]
    public OnlineMapsSmoothZoomMode smoothZoomMode
    {
        get { return (OnlineMapsSmoothZoomMode)(int)zoomMode; }
        set { zoomMode = (OnlineMapsZoomMode) (int) value; }
    }

    public override void Clear2DMarkerInstances(OnlineMapsMarker2DMode mode)
    {
        if (marker2DMode == OnlineMapsMarker2DMode.billboard)
        {
            Clear2DMarkerBillboards();
        }
        else
        {
            foreach (GameObject go in markersGameObjects) OnlineMapsUtils.DestroyImmediate(go);
            markersGameObjects = null;
        }
    }

    public override float GetBestElevationYScale(Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        if (lockYScale) return yScaleValue;

        Vector2 realDistance = OnlineMapsUtils.DistanceBetweenPoints(topLeftPosition, bottomRightPosition);
        return Mathf.Min(map.width / realDistance.x, map.height / realDistance.y) / 1000;
    }

    public override float GetBestElevationYScale(double tlx, double tly, double brx, double bry)
    {
        if (lockYScale) return yScaleValue;

        double dx, dy;
        OnlineMapsUtils.DistanceBetweenPoints(tlx, tly, brx, bry, out dx, out dy);
        dx = dx / map.tilesetSize.x * 1024;
        dy = dy / map.tilesetSize.y * 1024;
        return (float)Math.Min(map.width / dx, map.height / dy) / 1000;
    }

    public override Vector2 GetCoords(Vector2 position)
    {
        if (!HitTest(position)) return Vector2.zero;

        RaycastHit hit;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance))
            return Vector2.zero;

        return GetCoordsByWorldPosition(hit.point);
    }

    public override bool GetCoords(out double lng, out double lat, Vector2 position)
    {
        lat = 0;
        lng = 0;

        if (!HitTest(position)) return false;

        RaycastHit hit;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance)) return false;

        return GetCoordsByWorldPosition(out lng, out lat, hit.point);
    }

    /// <summary>
    /// Returns the geographical coordinates by world position.
    /// </summary>
    /// <param name="position">World position</param>
    /// <returns>Geographical coordinates or Vector2.zero</returns>
    public Vector2 GetCoordsByWorldPosition(Vector3 position)
    {
        Vector3 boundsSize = new Vector3(map.tilesetSize.x, 0, map.tilesetSize.y);
        boundsSize.Scale(transform.lossyScale);
        Vector3 size = new Vector3(0, 0, map.tilesetSize.y * transform.lossyScale.z) - Quaternion.Inverse(transform.rotation) * (position - transform.position);

        size.x = size.x / boundsSize.x;
        size.z = size.z / boundsSize.z;

        Vector2 r = new Vector3(size.x - .5f, size.z - .5f);

        int countX = map.width / OnlineMapsUtils.tileSize;
        int countY = map.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetTilePosition(out px, out py);
        px += countX * r.x;
        py -= countY * r.y;
        map.projection.TileToCoordinates(px, py, map.zoom, out px, out py);
        return new Vector2((float) px, (float) py);
    }

    /// <summary>
    /// Returns the geographical coordinates by world position.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <param name="position">World position</param>
    /// <returns>True - success, False - otherwise.</returns>
    public bool GetCoordsByWorldPosition(out double lng, out double lat, Vector3 position)
    {
        Vector3 boundsSize = new Vector3(map.tilesetSize.x, 0, map.tilesetSize.y);
        boundsSize.Scale(transform.lossyScale);
        Vector3 size = new Vector3(0, 0, map.tilesetSize.y * transform.lossyScale.z) - Quaternion.Inverse(transform.rotation) * (position - transform.position);

        size.x = size.x / boundsSize.x;
        size.z = size.z / boundsSize.z;

        Vector2 r = new Vector3(size.x - .5f, size.z - .5f);

        int countX = map.width / OnlineMapsUtils.tileSize;
        int countY = map.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetTilePosition(out px, out py);
        px += countX * r.x;
        py -= countY * r.y;
        map.projection.TileToCoordinates(px, py, map.zoom, out lng, out lat);
        return true;
    }

    private void GetElevation()
    {
        ignoreGetElevation = true;

        if (elevationRequest != null || waitSetElevationData) return;

        elevationBufferPosition = bufferPosition;
        ignoreGetElevation = false;

        const int s = OnlineMapsUtils.tileSize;
        int countX = map.width / s + 2;
        int countY = map.height / s + 2;

        double sx, sy, ex, ey;
        map.projection.TileToCoordinates(bufferPosition.x, bufferPosition.y, map.zoom, out sx, out sy);
        map.projection.TileToCoordinates(bufferPosition.x + countX, bufferPosition.y + countY, map.zoom, out ex, out ey);

        elevationRequestX1 = (float) sx;
        elevationRequestY1 = (float) sy;
        elevationRequestX2 = (float) ex;
        elevationRequestY2 = (float) ey;
        elevationRequestW = elevationRequestX2 - elevationRequestX1;
        elevationRequestH = elevationRequestY2 - elevationRequestY1;

        if (OnGetElevation == null)
        {
            StartDownloadElevation(sx, sy, ex, ey);
        }
        else
        {
            waitSetElevationData = true;
            OnGetElevation(new Vector2((float)sx, (float)sy), new Vector2((float)ex, (float)ey));
        }
    }

    public override float GetElevationValue(double x, double z, float yScale, Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        return GetElevationValue(x, z, yScale, topLeftPosition.x, topLeftPosition.y, bottomRightPosition.x, bottomRightPosition.y);
    }

    public override float GetElevationValue(double x, double z, float yScale, double tlx, double tly, double brx, double bry)
    {
        if (elevationData == null) return 0;

        x = x / -map.tilesetSize.x;
        z = z / map.tilesetSize.y;

        int ew = elevationDataWidth - 1;
        int eh = elevationDataHeight - 1;

        if (x < 0) x = 0;
        else if (x > 1) x = 1;

        if (z < 0) z = 0;
        else if (z > 1) z = 1;

        double cx = (brx - tlx) * x + tlx;
        double cz = (bry - tly) * z + tly;

        float rx = (float)((cx - elevationX1) / elevationW * ew);
        float ry = (float)((cz - elevationY1) / elevationH * eh);

        if (rx < 0) rx = 0;
        else if (rx > ew) rx = ew;

        if (ry < 0) ry = 0;
        else if (ry > eh) ry = eh;

        int x1 = (int)rx;
        int x2 = x1 + 1;
        int y1 = (int)ry;
        int y2 = y1 + 1;
        if (x2 > ew) x2 = ew;
        if (y2 > eh) y2 = eh;

        float p1 = (elevationData[x2, eh - y1] - elevationData[x1, eh - y1]) * (rx - x1) + elevationData[x1, eh - y1];
        float p2 = (elevationData[x2, eh - y2] - elevationData[x1, eh - y2]) * (rx - x1) + elevationData[x1, eh - y2];

        float v = (p2 - p1) * (ry - y1) + p1;
        if (elevationBottomMode == ElevationBottomMode.minValue) v -= elevationMinValue;
        return v * yScale * elevationScale;
    }

    /// <summary>
    /// Returns the maximum elevation for the current map.
    /// </summary>
    /// <param name="yScale">Best yScale.</param>
    /// <returns>Maximum elevation value.</returns>
    public float GetMaxElevationValue(float yScale)
    {
        return elevationData == null ? 0 : elevationMaxValue * yScale * elevationScale;
    }

    /// <summary>
    /// Gets flat marker by screen position.
    /// </summary>
    /// <param name="screenPosition">Screen position.</param>
    /// <returns>Instance of marker.</returns>
    public OnlineMapsMarker GetMarkerFromScreen(Vector2 screenPosition)
    {
        if (usedMarkers == null || usedMarkers.Count == 0) return null;

        OnlineMapsMarker marker = null;

        RaycastHit hit;
        if (cl.Raycast(activeCamera.ScreenPointToRay(screenPosition), out hit, OnlineMapsUtils.maxRaycastDistance))
        {
            double lng = double.MinValue, lat = double.MaxValue;
            foreach (TilesetFlatMarker flatMarker in usedMarkers)
            {
                if (flatMarker.Contains(hit.point, transform))
                {
                    double mx, my;
                    flatMarker.marker.GetPosition(out mx, out my);
                    if (my < lat || (Math.Abs(my - lat) < double.Epsilon && mx > lng)) marker = flatMarker.marker;
                }
            }
        }
        return marker;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPosition(Vector2 coords)
    {
        Vector2 mapPosition = OnlineMapsControlBase.instance.GetPosition(coords);

        float px = -mapPosition.x / map.tilesetWidth * map.tilesetSize.x;
        float pz = mapPosition.y / map.tilesetHeight * map.tilesetSize.y;

        Vector3 offset = transform.rotation * new Vector3(px, 0, pz);
        offset.Scale(map.transform.lossyScale);

        return map.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <returns></returns>
    public Vector3 GetWorldPosition(double lng, double lat)
    {
        double mx, my;
        GetPosition(lng, lat, out mx, out my);

        double px = -mx / map.tilesetWidth * map.tilesetSize.x;
        double pz = my / map.tilesetHeight * map.tilesetSize.y;

        Vector3 offset = transform.rotation * new Vector3((float)px, 0, (float)pz);
        offset.Scale(map.transform.lossyScale);

        return map.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <param name="topLeftPosition">Coordinates of top-left corner of map.</param>
    /// <param name="bottomRightPosition">Coordinates of bottom-right corner of map.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(Vector2 coords, Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        Vector2 mapPosition = OnlineMapsControlBase.instance.GetPosition(coords);

        float px = -mapPosition.x / map.tilesetWidth * map.tilesetSize.x;
        float pz = mapPosition.y / map.tilesetHeight * map.tilesetSize.y;

        float y = GetElevationValue(-mapPosition.x, mapPosition.y, GetBestElevationYScale(topLeftPosition, bottomRightPosition), topLeftPosition, bottomRightPosition);

        Vector3 offset = transform.rotation * new Vector3(px, y, pz);
        offset.Scale(map.transform.lossyScale);

        return map.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <param name="tlx">Top-left longitude.</param>
    /// <param name="tly">Top-left latitude.</param>
    /// <param name="brx">Bottom-right longitude.</param>
    /// <param name="bry">Bottom-right latitude.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(Vector2 coords, double tlx, double tly, double brx, double bry)
    {
        Vector2 mapPosition = GetPosition(coords);

        float px = -mapPosition.x / map.tilesetWidth * map.tilesetSize.x;
        float pz = mapPosition.y / map.tilesetHeight * map.tilesetSize.y;

        float y = GetElevationValue(-mapPosition.x, mapPosition.y, GetBestElevationYScale(tlx, tly, brx, bry), tlx, tly, brx, bry);

        Vector3 offset = transform.rotation * new Vector3(px, y, pz);
        offset.Scale(map.transform.lossyScale);

        return map.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Laatitude</param>
    /// <param name="tlx">Top-left longitude.</param>
    /// <param name="tly">Top-left latitude.</param>
    /// <param name="brx">Bottom-right longitude.</param>
    /// <param name="bry">Bottom-right latitude.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(double lng, double lat, double tlx, double tly, double brx, double bry)
    {
        double mx, my;
        GetPosition(lng, lat, out mx, out my);

        double px = -mx / map.tilesetWidth * map.tilesetSize.x;
        double pz = my / map.tilesetHeight * map.tilesetSize.y;

        float y = GetElevationValue(px, pz, GetBestElevationYScale(tlx, tly, brx, bry), tlx, tly, brx, bry);

        Vector3 offset = transform.rotation * new Vector3((float)px, y, (float)pz);
        offset.Scale(map.transform.lossyScale);

        return map.transform.position + offset;
    }

    protected override bool HitTest()
    {
#if NGUI
        if (UICamera.Raycast(GetInputPosition())) return false;
#endif
        RaycastHit hit;
        return cl.Raycast(activeCamera.ScreenPointToRay(GetInputPosition()), out hit, OnlineMapsUtils.maxRaycastDistance);
    }

    protected override bool HitTest(Vector2 position)
    {
#if NGUI
        if (UICamera.Raycast(position)) return false;
#endif
        RaycastHit hit;
        return cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance);
    }

    private void InitDrawingsMesh()
    {
        drawingsGameObject = new GameObject("Drawings");
        drawingsGameObject.transform.parent = transform;
        drawingsGameObject.transform.localPosition = new Vector3(0, OnlineMaps.instance.tilesetSize.magnitude / 4344, 0);
        drawingsGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        drawingsGameObject.transform.localScale = Vector3.one;
        drawingsGameObject.layer = gameObject.layer;
    }

    private void InitMapMesh()
    {
        _useElevation = useElevation;

        Shader tileShader = tilesetShader;

        MeshFilter meshFilter;
        boxCollider = null;

        if (tilesetMesh == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();

            if (colliderType == OnlineMapsColliderType.fullMesh || colliderType == OnlineMapsColliderType.simpleMesh) meshCollider = gameObject.AddComponent<MeshCollider>();
            else if (colliderType == OnlineMapsColliderType.box || colliderType == OnlineMapsColliderType.flatBox) boxCollider = gameObject.AddComponent<BoxCollider>();

            tilesetMesh = new Mesh {name = "Tileset"};
        }
        else
        {
            meshFilter = GetComponent<MeshFilter>();
            tilesetMesh.Clear();
            elevationData = null;
            elevationRequest = null;
            if (useElevation) ignoreGetElevation = false;
        }

        int w1 = map.tilesetWidth / OnlineMapsUtils.tileSize;
        int h1 = map.tilesetHeight / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < elevationResolution) subMeshVX = elevationResolution % w1 == 0 ? elevationResolution / w1 : elevationResolution / w1 + 1;
            if (h1 < elevationResolution) subMeshVZ = elevationResolution % h1 == 0 ? elevationResolution / h1 : elevationResolution / h1 + 1;
        }

        Vector2 subMeshSize = new Vector2(map.tilesetSize.x / w1, map.tilesetSize.y / h1);

        int w = w1 + 2;
        int h = h1 + 2;

        int countVertices = w * h * (subMeshVX + 1) * (subMeshVZ + 1);
        vertices = new Vector3[countVertices];
        uv = new Vector2[countVertices];
        Vector3[] normals = new Vector3[countVertices];
        Material[] materials = new Material[w * h];
        tilesetMesh.subMeshCount = w * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                InitMapSubMesh(ref normals, x, y, w, h, subMeshSize, subMeshVX, subMeshVZ);
            }
        }

        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;
        tilesetMesh.normals = normals;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                InitMapSubMeshTriangles(ref materials, x, y, w, h, subMeshVX, subMeshVZ, tileShader);
            }
        }

        triangles = null;

        gameObject.GetComponent<Renderer>().materials = materials;

        tilesetMesh.MarkDynamic();
        tilesetMesh.RecalculateBounds();
        meshFilter.sharedMesh = tilesetMesh;

        if (colliderType == OnlineMapsColliderType.fullMesh) meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
        else if (colliderType == OnlineMapsColliderType.simpleMesh)
        {
            InitSimpleMeshCollider();
            meshCollider.sharedMesh = meshCollider.sharedMesh;
        }
        else if (boxCollider != null)
        {
            boxCollider.center = new Vector3(-map.tilesetSize.x / 2, 0, map.tilesetSize.y / 2);
            boxCollider.size = new Vector3(map.tilesetSize.x, 0, map.tilesetSize.y);
        }

        UpdateMapMesh();
    }

    private void InitMapSubMesh(ref Vector3[] normals, int x, int y, int w, int h, Vector2 subMeshSize, int subMeshVX, int subMeshVZ)
    {
        int i = (x + y * w) * (subMeshVX + 1) * (subMeshVZ + 1);

        Vector2 cellSize = new Vector2(subMeshSize.x / subMeshVX, subMeshSize.y / subMeshVZ);

        float sx = x > 0 && x < w - 1 ? cellSize.x : 0;
        float sy = y > 0 && y < h - 1 ? cellSize.y : 0;

        float nextY = subMeshSize.y * (y - 1);

        float uvX = 1f / subMeshVX;
        float uvZ = 1f / subMeshVZ;

        for (int ty = 0; ty <= subMeshVZ; ty++)
        {
            float nextX = -subMeshSize.x * (x - 1);
            float uvy = 1 - uvZ * ty;

            for (int tx = 0; tx <= subMeshVX; tx++)
            {
                float uvx = 1 - uvX * tx;

                vertices[i] = new Vector3(nextX, 0, nextY);
                uv[i] = new Vector2(uvx, uvy);
                normals[i++] = new Vector3(0.0f, 1f, 0.0f);
                
                nextX -= sx;
            }

            nextY += sy;
        }
    }

    private void InitMapSubMeshTriangles(ref Material[] materials, int x, int y, int w, int h, int subMeshVX, int subMeshVZ, Shader tileShader)
    {
        if (triangles == null) triangles = new int[subMeshVX * subMeshVZ * 6];
        int i = (x + y * w) * (subMeshVX + 1) * (subMeshVZ + 1);

        for (int ty = 0; ty < subMeshVZ; ty++)
        {
            int cy = ty * subMeshVX * 6;
            int py1 = i + ty * (subMeshVX + 1);
            int py2 = i + (ty + 1) * (subMeshVX + 1);

            for (int tx = 0; tx < subMeshVX; tx++)
            {
                int ti = tx * 6 + cy;
                int p1 = py1 + tx;
                int p2 = p1 + 1;
                int p3 = py2 + tx;
                int p4 = p3 + 1;

                triangles[ti] = p1;
                triangles[ti + 1] = p2;
                triangles[ti + 2] = p4;
                triangles[ti + 3] = p1;
                triangles[ti + 4] = p4;
                triangles[ti + 5] = p3;
            }
        }

        tilesetMesh.SetTriangles(triangles, x + y * w);
        Material material;

        if (tileMaterial != null) material = Instantiate(tileMaterial) as Material;
        else material = new Material(tileShader);

        if (map.defaultTileTexture != null) material.mainTexture = map.defaultTileTexture;
        materials[x + y * w] = material;
    }

    private void InitSimpleMeshCollider()
    {
        Mesh simpleMesh = new Mesh();
        simpleMesh.MarkDynamic();

        int res = useElevation && elevationZoomRange.InRange(map.zoom) ? 6 : 1;
        int r2 = res + 1;
        Vector3[] vertices = new Vector3[r2 * r2];
        int[] triangles = new int[res * res * 6];

        float sx = -map.tilesetSize.x / res;
        float sy = map.tilesetSize.y / res;

        int ti = 0;

        for (int y = 0; y < r2; y++)
        {
            for (int x = 0; x < r2; x++)
            {
                vertices[y * r2 + x] = new Vector3(sx * x, 0, sy * y);

                if (x != 0 && y != 0)
                {
                    int p4 = y * r2 + x;
                    int p3 = p4 - 1;
                    int p2 = p4 - r2;
                    int p1 = p2 - 1;

                    triangles[ti++] = p1;
                    triangles[ti++] = p2;
                    triangles[ti++] = p4;
                    triangles[ti++] = p1;
                    triangles[ti++] = p4;
                    triangles[ti++] = p3;
                }
            }
        }

        simpleMesh.vertices = vertices;
        simpleMesh.SetTriangles(triangles, 0);
        simpleMesh.RecalculateBounds();

        meshCollider.sharedMesh = simpleMesh;
    }

    public override void OnAwakeBefore()
    {
        base.OnAwakeBefore();

        map = GetComponent<OnlineMaps>();

        InitMapMesh();
        if (useElevation) GetElevation();
    }

    protected override void OnDestroyLate()
    {
        base.OnDestroyLate();

        OnElevationUpdated = null;
        OnSmoothZoomBegin = null;
        OnSmoothZoomFinish = null;
        OnSmoothZoomProcess = null;

        if (drawingsGameObject != null) OnlineMapsUtils.DestroyImmediate(drawingsGameObject);
        drawingsGameObject = null;
        elevationData = null;
        elevationRequest = null;
        meshCollider = null;
        tilesetMesh = null;
        triangles = null;
        uv = null;
        vertices = null;
    }

    private void OnElevationRequestComplete(string response)
    {
        const int elevationDataResolution = 32;

        try
        {
            bool isFirstResponse = false;
            if (elevationData == null)
            {
                elevationData = new short[elevationDataResolution, elevationDataResolution];
                isFirstResponse = true;
            }
            Array ed = elevationData;

            if (OnlineMapsBingMapsElevation.ParseElevationArray(response, OnlineMapsBingMapsElevation.Output.json, ref ed))
            {
                elevationX1 = elevationRequestX1;
                elevationY1 = elevationRequestY1;
                elevationW = elevationRequestW;
                elevationH = elevationRequestH;
                elevationDataWidth = elevationDataResolution;
                elevationDataHeight = elevationDataResolution;

                UpdateElevationMinMax();
                if (OnElevationUpdated != null) OnElevationUpdated();

                UpdateControl();
                ignoreGetElevation = false;
            }
            else
            {
                if (isFirstResponse)
                {
                    elevationX1 = elevationRequestX1;
                    elevationY1 = elevationRequestY1;
                    elevationW = elevationRequestW;
                    elevationH = elevationRequestH;
                    elevationDataWidth = elevationDataResolution;
                    elevationDataHeight = elevationDataResolution;
                }
                Debug.LogWarning(response);
            }
            elevationRequest = null;

            if (ignoreGetElevation) GetElevation();
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }

    private void ReinitMapMesh(int w, int h, int subMeshVX, int subMeshVZ)
    {
        Material[] materials = rendererInstance.materials;

        vertices = new Vector3[w * h * (subMeshVX + 1) * (subMeshVZ + 1)];
        uv = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        Array.Resize(ref materials, w * h);

        for (int i = 0; i < normals.Length; i++) normals[i] = new Vector3(0, 1, 0);
        tilesetMesh.Clear();
        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;
        tilesetMesh.normals = normals;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null) continue;

            if (tileMaterial != null) materials[i] = Instantiate(tileMaterial) as Material;
            else materials[i] = new Material(tilesetShader);

            if (map.defaultTileTexture != null) materials[i].mainTexture = map.defaultTileTexture;
        }

        tilesetMesh.subMeshCount = w * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (triangles == null) triangles = new int[subMeshVX * subMeshVZ * 6];
                int i = (x + y * w) * (subMeshVX + 1) * (subMeshVZ + 1);

                for (int ty = 0; ty < subMeshVZ; ty++)
                {
                    int cy = ty * subMeshVX * 6;
                    int py1 = i + ty * (subMeshVX + 1);
                    int py2 = i + (ty + 1) * (subMeshVX + 1);

                    for (int tx = 0; tx < subMeshVX; tx++)
                    {
                        int ti = tx * 6 + cy;
                        int p1 = py1 + tx;
                        int p2 = py1 + tx + 1;
                        int p3 = py2 + tx;
                        int p4 = py2 + tx + 1;

                        triangles[ti] = p1;
                        triangles[ti + 1] = p2;
                        triangles[ti + 2] = p4;
                        triangles[ti + 3] = p1;
                        triangles[ti + 4] = p4;
                        triangles[ti + 5] = p3;
                    }
                }

                tilesetMesh.SetTriangles(triangles, x + y * w);
            }
        }

        triangles = null;
        rendererInstance.materials = materials;
        firstUpdate = true;
    }

    /// <summary>
    /// Resize map
    /// </summary>
    /// <param name="width">Width (pixels)</param>
    /// <param name="height">Height (pixels)</param>
    /// <param name="changeSizeInScene">Change the size of the map in the scene or leave the same.</param>
    public void Resize(int width, int height, bool changeSizeInScene = true)
    {
        Resize(width, height, changeSizeInScene? new Vector2(width, height) : map.tilesetSize);
    }

    /// <summary>
    /// Resize map
    /// </summary>
    /// <param name="width">Width (pixels)</param>
    /// <param name="height">Height (pixels)</param>
    /// <param name="sizeX">Size X (in scene)</param>
    /// <param name="sizeZ">Size Z (in scene)</param>
    public void Resize(int width, int height, float sizeX, float sizeZ)
    {
        Resize(width, height, new Vector2(sizeX, sizeZ));
    }

    /// <summary>
    /// Resize map
    /// </summary>
    /// <param name="width">Width (pixels)</param>
    /// <param name="height">Height (pixels)</param>
    /// <param name="sizeInScene">Size in scene (X-X, Y-Z)</param>
    public void Resize(int width, int height, Vector2 sizeInScene)
    {
        map.width = map.tilesetWidth = width;
        map.height = map.tilesetHeight = height;
        map.tilesetSize = sizeInScene;

        int w1 = width / OnlineMapsUtils.tileSize;
        int h1 = height / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < elevationResolution) subMeshVX = elevationResolution % w1 == 0 ? elevationResolution / w1 : elevationResolution / w1 + 1;
            if (h1 < elevationResolution) subMeshVZ = elevationResolution % h1 == 0 ? elevationResolution / h1 : elevationResolution / h1 + 1;
        }

        int w = w1 + 2;
        int h = h1 + 2;

        _bufferPosition = null;

        ReinitMapMesh(w, h, subMeshVX, subMeshVZ);

        map.UpdateBorders();
        map.Redraw();
    }

    private void RestoreGestureZoom()
    {
        RestoreGestureZoom(false);
    }

    private void RestoreGestureZoom(bool forceRestore)
    {
        if (!forceRestore && map.buffer.apiZoom != waitMapZoom) return;

        map.OnMapUpdated -= RestoreGestureZoom;

        transform.position = originalPosition;
        transform.localScale = originalScale;

        if (allowCameraControl) UpdateCameraPosition();
        UpdateControl();

        if (OnSmoothZoomFinish != null) OnSmoothZoomFinish();
        needRestoreGestureZoom = false;
    }

    public override OnlineMapsXML SaveSettings(OnlineMapsXML parent)
    {
        OnlineMapsXML element = base.SaveSettings(parent);
        element.Create("CheckMarker2DVisibility", (int) checkMarker2DVisibility);
        element.Create("SmoothZoom", smoothZoom);
        element.Create("UseElevation", useElevation);
        element.Create("TileMaterial", tileMaterial);
        element.Create("TileShader", tilesetShader);
        element.Create("DrawingShader", drawingShader);
        element.Create("MarkerMaterial", markerMaterial);
        element.Create("MarkerShader", markerShader);
        return element;
    }

    /// <summary>
    /// Allows you to set the current values ​​of elevation.
    /// </summary>
    /// <param name="data">Elevation data [32x32]</param>
    public void SetElevationData(short[,] data)
    {
        elevationData = data;
        elevationX1 = elevationRequestX1;
        elevationY1 = elevationRequestY1;
        elevationW = elevationRequestW;
        elevationH = elevationRequestH;
        elevationDataWidth = data.GetLength(0);
        elevationDataHeight = data.GetLength(1);

        UpdateElevationMinMax();

        waitSetElevationData = false;

        if (OnElevationUpdated != null) OnElevationUpdated();
        UpdateControl();
    }

    private void SetMarkersMesh(int usedMarkersCount, List<Texture> usedTextures, List<List<int>> usedTexturesMarkerIndex, int meshIndex)
    {
        Vector2[] markersUV = new Vector2[markersVertices.Count];
        Vector3[] markersNormals = new Vector3[markersVertices.Count];

        Vector2 uvp1 = new Vector2(1, 1);
        Vector2 uvp2 = new Vector2(0, 1);
        Vector2 uvp3 = new Vector2(0, 0);
        Vector2 uvp4 = new Vector2(1, 0);

        for (int i = 0; i < usedMarkersCount; i++)
        {
            int vi = i * 4;
            markersNormals[vi] = Vector3.up;
            markersNormals[vi + 1] = Vector3.up;
            markersNormals[vi + 2] = Vector3.up;
            markersNormals[vi + 3] = Vector3.up;

            markersUV[vi] = uvp2;
            markersUV[vi + 1] = uvp1;
            markersUV[vi + 2] = uvp4;
            markersUV[vi + 3] = uvp3;
        }

        Mesh markersMesh = markersMeshes.Count > meshIndex? markersMeshes[meshIndex]: null;
        if (markersMesh == null) markersMesh = InitMarkersMesh(meshIndex);

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
        markersMesh.vertices = markersVertices.ToArray();
#else
        markersMesh.SetVertices(markersVertices);
#endif

        markersMesh.uv = markersUV;
        markersMesh.normals = markersNormals;

        Renderer markersRenderer = markersRenderers[meshIndex];

        if (markersRenderer.materials.Length != usedTextures.Count) markersRenderer.materials = new Material[usedTextures.Count];

        markersMesh.subMeshCount = usedTextures.Count;

        for (int i = 0; i < usedTextures.Count; i++)
        {
            int markerCount = usedTexturesMarkerIndex[i].Count;
            int[] markersTriangles = new int[markerCount * 6];

            for (int j = 0; j < markerCount; j++)
            {
                int vi = usedTexturesMarkerIndex[i][j] * 4;
                int vj = j * 6;

                markersTriangles[vj + 0] = vi;
                markersTriangles[vj + 1] = vi + 1;
                markersTriangles[vj + 2] = vi + 2;
                markersTriangles[vj + 3] = vi;
                markersTriangles[vj + 4] = vi + 2;
                markersTriangles[vj + 5] = vi + 3;
            }

            markersMesh.SetTriangles(markersTriangles, i);

            Material material = markersRenderer.materials[i];
            if (material == null)
            {
                if (markerMaterial != null) material = markersRenderer.materials[i] = new Material(markerMaterial);
                else material = markersRenderer.materials[i] = new Material(markerShader);
            }

            if (material.mainTexture != usedTextures[i])
            {
                if (markerMaterial != null)
                {
                    material.shader = markerMaterial.shader;
                    material.CopyPropertiesFromMaterial(markerMaterial);
                }
                else
                {
                    material.shader = markerShader;
                    material.color = Color.white;
                }
                material.SetTexture("_MainTex", usedTextures[i]);
            }
        }
    }

    public void StartDownloadElevation(double sx, double sy, double ex, double ey)
    {
        elevationRequest = OnlineMapsBingMapsElevation.GetElevationByBounds(bingAPI, sx, sy, ex, ey, 32, 32);
        elevationRequest.OnComplete += OnElevationRequestComplete;
    }

    public override void UpdateControl()
    {
        base.UpdateControl();

        _bufferPosition = null;

        if (OnlineMapsTile.tiles == null) return;

        if (useElevation != _useElevation)
        {
            elevationBufferPosition = null;
            triangles = null;
            InitMapMesh();
        }
        UpdateMapMesh();

        if (map.drawingElements.Count > 0)
        {
            if (drawingMode == OnlineMapsTilesetDrawingMode.meshes)
            {
                if (drawingsGameObject == null) InitDrawingsMesh();
                int index = 0;
                foreach (OnlineMapsDrawingElement drawingElement in map.drawingElements)
                {
                    drawingElement.DrawOnTileset(this, index++);
                }
            }
        }

        if (marker2DMode == OnlineMapsMarker2DMode.flat) UpdateMarkersMesh();
    }

    private void UpdateElevationMinMax()
    {
        elevationMinValue = short.MaxValue;
        elevationMaxValue = short.MinValue;

        if (elevationData == null) return;

        int s1 = elevationData.GetLength(0);
        int s2 = elevationData.GetLength(1);

        for (int i = 0; i < s1; i++)
        {
            for (int j = 0; j < s2; j++)
            {
                short v = elevationData[i, j];
                if (v < elevationMinValue) elevationMinValue = v;
                if (v > elevationMaxValue) elevationMaxValue = v;
            }
        }
    }

    protected override void UpdateGestureZoom()
    {
        if (!smoothZoom)
        {
            base.UpdateGestureZoom();
            return;
        }

        if (!allowUserControl) return;

        if (Input.touchCount == 2)
        {
            Vector2 p1 = Input.GetTouch(0).position;
            Vector2 p2 = Input.GetTouch(1).position;
            float distance = (p1 - p2).magnitude;

            Vector2 center = Vector2.Lerp(p1, p2, 0.5f);

            if (!smoothZoomStarted)
            {
                if (needRestoreGestureZoom) RestoreGestureZoom(true);

                if (OnSmoothZoomInit != null) OnSmoothZoomInit();

                smoothZoomPoint = center;
                lockClick = true;

                RaycastHit hit;
                if (!cl.Raycast(activeCamera.ScreenPointToRay(center), out hit, OnlineMapsUtils.maxRaycastDistance)) return;
                
                if (zoomMode == OnlineMapsZoomMode.target)
                {
                    smoothZoomHitPoint = hit.point;
                }
                else
                {
                    smoothZoomHitPoint = transform.position + transform.rotation * new Vector3(map.tilesetSize.x / -2 * transform.lossyScale.x, 0, map.tilesetSize.y / 2 * transform.lossyScale.z);
                }

                originalPosition = transform.position;
                originalScale = transform.localScale;
                smoothZoomOffset = Quaternion.Inverse(transform.rotation) * (originalPosition - smoothZoomHitPoint);
                smoothZoomOffset.Scale(new Vector3(-1f / map.tilesetWidth, 0, -1f / map.tilesetHeight));

                smoothZoomStarted = true;
                isMapDrag = false;
                waitZeroTouches = true;

                if (OnSmoothZoomBegin != null) OnSmoothZoomBegin();
            }
            else
            {
                RaycastHit hit;
                if (!cl.Raycast(activeCamera.ScreenPointToRay(center), out hit, OnlineMapsUtils.maxRaycastDistance)) return;

                float scale = 1;

                if (Mathf.Abs(distance - lastGestureDistance) > 2)
                {
                    if (!invertTouchZoom) scale = distance / lastGestureDistance;
                    else scale = lastGestureDistance / distance;
                }

                transform.localScale = transform.localScale * scale;

                if (transform.localScale.x < smoothZoomMinScale) transform.localScale = new Vector3(smoothZoomMinScale, smoothZoomMinScale, smoothZoomMinScale);
                else if (transform.localScale.x > smoothZoomMaxScale) transform.localScale = new Vector3(smoothZoomMaxScale, smoothZoomMaxScale, smoothZoomMaxScale);

                Vector3 s = transform.localScale - originalScale;
                s.Scale(new Vector3(1f / originalScale.x, 1 / originalScale.y, 1 / originalScale.z));

                Vector3 p = transform.rotation * new Vector3(map.tilesetWidth * smoothZoomOffset.x * s.x, 0, map.tilesetHeight * smoothZoomOffset.z * s.z);
                transform.position = originalPosition - p;

                OnGestureZoom(p1, p2);
            }

            lastGestureDistance = distance;
            lastGestureCenter = center;

            if (OnSmoothZoomProcess != null) OnSmoothZoomProcess();
        }
        else
        {
            if (smoothZoomStarted)
            {
                smoothZoomStarted = false;

                float s = transform.localScale.x;
                int offset = Mathf.RoundToInt(s > originalScale.x ? s / originalScale.x - 1 : -1 / (s / originalScale.x) + 1);

                lastGestureDistance = 0;
                lastGestureCenter = Vector2.zero;

                if (offset != 0)
                {
                    ZoomOnPoint(offset, smoothZoomPoint);
                    waitMapZoom = map.zoom;
                    if (map.renderInThread)
                    {
                        waitZeroTouches = true;
                        needRestoreGestureZoom = true;
                        map.OnMapUpdated += RestoreGestureZoom;
                        map.Redraw();
                    }
                    else
                    {
                        map.RedrawImmediately();
                        RestoreGestureZoom();
                    }
                }
                else
                {
                    waitMapZoom = map.buffer.apiZoom;
                    RestoreGestureZoom();
                }
            }
        }
    }

    private void UpdateMapMesh()
    {
        int zoom = map.buffer.apiZoom;
        if (useElevation && !ignoreGetElevation && elevationBufferPosition != bufferPosition && elevationZoomRange.InRange(zoom)) GetElevation();

        int w1 = map.tilesetWidth / OnlineMapsUtils.tileSize;
        int h1 = map.tilesetHeight / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < elevationResolution) subMeshVX = elevationResolution % w1 == 0 ? elevationResolution / w1 : elevationResolution / w1 + 1;
            if (h1 < elevationResolution) subMeshVZ = elevationResolution % h1 == 0 ? elevationResolution / h1 : elevationResolution / h1 + 1;
        }

        double subMeshSizeX = (double)map.tilesetSize.x / w1;
        double subMeshSizeY = (double)map.tilesetSize.y / h1;

        double tlx, tly, brx, bry;
        map.buffer.GetCorners(out tlx, out tly, out brx, out bry);

        double tlpx, tlpy;

        map.projection.CoordinatesToTile(tlx, tly, zoom, out tlpx, out tlpy);
        double posX = tlpx - bufferPosition.x;
        double posY = tlpy - bufferPosition.y;

        int maxX = 1 << zoom;
        if (posX >= maxX) posX -= maxX;
        else if (posX < 0) posX += maxX;
        
        double startPosX = subMeshSizeX * posX;
        double startPosZ = -subMeshSizeY * posY;

        float yScale = GetBestElevationYScale(tlx, tly, brx, bry);

        int w = w1 + 2;
        int h = h1 + 2;

        Material[] materials = rendererInstance.materials;

        if (vertices.Length != w * h * (subMeshVX + 1) * (subMeshVZ + 1))
        {
            ReinitMapMesh(w, h, subMeshVX, subMeshVZ);
            materials = rendererInstance.materials;
        }

        Material fMaterial = materials[0];

        bool hasTraffic = fMaterial.HasProperty("_TrafficTex");
        bool hasOverlayBack = fMaterial.HasProperty("_OverlayBackTex");
        bool hasOverlayBackAlpha = fMaterial.HasProperty("_OverlayBackAlpha");
        bool hasOverlayFront = fMaterial.HasProperty("_OverlayFrontTex");
        bool hasOverlayFrontAlpha = fMaterial.HasProperty("_OverlayFrontAlpha");

        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                UpdateMapSubMesh(
                    x, y, w, h, subMeshSizeX, subMeshSizeY, subMeshVX, subMeshVZ, startPosX, startPosZ, yScale, 
                    tlx, tly, brx, bry, materials, ref minY, ref maxY, 
                    hasTraffic, hasOverlayBack, hasOverlayBackAlpha, hasOverlayFront, hasOverlayFrontAlpha
                );
            }
        }

        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;

        tilesetMesh.RecalculateBounds();

        if (useElevation || firstUpdate)
        {
            if (meshCollider != null)
            {
                if (firstUpdate || elevationZoomRange.InRange(zoom))
                {
                    colliderWithElevation = true;
                    if (colliderType == OnlineMapsColliderType.fullMesh) meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
                    else UpdateSimpleMeshCollider(yScale, tlx, tly, brx, bry);
                }
                else if (colliderWithElevation)
                {
                    colliderWithElevation = false;
                    if (colliderType == OnlineMapsColliderType.fullMesh) meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
                    else UpdateSimpleMeshCollider(yScale, tlx, tly, brx, bry);
                }
            }
            else if (boxCollider != null)
            {
                boxCollider.center = new Vector3(-map.tilesetSize.x / 2, (minY + maxY) / 2, map.tilesetSize.y / 2);
                boxCollider.size = new Vector3(map.tilesetSize.x, colliderType == OnlineMapsColliderType.box? maxY - minY: 0, map.tilesetSize.y);
            }

            firstUpdate = false;
        }

        if (OnMeshUpdated != null) OnMeshUpdated();
    }

    private void UpdateMapSubMesh(int x, int y, int w, int h, double subMeshSizeX, double subMeshSizeY, int subMeshVX, int subMeshVZ, double startPosX, double startPosZ, float yScale, double tlx, double tly, double brx, double bry, Material[] materials, ref float minY, ref float maxY, bool hasTraffic, bool hasOverlayBack, bool hasOverlayBackAlpha, bool hasOverlayFront, bool hasOverlayFrontAlpha)
    {
        int mi = x + y * w;
        int i = mi * (subMeshVX + 1) * (subMeshVZ + 1);

        double cellSizeX = subMeshSizeX / subMeshVX;
        double cellSizeY = subMeshSizeY / subMeshVZ;

        double uvX = 1.0 / subMeshVX;
        double uvZ = 1.0 / subMeshVZ;

        int bx = x + bufferPosition.x;
        int by = y + bufferPosition.y;

        int zoom = map.buffer.apiZoom;
        int maxX = 1 << zoom;

        if (bx >= maxX) bx -= maxX;
        if (bx < 0) bx += maxX;

        OnlineMapsTile tile;
        OnlineMapsTile.GetTile(zoom, bx, by, out tile);

        OnlineMapsTile currentTile = tile;
        Vector2 offset = Vector2.zero;
        float scale = 1;
        int z = zoom;

        Texture tileTexture = currentTile != null ? currentTile.texture : null;
        bool sendEvent = true;

        while ((currentTile == null || tileTexture == null) && z > 2)
        {
            z--;

            int s = 1 << (zoom - z);
            int ctx = bx / s;
            int cty = by / s;
            OnlineMapsTile t;
            OnlineMapsTile.GetTile(z, ctx, cty, out t);
            if (t != null && t.status == OnlineMapsTileStatus.loaded)
            {
                currentTile = t;
                tileTexture = t.texture;
                scale = 1f / s;
                offset.x = bx % s * scale;
                offset.y = (s - by % s - 1) * scale;
                sendEvent = false;
                break;
            }
        }

        bool needGetElevation = useElevation && elevationData != null && elevationZoomRange.InRange(zoom);

        float fy = 0;
        double spx = startPosX - x * subMeshSizeX;
        double spz = startPosZ + y * subMeshSizeY;
        float tilesizeX = map.tilesetSize.x;
        float tilesizeZ = map.tilesetSize.y;

        for (int ty = 0; ty <= subMeshVZ; ty++)
        {
            double uvy = 1 - uvZ * ty;
            double pz = spz + ty * cellSizeY;

            if (pz < 0)
            {
                uvy = uvZ * ((pz + cellSizeY) / cellSizeY - 1) + uvy;
                pz = 0;
            }
            else if (pz > tilesizeZ)
            {
                uvy = uvZ * ((pz - tilesizeZ) / cellSizeY) + uvy;
                pz = tilesizeZ;
            }

            for (int tx = 0; tx <= subMeshVX; tx++)
            {
                double uvx = uvX * tx;
                double px = spx - tx * cellSizeX; 

                if (px > 0)
                {
                    uvx = uvX * (px - cellSizeX) / cellSizeX + uvx + uvX;
                    px = 0;
                }
                else if (px < -tilesizeX)
                {
                    uvx = uvX * ((px + tilesizeX) / cellSizeX - 1) + uvx + uvX;
                    px = -tilesizeX;
                }

                if (needGetElevation) fy = GetElevationValue(px, pz, yScale, tlx, tly, brx, bry);

                float fx = (float) px;
                float fz = (float) pz;

                float fux = (float) uvx;
                float fuy = (float) uvy;

                if (fy < minY) minY = fy;
                if (fy > maxY) maxY = fy;

                if (fux < 0) fux = 0;
                else if (fux > 1) fux = 1;

                if (fuy < 0) fuy = 0;
                else if (fuy > 1) fuy = 1;

                vertices[i] = new Vector3(fx, fy, fz);
                uv[i++] = new Vector2(fux, fuy);
            }
        }

        Material material = materials[mi];

        if (currentTile != null)
        {
            bool hasTileTexture = tileTexture != null;
            if (!hasTileTexture)
            {
                if (map.defaultTileTexture != null) tileTexture = map.defaultTileTexture;
                else if (OnlineMapsTile.emptyColorTexture != null) tileTexture = OnlineMapsTile.emptyColorTexture;
                else
                {
                    tileTexture = OnlineMapsTile.emptyColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    tileTexture.name = "Empty Texture";
                    OnlineMapsTile.emptyColorTexture.SetPixel(0, 0, map.emptyColor);
                    OnlineMapsTile.emptyColorTexture.Apply(false);
                }

                sendEvent = false;
            }

            material.mainTextureOffset = offset;
            material.mainTextureScale = new Vector2(scale, scale);

            if (material.mainTexture != tileTexture)
            {
                material.mainTexture = tileTexture;
                if (sendEvent && OnChangeMaterialTexture != null) OnChangeMaterialTexture(currentTile, material); 
            }

            if (hasTraffic)
            {
                material.SetTexture("_TrafficTex", currentTile.trafficTexture);
                material.SetTextureOffset("_TrafficTex", material.mainTextureOffset);
                material.SetTextureScale("_TrafficTex", material.mainTextureScale);
            }
            if (hasOverlayBack)
            {
                material.SetTexture("_OverlayBackTex", currentTile.overlayBackTexture);
                material.SetTextureOffset("_OverlayBackTex", material.mainTextureOffset);
                material.SetTextureScale("_OverlayBackTex", material.mainTextureScale);
            }
            if (hasOverlayBackAlpha) material.SetFloat("_OverlayBackAlpha", currentTile.overlayBackAlpha);
            if (hasOverlayFront)
            {
                if (drawingMode == OnlineMapsTilesetDrawingMode.overlay)
                {
                    if (currentTile.status == OnlineMapsTileStatus.loaded && (currentTile.drawingChanged || currentTile.overlayFrontTexture == null))
                    {
                        if (overlayFrontBuffer == null) overlayFrontBuffer = new Color32[OnlineMapsUtils.sqrTileSize];
                        else
                        {
                            for (int k = 0; k < OnlineMapsUtils.sqrTileSize; k++) overlayFrontBuffer[k] = new Color32();
                        }
                        foreach (OnlineMapsDrawingElement drawingElement in map.drawingElements)
                        {
                            drawingElement.Draw(overlayFrontBuffer, new OnlineMapsVector2i(currentTile.x, currentTile.y), OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize, currentTile.zoom, true);
                        }
                        if (currentTile.overlayFrontTexture == null)
                        {
                            currentTile.overlayFrontTexture = new Texture2D(OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize, TextureFormat.ARGB32, false);
                            currentTile.overlayFrontTexture.wrapMode = TextureWrapMode.Clamp;
                        }
                        currentTile.overlayFrontTexture.SetPixels32(overlayFrontBuffer);
                        currentTile.overlayFrontTexture.Apply(false);
                    }
                }

                material.SetTexture("_OverlayFrontTex", currentTile.overlayFrontTexture);
                material.SetTextureOffset("_OverlayFrontTex", material.mainTextureOffset);
                material.SetTextureScale("_OverlayFrontTex", material.mainTextureScale);
            }
            if (hasOverlayFrontAlpha) material.SetFloat("_OverlayFrontAlpha", currentTile.overlayFrontAlpha);
            if (OnDrawTile != null) OnDrawTile(currentTile, material);
        }
        else
        {
            if (OnlineMapsTile.emptyColorTexture == null)
            {
                OnlineMapsTile.emptyColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                OnlineMapsTile.emptyColorTexture.name = "Empty Texture";
                OnlineMapsTile.emptyColorTexture.SetPixel(0, 0, map.emptyColor);
                OnlineMapsTile.emptyColorTexture.Apply(false);
            }

            material.mainTexture = OnlineMapsTile.emptyColorTexture;
            if (hasTraffic) material.SetTexture("_TrafficTex", null);
            if (hasOverlayBack) material.SetTexture("_OverlayBackTex", null);
            if (hasOverlayFront) material.SetTexture("_OverlayFrontTex", null);
        }
    }

    private void UpdateMarkersMesh()
    {
        if (markersGameObjects == null) InitMarkersMesh(0);

        double tlx, tly, brx, bry;
        map.GetCorners(out tlx, out tly, out brx, out bry);
        if (brx < tlx) brx += 360;

        int zoom = map.buffer.apiZoom;
        int maxX = 1 << zoom;
        int maxX2 = maxX / 2;

        double px, py;
        map.projection.CoordinatesToTile(tlx, tly, zoom, out px, out py);

        float yScale = GetBestElevationYScale(tlx, tly, brx, bry);

        float cx = -map.tilesetSize.x / map.tilesetWidth;
        float cy = map.tilesetSize.y / map.tilesetHeight;

        if (usedMarkers == null) usedMarkers = new List<TilesetFlatMarker>(32);
        else
        {
            for (int i = 0; i < usedMarkers.Count; i++) usedMarkers[i].Dispose();
            usedMarkers.Clear();
        }

        List<Texture> usedTextures = new List<Texture> (32) { map.defaultMarkerTexture };
        List<List<int>> usedTexturesMarkerIndex = new List<List<int>>(32) { new List<int>(32) };

        int usedMarkersCount = 0;

        Bounds tilesetBounds = new Bounds(new Vector3(map.tilesetSize.x / -2, 0, map.tilesetSize.y / 2), new Vector3(map.tilesetSize.x, 0, map.tilesetSize.y));

        IEnumerable<OnlineMapsMarker> markers = map.markers.Where(delegate(OnlineMapsMarker marker)
        {
            if (!marker.enabled || !marker.range.InRange(zoom)) return false;

            if (OnCheckMarker2DVisibility != null)
            {
                if (!OnCheckMarker2DVisibility(marker)) return false;
            }
            else if (checkMarker2DVisibility == OnlineMapsTilesetCheckMarker2DVisibility.pivot)
            {
                double mx, my;
                marker.GetPosition(out mx, out my);

                bool a = my > tly || 
                         my < bry ||
                         (
                            (mx < tlx || mx > brx) &&
                            (mx + 360 < tlx || mx + 360 > brx) &&
                            (mx - 360 < tlx || mx - 360 > brx)
                         );
                if (a) return false;
            }

            return true;
        });

        float[] offsets = null;
        bool useOffsetY = false;

        int index = 0;

        if (markerComparer != null)
        {
            markers = markers.OrderBy(m => m, markerComparer);
        }
        else
        {
            markers = markers.OrderBy(m =>
            {
                double mx, my;
                m.GetPosition(out mx, out my);
                return 90 - my;
            });
            useOffsetY = OnGetFlatMarkerOffsetY != null;

            if (useOffsetY)
            {
                int countMarkers = markers.Count();

                TilesetSortedMarker[] sortedMarkers = new TilesetSortedMarker[countMarkers];
                foreach (OnlineMapsMarker marker in markers)
                {
                    sortedMarkers[index++] = new TilesetSortedMarker
                    {
                        marker = marker,
                        offset = OnGetFlatMarkerOffsetY(marker)
                    };
                }

                offsets = new float[countMarkers];
                OnlineMapsMarker[] nMarkers = new OnlineMapsMarker[countMarkers];
                int i = 0;
                foreach (TilesetSortedMarker sm in sortedMarkers.OrderBy(m => m.offset))
                {
                    nMarkers[i] = sm.marker;
                    offsets[i] = sm.offset;
                    i++;
                    sm.Dispose();
                }
                markers = nMarkers;
            }
        }

        if (markersVertices == null) markersVertices = new List<Vector3>(64);
        else markersVertices.Clear(); 

        Vector3 tpos = transform.position;

        foreach (Mesh mesh in markersMeshes) mesh.Clear();

        Matrix4x4 matrix = new Matrix4x4();
        int meshIndex = 0;
        index = -1;
        foreach (OnlineMapsMarker marker in markers)
        {
            index++;
            double fx, fy;
            marker.GetTilePosition(out fx, out fy);

            Vector2 offset = marker.GetAlignOffset();
            offset *= marker.scale;

            fx = fx - px;
            if (fx < -maxX2) fx += maxX;
            else if (fx > maxX2) fx -= maxX;
            fx = fx * OnlineMapsUtils.tileSize - offset.x;
            fy = (fy - py) * OnlineMapsUtils.tileSize - offset.y;

            if (marker.texture == null) marker.texture = map.defaultMarkerTexture;

            float markerWidth = marker.texture.width * marker.scale;
            float markerHeight = marker.texture.height * marker.scale;

            float rx1 = (float)(fx * cx);
            float ry1 = (float)(fy * cy);
            float rx2 = (float)((fx + markerWidth) * cx);
            float ry2 = (float)((fy + markerHeight) * cy);

            Vector3 center = new Vector3((float)((fx + offset.x) * cx), 0, (float)((fy + offset.y) * cy));

            Vector3 p1 = new Vector3(rx1 - center.x, 0, ry1 - center.z);
            Vector3 p2 = new Vector3(rx2 - center.x, 0, ry1 - center.z);
            Vector3 p3 = new Vector3(rx2 - center.x, 0, ry2 - center.z);
            Vector3 p4 = new Vector3(rx1 - center.x, 0, ry2 - center.z);

            float angle = Mathf.Repeat(marker.rotation, 1) * 360;

            if (Math.Abs(angle) > float.Epsilon)
            {
                matrix.SetTRS(Vector3.zero, Quaternion.Euler(0, angle, 0), Vector3.one);

                p1 = matrix.MultiplyPoint(p1) + center;
                p2 = matrix.MultiplyPoint(p2) + center;
                p3 = matrix.MultiplyPoint(p3) + center;
                p4 = matrix.MultiplyPoint(p4) + center;
            }
            else
            {
                p1 += center;
                p2 += center;
                p3 += center;
                p4 += center;
            }

            if (checkMarker2DVisibility == OnlineMapsTilesetCheckMarker2DVisibility.bounds)
            {
                Vector3 markerCenter = (p2 + p4) / 2;
                Vector3 markerSize = p4 - p2;
                if (!tilesetBounds.Intersects(new Bounds(markerCenter, markerSize))) continue;
            }

            float y = GetElevationValue((rx1 + rx2) / 2, (ry1 + ry2) / 2, yScale, tlx, tly, brx, bry);
            float yOffset = useOffsetY ? offsets[index] : 0;

            p1.y = p2.y = p3.y = p4.y = y + yOffset;

            markersVertices.Add(p1);
            markersVertices.Add(p2);
            markersVertices.Add(p3);
            markersVertices.Add(p4);

            usedMarkers.Add(new TilesetFlatMarker(marker, p1 + tpos, p2 + tpos, p3 + tpos, p4 + tpos));

            if (marker.texture == map.defaultMarkerTexture)
            {
                usedTexturesMarkerIndex[0].Add(usedMarkersCount);
            }
            else
            {
                int textureIndex = usedTextures.IndexOf(marker.texture);
                if (textureIndex != -1)
                {
                    usedTexturesMarkerIndex[textureIndex].Add(usedMarkersCount);
                }
                else
                {
                    usedTextures.Add(marker.texture);
                    usedTexturesMarkerIndex.Add(new List<int>(32));
                    usedTexturesMarkerIndex[usedTexturesMarkerIndex.Count - 1].Add(usedMarkersCount);
                }
            }

            usedMarkersCount++;

            if (usedMarkersCount == 16250)
            {
                SetMarkersMesh(usedMarkersCount, usedTextures, usedTexturesMarkerIndex, meshIndex);
                meshIndex++;
                markersVertices.Clear();
                usedMarkersCount = 0;
                usedTextures.Clear();
                usedTextures.Add(map.defaultMarkerTexture);
                usedTexturesMarkerIndex.Clear();
                usedTexturesMarkerIndex.Add(new List<int>(32));
            }
        }

        SetMarkersMesh(usedMarkersCount, usedTextures, usedTexturesMarkerIndex, meshIndex);
    }

    private void UpdateSimpleMeshCollider(float yScale, double tlx, double tly, double brx, double bry)
    {
        int res = useElevation && elevationZoomRange.InRange(map.zoom) ? 6 : 1;
        int r2 = res + 1;

        Vector3[] vertices = new Vector3[r2 * r2];
        float sx = -map.tilesetSize.x / res;
        float sy = map.tilesetSize.y / res;

        int[] triangles = new int[res * res * 6];
        int ti = 0;

        for (int y = 0; y < r2; y++)
        {
            for (int x = 0; x < r2; x++)
            {
                float px = sx * x;
                float pz = sy * y;
                float py = GetElevationValue(px, pz, yScale, tlx, tly, brx, bry);
                vertices[y * r2 + x] = new Vector3(sx * x, py, sy * y);

                if (x != 0 && y != 0)
                {
                    int p4 = y * r2 + x;
                    int p3 = p4 - 1;
                    int p2 = p4 - r2;
                    int p1 = p2 - 1;

                    triangles[ti++] = p1;
                    triangles[ti++] = p2;
                    triangles[ti++] = p4;
                    triangles[ti++] = p1;
                    triangles[ti++] = p4;
                    triangles[ti++] = p3;
                }
            }
        }

        Mesh mesh = meshCollider.sharedMesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = mesh;
    }

    internal class TilesetFlatMarker
    {
        public OnlineMapsMarker marker;
        private double[] poly;

        public TilesetFlatMarker(OnlineMapsMarker marker, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            this.marker = marker;
            poly = new double[] {p1.x, p1.z, p2.x, p2.z, p3.x, p3.z, p4.x, p4.z};
        }

        public bool Contains(Vector3 point, Transform transform)
        {
            Vector3 p = Quaternion.Inverse(transform.rotation) * (point - transform.position);
            p.x /= transform.lossyScale.x;
            p.z /= transform.lossyScale.z;
            p += transform.position;
            return OnlineMapsUtils.IsPointInPolygon(poly, p.x, p.z);
        }

        public void Dispose()
        {
            marker = null;
            poly = null;
        }
    }

    internal class TilesetSortedMarker
    {
        public OnlineMapsMarker marker;
        public float offset;

        public void Dispose()
        {
            marker = null;
        }
    }

    /// <summary>
    /// Type of tileset map collider.
    /// </summary>
    public enum OnlineMapsColliderType
    {
        box,
        fullMesh,
        simpleMesh,
        flatBox
    }

    /// <summary>
    /// Mode of smooth zoom.
    /// </summary>
    public enum OnlineMapsSmoothZoomMode
    {
        /// <summary>
        /// Zoom at touch point.
        /// </summary>
        target,

        /// <summary>
        /// Zoom at center of map.
        /// </summary>
        center
    }

    public enum ElevationBottomMode
    {
        zero,
        minValue
    }
}