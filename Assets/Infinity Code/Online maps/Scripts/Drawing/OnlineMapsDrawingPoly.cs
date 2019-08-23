/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class draws a closed polygon on the map.
/// </summary>
public class OnlineMapsDrawingPoly : OnlineMapsDrawingElement
{
    private static List<Vector3> vertices;
    private static List<Vector3> normals;
    private static List<int> triangles;
    private static List<Vector2> uv;

    private Color _backgroundColor = new Color(1, 1, 1, 0);
    private Color _borderColor = Color.black;
    private float _borderWidth = 1;

    /// <summary>
    /// Background color of the polygon.\n
    /// Note: Not supported in tileset.
    /// </summary>
    public Color backgroundColor
    {
        get { return _backgroundColor; }
        set
        {
            _backgroundColor = value;
            OnlineMaps.instance.Redraw();
        }
    }

    /// <summary>
    /// Border color of the polygon.
    /// </summary>
    public Color borderColor
    {
        get { return _borderColor; }
        set
        {
            _borderColor = value;
            OnlineMaps.instance.Redraw();
        }
    }

    [Obsolete("Renamed. Use borderWidth.")]
    public float borderWeight
    {
        get { return borderWidth; }
        set { borderWidth = value; }
    }

    /// <summary>
    /// Border width of the polygon.
    /// </summary>
    public float borderWidth
    {
        get { return _borderWidth; }
        set
        {
            _borderWidth = value;
            OnlineMaps.instance.Redraw();
        }
    }

    /// <summary>
    /// IEnumerable of points of the polygon. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </summary>
    public IEnumerable points
    {
        get { return _points; }
        set
        {
            if (value == null) throw new Exception("Points can not be null.");
            _points = value;
        }
    }

    /// <summary>
    /// Center point of the polygon.
    /// </summary>
    public override Vector2 center
    {
        get
        {
            Vector2 centerPoint = Vector2.zero;
            int count = 0;
            foreach (Vector2 point in points)
            {
                centerPoint += point;
                count++;
            }
            if (count == 0) return Vector2.zero;
            return centerPoint / count;
        }
    }

    private IEnumerable _points;

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    public OnlineMapsDrawingPoly()
    {
        points = new List<Vector2>();
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the polygon. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    public OnlineMapsDrawingPoly(IEnumerable points):this()
    {
        if (points == null) throw new Exception("Points can not be null.");
        _points = points;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the polygon. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    /// <param name="borderColor">Border color of the polygon.</param>
    public OnlineMapsDrawingPoly(IEnumerable points, Color borderColor)
        : this(points)
    {
        this.borderColor = borderColor;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the polygon. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    /// <param name="borderColor">Border color of the polygon.</param>
    /// <param name="borderWidth">Border width of the polygon.</param>
    public OnlineMapsDrawingPoly(IEnumerable points, Color borderColor, float borderWidth)
        : this(points, borderColor)
    {
        this.borderWidth = borderWidth;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the polygon. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    /// <param name="borderColor">Border color of the polygon.</param>
    /// <param name="borderWidth">Border width of the polygon.</param>
    /// <param name="backgroundColor">
    /// Background color of the polygon.\n
    /// Note: Not supported in tileset.
    /// </param>
    public OnlineMapsDrawingPoly(IEnumerable points, Color borderColor, float borderWidth, Color backgroundColor)
        : this(points, borderColor, borderWidth)
    {
        this.backgroundColor = backgroundColor;
    }

    public override void Draw(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, bool invertY = false)
    {
        if (!visible) return;

        FillPoly(buffer, bufferPosition, bufferWidth, bufferHeight, zoom, points, backgroundColor, invertY);
        DrawLineToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, zoom, points, borderColor, borderWidth, true, invertY);
    }

    public override void DrawOnTileset(OnlineMapsTileSetControl control, int index)
    {
        base.DrawOnTileset(control, index);

        if (!visible)
        {
            active = false;
            return;
        }

        InitMesh(control, "Poly", borderColor, backgroundColor);
        InitLineMesh(points, control, ref vertices, ref normals, ref triangles, ref uv, borderWidth, true, false);

        if (vertices.Count > 0)
        {
            Vector3 v1 = (vertices[0] + vertices[3]) / 2;
            Vector3 v2 = (vertices[vertices.Count - 3] + vertices[vertices.Count - 2]) / 2;
            if (Math.Abs((v1 - v2).magnitude) < float.Epsilon)
            {
                vertices[0] = vertices[vertices.Count - 3] = (vertices[0] + vertices[vertices.Count - 3]) / 2;
                vertices[3] = vertices[vertices.Count - 2] = (vertices[3] + vertices[vertices.Count - 2]) / 2;
            }
        }

        int[] fillTriangles = null;

        if (!checkMapBoundaries && backgroundColor.a > 0 && vertices.Count > 0)
        {
            float l1 = 0;
            float l2 = 0;

            for (int i = 0; i < vertices.Count / 4 - 1; i++)
            {
                Vector3 p11 = vertices[i * 4];
                Vector3 p12 = vertices[(i + 1) * 4];

                Vector3 p21 = vertices[i * 4 + 3];
                Vector3 p22 = vertices[(i + 1) * 4 + 3];

                l1 += (p11 - p12).magnitude;
                l2 += (p21 - p22).magnitude;
            }

            bool side = l2 < l1;
            int off1 = side ? 3 : 0;
            int off2 = side ? 2 : 1;

            Vector2 lastPoint = Vector2.zero;
            List<int> internalIndices = new List<int>(vertices.Count / 4);
            List<Vector2> internalPoints = new List<Vector2>(vertices.Count / 4);
            for (int i = 0; i < vertices.Count / 4; i++)
            {
                Vector3 p = vertices[i * 4 + off1];
                Vector2 p2 = new Vector2(p.x, p.z);
                if (i > 0)
                {


                    if ((lastPoint - p2).magnitude > borderWidth / 2)
                    {
                        internalIndices.Add(i * 4 + off1);
                        internalPoints.Add(p2);
                        lastPoint = p2;
                    }
                }
                else
                {
                    internalIndices.Add(i * 4 + off1);
                    internalPoints.Add(p2);
                    lastPoint = p2;
                }
                p = vertices[i * 4 + off2];
                p2 = new Vector2(p.x, p.z);
                if ((lastPoint - p2).magnitude > borderWidth / 2)
                {
                    internalIndices.Add(i * 4 + off2);
                    internalPoints.Add(p2);
                    lastPoint = p2;
                }
            }

            if (internalPoints[0] == internalPoints[internalPoints.Count - 1]) internalPoints.RemoveAt(internalPoints.Count - 1);

            fillTriangles = OnlineMapsUtils.Triangulate(internalPoints).ToArray();

            for (int i = 0; i < fillTriangles.Length; i++) fillTriangles[i] = internalIndices[fillTriangles[i]];

            Vector3 side1 = vertices[fillTriangles[1]] - vertices[fillTriangles[0]];
            Vector3 side2 = vertices[fillTriangles[2]] - vertices[fillTriangles[0]];
            Vector3 perp = Vector3.Cross(side1, side2);

            bool reversed = perp.y < 0;
            if (reversed) fillTriangles = fillTriangles.Reverse().ToArray();
        }

        mesh.Clear();
        mesh.subMeshCount = 2;

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();
#else
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uv);
#endif

        mesh.SetTriangles(triangles.ToArray(), 0);
        if (fillTriangles != null) mesh.SetTriangles(fillTriangles.ToArray(), 1);

        UpdateMaterialsQuote(control, index);
    }

    public override bool HitTest(Vector2 positionLngLat, int zoom)
    {
        if (points == null) return false;
        return OnlineMapsUtils.IsPointInPolygon(points, positionLngLat.x, positionLngLat.y);
    }

    protected override void DisposeLate()
    {
        base.DisposeLate();

        _points = null;
    }

    public override bool Validate()
    {
        return _points != null;
    }
}