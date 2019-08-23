/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Component that controls the buildings.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Buildings")]
[Serializable]
public class OnlineMapsBuildings : MonoBehaviour
{
    /// <summary>
    /// The event, which occurs when creating of the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnBuildingCreated;

    /// <summary>
    /// The event, which occurs when disposing of the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnBuildingDispose;

    /// <summary>
    /// This event is triggered before create a building. \n
    /// Return TRUE - if you want to create this building, FALSE - avoid creating this building.
    /// </summary>
    public Predicate<OnlineMapsBuildingsNodeData> OnCreateBuilding;

    /// <summary>
    /// The event, which occurs when the new building was received.
    /// </summary>
    public Action OnNewBuildingsReceived;

    /// <summary>
    /// This event is fired when the height of the building is unknown.\n
    /// It allows you to control the height of buildings.\n
    /// Return - the height of buildings.
    /// </summary>
    public Func<OnlineMapsOSMWay, float> OnGenerateBuildingHeight;

    /// <summary>
    /// This event is called when creating a request to OSM Overpass API.
    /// </summary>
    public Func<string, Vector2, Vector2, string> OnPrepareRequest;

    /// <summary>
    /// The event, which occurs when the request for a building sent.
    /// </summary>
    public Action OnRequestSent;

    /// <summary>
    /// The event, which occurs after the response has been received.
    /// </summary>
    public Action OnRequestComplete;

    /// <summary>
    /// This event is triggered before show a building. \n
    /// Return TRUE - if you want to show this building, FALSE - do not show this building.
    /// </summary>
    public Predicate<OnlineMapsBuildingBase> OnShowBuilding;

    /// <summary>
    /// GameObject, which will create the building.
    /// </summary>
    public static GameObject buildingContainer;

    public static float requestRate = 0.1f;

    private static OnlineMapsBuildings _instance;

    [NonSerialized]
    public Dictionary<string, OnlineMapsBuildingBase> buildings;

    /// <summary>
    /// Range of zoom, in which the building will be created.
    /// </summary>
    public OnlineMapsRange zoomRange = new OnlineMapsRange(19, OnlineMaps.MAXZOOM);

    /// <summary>
    /// Range levels of buildings, if the description of the building is not specified.
    /// </summary>
    public OnlineMapsRange levelsRange = new OnlineMapsRange(3, 7, 1, 100);

    /// <summary>
    /// Height of the building level.
    /// </summary>
    public float levelHeight = 4.5f;

    /// <summary>
    /// Minimal height of the building.
    /// </summary>
    public float minHeight = 4.5f;

    /// <summary>
    /// Scale height of the building.
    /// </summary>
    public float heightScale = 1;

    /// <summary>
    /// Materials of buildings.
    /// </summary>
    public OnlineMapsBuildingMaterial[] materials;

    /// <summary>
    /// The maximum number of active buildings (0 - unlimited).
    /// </summary>
    public int maxActiveBuildings = 0;

    /// <summary>
    /// The maximum number of buildings (0 - unlimited).
    /// </summary>
    public int maxBuilding = 0;
    
    private OnlineMapsVector2i topLeft;
    private OnlineMapsVector2i bottomRight;

    private Dictionary<string, OnlineMapsBuildingBase> unusedBuildings;
    private Queue<OnlineMapsBuildingsNodeData> newBuildingsData;

    private bool sendBuildingsReceived = false;
    private string requestData;
    private float lastRequestTime;

    [NonSerialized]
    private OnlineMapsOSMAPIQuery osmRequest;

    private static OnlineMaps map
    {
        get { return OnlineMaps.instance; }
    }

    /// <summary>
    /// Instance of OnlineMapsBuildings.
    /// </summary>
    public static OnlineMapsBuildings instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Returns the active (visible) building.
    /// </summary>
    public IEnumerable<OnlineMapsBuildingBase> activeBuildings
    {
        get { return buildings.Select(b => b.Value); }
    }

    public void CreateBuilding(OnlineMapsBuildingsNodeData data)
    {
        if (OnCreateBuilding != null && !OnCreateBuilding(data)) return;
        if (buildings.ContainsKey(data.way.id) || unusedBuildings.ContainsKey(data.way.id)) return;

        OnlineMapsBuildingBase building = OnlineMapsBuildingBuiltIn.Create(this, data.way, data.nodes);

        if (building != null)
        {
            building.LoadMeta(data.way);
            if (OnBuildingCreated != null) OnBuildingCreated(building);
            unusedBuildings.Add(data.way.id, building);
        }
    }

    private void GenerateBuildings()
    {
        long startTicks = DateTime.Now.Ticks;
        const int maxTicks = 500000;

        lock (newBuildingsData)
        {
            int newBuildingIndex = newBuildingsData.Count;
            while (newBuildingIndex > 0)
            {
                if (maxBuilding > 0 && unusedBuildings.Count + buildings.Count >= maxBuilding) break;

                newBuildingIndex--;
                OnlineMapsBuildingsNodeData data = newBuildingsData.Dequeue();
                CreateBuilding(data);

                data.Dispose();

                if (DateTime.Now.Ticks - startTicks > maxTicks) break;
            }
        }

        OnlineMapsBuildingBuiltIn.roofIndices = null;
    }

    private void LoadNewBuildings()
    {
        double tlx, tly, brx, bry;
        map.projection.TileToCoordinates(topLeft.x, topLeft.y, map.zoom, out tlx, out tly);
        map.projection.TileToCoordinates(bottomRight.x, bottomRight.y, map.zoom, out brx, out bry);

        requestData = String.Format("node({0},{1},{2},{3});way(bn)[{4}];(._;>;);out;", bry, tlx, tly, brx, "'building'");
        if (OnPrepareRequest != null) requestData = OnPrepareRequest(requestData, new Vector2((float)tlx, (float)tly), new Vector2((float)brx, (float)bry));
    }

    private void OnBuildingRequestComplete(string response)
    {
        osmRequest = null;

        Action action = () =>
        {
            Dictionary<string, OnlineMapsOSMNode> nodes;
            List<OnlineMapsOSMWay> ways;
            List<OnlineMapsOSMRelation> relations;

            OnlineMapsOSMAPIQuery.ParseOSMResponseFast(response, out nodes, out ways, out relations);

            lock (newBuildingsData)
            {
                foreach (OnlineMapsOSMWay way in ways)
                {
                    newBuildingsData.Enqueue(new OnlineMapsBuildingsNodeData(way, nodes));
                }
            }

            sendBuildingsReceived = true;
        };

#if !UNITY_WEBGL
        if (map.renderInThread) OnlineMapsThreadManager.AddThreadAction(action);
        else action();
#else
        action();
#endif

        if (OnRequestComplete != null) OnRequestComplete();
    }

    private void OnEnable()
    {
        _instance = this;

        buildings = new Dictionary<string, OnlineMapsBuildingBase>();
        unusedBuildings = new Dictionary<string, OnlineMapsBuildingBase>();
        newBuildingsData = new Queue<OnlineMapsBuildingsNodeData>();

        buildingContainer = new GameObject("Buildings");
        buildingContainer.transform.parent = transform;
        buildingContainer.transform.localPosition = Vector3.zero;
        buildingContainer.transform.localRotation = Quaternion.Euler(Vector3.zero);
        buildingContainer.transform.localScale = Vector3.one;
    }

    private void SendRequest()
    {
        if (osmRequest != null || string.IsNullOrEmpty(requestData)) return;

        osmRequest = OnlineMapsOSMAPIQuery.Find(requestData);
        osmRequest.OnComplete += OnBuildingRequestComplete;
        if (OnRequestSent != null) OnRequestSent();
        lastRequestTime = Time.time;
        requestData = null;
    }

    private void Start()
    {
        buildingContainer.layer = map.gameObject.layer;

        map.OnChangePosition += UpdateBuildings;
        map.OnChangeZoom += UpdateBuildingsScale;

        UpdateBuildings();
    }

    private void RemoveAllBuildings()
    {
        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings)
        {
            if (OnBuildingDispose != null) OnBuildingDispose(building.Value);
            OnlineMapsUtils.DestroyImmediate(building.Value.gameObject);
        }

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings)
        {
            if (OnBuildingDispose != null) OnBuildingDispose(building.Value);
            OnlineMapsUtils.DestroyImmediate(building.Value.gameObject);
        }

        buildings.Clear();
        unusedBuildings.Clear();
    }

    private void Update()
    {
        if (sendBuildingsReceived)
        {
            if (OnNewBuildingsReceived != null) OnNewBuildingsReceived();
            sendBuildingsReceived = false;
        }

        GenerateBuildings();
        UpdateBuildings();
    }

    private void UpdateBuildings()
    {
        if (!zoomRange.InRange(map.zoom))
        {
            RemoveAllBuildings();
            return;
        }

        double tlx, tly, brx, bry;
        map.GetTileCorners(out tlx, out tly, out brx, out bry);

        OnlineMapsVector2i newTopLeft = new OnlineMapsVector2i((int)Math.Round(tlx - 2), (int)Math.Round(tly - 2));
        OnlineMapsVector2i newBottomRight = new OnlineMapsVector2i((int)Math.Round(brx + 2), (int)Math.Round(bry + 2));

        if (newTopLeft != topLeft || newBottomRight != bottomRight)
        {
            topLeft = newTopLeft;
            bottomRight = newBottomRight;
            LoadNewBuildings();
        }

        if (lastRequestTime + requestRate < Time.time) SendRequest();

        UpdateBuildingsPosition();
    }

    private void UpdateBuildingsPosition()
    {
        OnlineMapsTileSetControl control = OnlineMapsTileSetControl.instance;

        Bounds bounds = new Bounds();

        double tlx, tly, brx, bry;
        map.GetCorners(out tlx, out tly, out brx, out bry);

        bounds.min = new Vector3((float)tlx, (float)bry);
        bounds.max = new Vector3((float)brx, (float)tly);

        List<string> unusedKeys = new List<string>();

        bool useElevation = control.useElevation;

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings)
        {
            if (!bounds.Intersects(building.Value.boundsCoords)) unusedKeys.Add(building.Key);
            else
            {
                if (useElevation)
                {
                    Vector3 newPosition = control.GetWorldPositionWithElevation(building.Value.centerCoordinates.x, building.Value.centerCoordinates.y, tlx, tly, brx, bry);
                    building.Value.transform.position = newPosition;
                }
                else
                {
                    Vector3 newPosition = control.GetWorldPosition(building.Value.centerCoordinates.x, building.Value.centerCoordinates.y);
                    building.Value.transform.position = newPosition;
                }
            }
        }

        List<string> usedKeys = new List<string>();
        List<string> destroyKeys = new List<string>();

        double px, py;
        map.GetTilePosition(out px, out py);

        float maxDistance = Mathf.Pow((map.width / OnlineMapsUtils.tileSize) >> 1, 2) + Mathf.Pow((map.height / OnlineMapsUtils.tileSize) >> 1, 2) * 4;

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings)
        {
            OnlineMapsBuildingBase value = building.Value;
            if (bounds.Intersects(value.boundsCoords))
            {
                usedKeys.Add(building.Key);
                if (useElevation)
                {
                    Vector3 newPosition = control.GetWorldPositionWithElevation(building.Value.centerCoordinates, tlx, tly, brx, bry);
                    building.Value.transform.position = newPosition;
                }
                else
                {
                    Vector3 newPosition = control.GetWorldPosition(building.Value.centerCoordinates.x, building.Value.centerCoordinates.y);
                    building.Value.transform.position = newPosition;
                }
            }
            else
            {
                double bx, by;
                map.projection.CoordinatesToTile(value.centerCoordinates.x, value.centerCoordinates.y, map.zoom, out bx, out by);
                if (OnlineMapsUtils.SqrMagnitude(0, 0, bx - px, by - py) > maxDistance) destroyKeys.Add(building.Key);
            }
        }

        for (int i = 0; i < unusedKeys.Count; i++)
        {
            string key = unusedKeys[i];
            OnlineMapsBuildingBase value = buildings[key];
            value.gameObject.SetActive(false);
            unusedBuildings.Add(key, value);
            buildings.Remove(key);
        }

        for (int i = 0; i < usedKeys.Count; i++)
        {
            if (maxActiveBuildings > 0 && buildings.Count >= maxActiveBuildings) break;

            string key = usedKeys[i];
            OnlineMapsBuildingBase value = unusedBuildings[key];

            if (OnShowBuilding != null && !OnShowBuilding(value)) continue;
            value.gameObject.SetActive(true);
            buildings.Add(key, value);
            unusedBuildings.Remove(key);
        }

        for (int i = 0; i < destroyKeys.Count; i++)
        {
            string key = destroyKeys[i];
            OnlineMapsBuildingBase value = unusedBuildings[key];
            if (OnBuildingDispose != null) OnBuildingDispose(value);
            value.Dispose();
            OnlineMapsUtils.DestroyImmediate(value.gameObject);
            unusedBuildings.Remove(key);
        }
    }

    private void UpdateBuildingsScale()
    {
        UpdateBuildings();
        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings) UpdateBuildingScale(building.Value);
        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings) UpdateBuildingScale(building.Value);
    }

    private static void UpdateBuildingScale(OnlineMapsBuildingBase building)
    {
        if (building.initialZoom == map.zoom) building.transform.localScale = Vector3.one;
        else if (building.initialZoom < map.zoom) building.transform.localScale = Vector3.one * (1 << map.zoom - building.initialZoom);
        else if (building.initialZoom > map.zoom) building.transform.localScale = Vector3.one / (1 << building.initialZoom - map.zoom);
    }
}

/// <summary>
/// It contains a dictionary of nodes and way of a building contour.
/// </summary>
public class OnlineMapsBuildingsNodeData
{
    /// <summary>
    /// Way of a building contour.
    /// </summary>
    public OnlineMapsOSMWay way;

    /// <summary>
    /// Dictionary of nodes.
    /// </summary>
    public Dictionary<string, OnlineMapsOSMNode> nodes;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="way">Way of a building contour.</param>
    /// <param name="nodes">Dictionary of nodes.</param>
    public OnlineMapsBuildingsNodeData(OnlineMapsOSMWay way, Dictionary<string, OnlineMapsOSMNode> nodes)
    {
        this.way = way;
        this.nodes = nodes;
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public void Dispose()
    {
        way = null;
        nodes = null;
    }
}