/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that draws a line on the map.
/// </summary>
public class OnlineMapsDrawingLine : OnlineMapsDrawingElement
{
    private static List<Vector3> vertices;
    private static List<Vector3> normals;
    private static List<int> triangles;
    private static List<Vector2> uv;

    private Color _color = Color.black;
    private Texture2D _texture;
    private float _width = 1;
    private IEnumerable _points;

    /// <summary>
    /// Color of the line.
    /// </summary>
    public Color color
    {
        get { return _color; }
        set
        {
            _color = value;
            OnlineMaps.instance.Redraw();
        }
    }

    /// <summary>
    /// Texture of line.\n
    /// Uses only in tileset.
    /// </summary>
    public Texture2D texture
    {
        get { return _texture; }
        set
        {
            _texture = value;
            OnlineMaps.instance.Redraw();
        }
    }

    /// <summary>
    /// IEnumerable of points of the line. Geographic coordinates.\n
    /// Can be:\n
    /// IEnumerable<Vector2>, where X - longitide, Y - latitude, \n
    /// IEnumerable<float> or IEnumerable<double>, where values (lng, lat, lng, lat... etc).
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

    [Obsolete("Renamed. Use width.")]
    public float weight
    {
        get { return width; }
        set { width = value; }
    }

    /// <summary>
    /// Width of the line.
    /// </summary>
    public float width
    {
        get { return _width; }
        set
        {
            _width = value;
            OnlineMaps.instance.Redraw();
        }
    }

    /// <summary>
    /// Creates a new line.
    /// </summary>
    public OnlineMapsDrawingLine()
    {
        points = new List<Vector2>();
    }

    /// <summary>
    /// Creates a new line.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the line. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    public OnlineMapsDrawingLine(IEnumerable points):this()
    {
        if (_points == null) throw new Exception("Points can not be null.");
        _points = points;
    }

    /// <summary>
    /// Creates a new line.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the line. Geographic coordinates.\n
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    /// <param name="color">Color of the line.</param>
    public OnlineMapsDrawingLine(IEnumerable points, Color color):this(points)
    {
        this.color = color;
    }

    /// <summary>
    /// Creates a new line.
    /// </summary>
    /// <param name="points">
    /// IEnumerable of points of the line. Geographic coordinates.
    /// The values can be of type: Vector2, float, double.\n
    /// If values float or double, the value should go in pairs(longitude, latitude).
    /// </param>
    /// <param name="color">Color of the line.</param>
    /// <param name="width">Width of the line.</param>
    public OnlineMapsDrawingLine(IEnumerable points, Color color, float width) : this(points, color)
    {
        this.width = width;
    }

    public override void Draw(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, bool invertY = false)
    {
        if (!visible) return;

        DrawLineToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, zoom, points, color, width, false, invertY);
    }

    public override void DrawOnTileset(OnlineMapsTileSetControl control, int index)
    {
        base.DrawOnTileset(control, index);

        if (!visible)
        {
            active = false;
            return;
        }

        InitMesh(control, "Line", color, default(Color), texture);

        InitLineMesh(points, control, ref vertices, ref normals, ref triangles, ref uv, width);

        mesh.Clear();

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

        UpdateMaterialsQuote(control, index);
    }

    protected override void DisposeLate()
    {
        base.DisposeLate();

        _points = null;
        texture = null;
    }

    public override bool HitTest(Vector2 positionLngLat, int zoom)
    {
        if (points == null) return false;

        double cx, cy;
        OnlineMapsProjection projection = api.projection;
        projection.CoordinatesToTile(positionLngLat.x, positionLngLat.y, zoom, out cx, out cy);

        int valueType = -1; // 0 - Vector2, 1 - float, 2 - double

        object v1 = null;
        object v2 = null;
        object v3 = null;
        int i = 0;

        float sqrW = width * width;

        foreach (object p in points)
        {
            if (valueType == -1)
            {
                if (p is Vector2) valueType = 0;
                else if (p is float) valueType = 1;
                else if (p is double) valueType = 2;
            }

            object v4 = v3;
            v3 = v2;
            v2 = v1;
            v1 = p;

            double p1tx = 0, p1ty = 0, p2tx = 0, p2ty = 0;
            bool drawPart = false;

            if (valueType == 0)
            {
                if (i > 0)
                {
                    Vector2 p1 = (Vector2)v2;
                    Vector2 p2 = (Vector2)v1;

                    projection.CoordinatesToTile(p1.x, p1.y, zoom, out p1tx, out p1ty);
                    projection.CoordinatesToTile(p2.x, p2.y, zoom, out p2tx, out p2ty);
                    drawPart = true;
                }
            }
            else if (i > 2 && i % 2 == 1)
            {
                if (valueType == 1)
                {
                    projection.CoordinatesToTile((float)v4, (float)v3, zoom, out p1tx, out p1ty);
                    projection.CoordinatesToTile((float)v2, (float)v1, zoom, out p2tx, out p2ty);
                }
                else if (valueType == 2)
                {
                    projection.CoordinatesToTile((double)v4, (double)v3, zoom, out p1tx, out p1ty);
                    projection.CoordinatesToTile((double)v2, (double)v1, zoom, out p2tx, out p2ty);
                }
                drawPart = true;
            }

            if (drawPart)
            {
                double nx, ny;
                OnlineMapsUtils.NearestPointStrict(cx, cy, p1tx, p1ty, p2tx, p2ty, out nx, out ny);
                double dx = (cx - nx) * OnlineMapsUtils.tileSize;
                double dy = (cy - ny) * OnlineMapsUtils.tileSize;
                double d = dx * dx + dy * dy;
                if (d < sqrW) return true;
            }

            i++;
        }

        return false;
    }

    public override bool Validate()
    {
        return _points != null;
    }
}