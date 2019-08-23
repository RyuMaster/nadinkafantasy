/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that draws a rectangle on the map.
/// </summary>
public class OnlineMapsDrawingRect : OnlineMapsDrawingElement
{
    private static List<Vector3> vertices;
    private static List<Vector3> normals;
    private static List<int> backTriangles;
    private static List<int> borderTriangles;
    private static List<Vector2> uv;
    private static List<Vector2> activePoints;

    private Color _backgroundColor = new Color(1, 1, 1, 0);
    private Color _borderColor = Color.black;
    public float _borderWidth = 1;
    
    private double[] points;
    private double _height = 1;
    private double _width = 1;
    private double _x = 0;
    private double _y = 0;

    /// <summary>
    /// Center point of the rectangle.
    /// </summary>
    public override Vector2 center
    {
        get { return new Vector2((float)(_x + _width / 2), (float)(_y + _height / 2)); }
    }

    [Obsolete("Renamed. Use borderWidth.")]
    public float borderWeight
    {
        get { return borderWidth; }
        set { borderWidth = value; }
    }

    /// <summary>
    /// Background color of the rectangle.
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
    /// Border color of the rectangle.
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

    /// <summary>
    /// Border width of the rectangle.
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
    /// Gets or sets the width of the rectangle. Geographic coordinates.
    /// </summary>
    public double width
    {
        get { return _width; }
        set
        {
            _width = value;
            InitPoints();
        }
    }

    /// <summary>
    /// Gets or sets the height of the rectangle. Geographic coordinates.
    /// </summary>
    public double height
    {
        get { return _height; }
        set
        {
            _height = value;
            InitPoints();
        }
    }

    /// <summary>
    /// Gets or sets the x position of the rectangle. Geographic coordinates.
    /// </summary>
    public double x
    {
        get { return _x; }
        set
        {
            _x = value;
            InitPoints();
        }
    }

    /// <summary>
    /// Gets or sets the y position of the rectangle. Geographic coordinates.
    /// </summary>
    public double y
    {
        get { return _y; }
        set
        {
            _y = value;
            InitPoints();
        }
    }

    /// <summary>
    /// Coordinates of top-left corner.
    /// </summary>
    public Vector2 topLeft
    {
        get
        {
            return new Vector2((float)_x, (float)_y);
        }
        set
        {
            Vector2 br = bottomRight;
            _x = value.x;
            _y = value.y;
            bottomRight = br;
        }
    }

    /// <summary>
    /// Coordinates of top-right corner.
    /// </summary>
    public Vector2 topRight
    {
        get
        {
            return new Vector2((float)(_x + _width), (float)_y);
        }
        set
        {
            double b = _y + _height;
            _width = value.x - _x;
            _y = value.y;
            _height = b - _y;
            InitPoints();
        }
    }

    /// <summary>
    /// Coordinates of bottom-left corner.
    /// </summary>
    public Vector2 bottomLeft
    {
        get
        {
            return new Vector2((float)_x, (float)(_y + _height));
        }
        set
        {
            double r = _x + _width;
            _x = value.x;
            _height = value.y - _y;
            _width = r - _x;
            InitPoints();
        }
    }

    /// <summary>
    /// Coordinates of bottom-right corner.
    /// </summary>
    public Vector2 bottomRight
    {
        get
        {
            return new Vector2((float)(_x + _width), (float)(_y + _height));
        }
        set
        {
            _width = value.x - _x;
            _height = value.y - _y;
            InitPoints();
        }
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="x">Position X. Geographic coordinates.</param>
    /// <param name="y">Position Y. Geographic coordinates.</param>
    /// <param name="width">Width. Geographic coordinates.</param>
    /// <param name="height">Height. Geographic coordinates.</param>
    public OnlineMapsDrawingRect(double x, double y, double width, double height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;

        InitPoints();
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="position">The position of the rectangle. Geographic coordinates.</param>
    /// <param name="size">The size of the rectangle. Geographic coordinates.</param>
    public OnlineMapsDrawingRect(Vector2 position, Vector2 size):this(position.x, position.y, size.x, size.y)
    {
        
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="rect">Rectangle. Geographic coordinates.</param>
    public OnlineMapsDrawingRect(Rect rect): this(rect.x, rect.y, rect.width, rect.height)
    {
        
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="x">Position X. Geographic coordinates.</param>
    /// <param name="y">Position Y. Geographic coordinates.</param>
    /// <param name="width">Width. Geographic coordinates.</param>
    /// <param name="height">Height. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    public OnlineMapsDrawingRect(float x, float y, float width, float height, Color borderColor)
        : this(x, y, width, height)
    {
        this.borderColor = borderColor;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="position">The position of the rectangle. Geographic coordinates.</param>
    /// <param name="size">The size of the rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    public OnlineMapsDrawingRect(Vector2 position, Vector2 size, Color borderColor)
        : this(position.x, position.y, size.x, size.y)
    {
        this.borderColor = borderColor;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="rect">Rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    public OnlineMapsDrawingRect(Rect rect, Color borderColor)
        : this(rect)
    {
        this.borderColor = borderColor;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="x">Position X. Geographic coordinates.</param>
    /// <param name="y">Position Y. Geographic coordinates.</param>
    /// <param name="width">Width. Geographic coordinates.</param>
    /// <param name="height">Height. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    public OnlineMapsDrawingRect(float x, float y, float width, float height, Color borderColor, float borderWidth)
        : this(x, y, width, height, borderColor)
    {
        this.borderWidth = borderWidth;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="position">The position of the rectangle. Geographic coordinates.</param>
    /// <param name="size">The size of the rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    public OnlineMapsDrawingRect(Vector2 position, Vector2 size, Color borderColor, float borderWidth)
        : this(position, size, borderColor)
    {
        this.borderWidth = borderWidth;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="rect">Rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    public OnlineMapsDrawingRect(Rect rect, Color borderColor, float borderWidth)
        : this(rect, borderColor)
    {
        this.borderWidth = borderWidth;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="x">Position X. Geographic coordinates.</param>
    /// <param name="y">Position Y. Geographic coordinates.</param>
    /// <param name="width">Width. Geographic coordinates.</param>
    /// <param name="height">Height. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    /// <param name="backgroundColor">Background color.</param>
    public OnlineMapsDrawingRect(float x, float y, float width, float height, Color borderColor, float borderWidth, Color backgroundColor)
        : this(x, y, width, height, borderColor, borderWidth)
    {
        this.backgroundColor = backgroundColor;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="position">The position of the rectangle. Geographic coordinates.</param>
    /// <param name="size">The size of the rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    /// <param name="backgroundColor">Background color.</param>
    public OnlineMapsDrawingRect(Vector2 position, Vector2 size, Color borderColor, float borderWidth, Color backgroundColor)
        : this(position, size, borderColor, borderWidth)
    {
        this.backgroundColor = backgroundColor;
    }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="rect">Rectangle. Geographic coordinates.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="borderWidth">Border width.</param>
    /// <param name="backgroundColor">Background color.</param>
    public OnlineMapsDrawingRect(Rect rect, Color borderColor, float borderWidth, Color backgroundColor)
        : this(rect, borderColor, borderWidth)
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

        InitMesh(control, "Rect", borderColor, backgroundColor);

        OnlineMaps map = api;
        map.GetCorners(out tlx, out tly, out brx, out bry);

        List<Vector2> localPoints = GetLocalPoints(points, true, false);

        Rect rect1 = new Rect(localPoints[0].x, localPoints[2].y, localPoints[2].x - localPoints[0].x, localPoints[0].y - localPoints[2].y);
        Rect rect2 = new Rect(0, 0, map.tilesetSize.x, map.tilesetSize.y);

        bool ignoreLeft = false;
        bool ignoreRight = false;
        bool ignoreTop = false;
        bool ignoreBottom = false;
        int countIgnore = 0;

        if (checkMapBoundaries)
        {
            if (!rect2.Overlaps(rect1))
            {
                if (active) active = false;
                return;
            }
            if (!active) active = true;

            for (int i = 0; i < localPoints.Count; i++)
            {
                Vector2 point = localPoints[i];
                if (point.x < 0)
                {
                    point.x = 0;
                    if (!ignoreLeft) countIgnore++;
                    ignoreLeft = true;
                }
                if (point.y < 0)
                {
                    point.y = 0;
                    if (!ignoreTop) countIgnore++;
                    ignoreTop = true;
                }
                if (point.x > map.tilesetSize.x)
                {
                    point.x = map.tilesetSize.x;
                    if (!ignoreRight) countIgnore++;
                    ignoreRight = true;
                }
                if (point.y > map.tilesetSize.y)
                {
                    point.y = map.tilesetSize.y;
                    if (!ignoreBottom) countIgnore++;
                    ignoreBottom = true;
                }

                localPoints[i] = point;
            }
        }
        

        if (vertices == null) vertices = new List<Vector3>(16);
        else vertices.Clear();

        if (normals == null) normals = new List<Vector3>(16);
        else normals.Clear();

        if (backTriangles == null) backTriangles = new List<int>(6);
        else backTriangles.Clear();

        if (borderTriangles == null) borderTriangles = new List<int>();
        else borderTriangles.Clear();

        if (uv == null) uv = new List<Vector2>(16);
        else uv.Clear();

        vertices.Add(new Vector3(-localPoints[0].x, -0.05f, localPoints[0].y));
        vertices.Add(new Vector3(-localPoints[1].x, -0.05f, localPoints[1].y));
        vertices.Add(new Vector3(-localPoints[2].x, -0.05f, localPoints[2].y));
        vertices.Add(new Vector3(-localPoints[3].x, -0.05f, localPoints[3].y));

        if (!ignoreTop)
        {
            vertices[2] += new Vector3(0, 0, borderWidth);
            vertices[3] += new Vector3(0, 0, borderWidth);
        }

        if (!ignoreBottom)
        {
            vertices[0] -= new Vector3(0, 0, borderWidth);
            vertices[1] -= new Vector3(0, 0, borderWidth);
        }

        if (!ignoreLeft)
        {
            vertices[0] -= new Vector3(borderWidth, 0, 0);
            vertices[3] -= new Vector3(borderWidth, 0, 0);
        }

        if (!ignoreRight)
        {
            vertices[1] += new Vector3(borderWidth, 0, 0);
            vertices[2] += new Vector3(borderWidth, 0, 0);
        }

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(1, 1));
        uv.Add(new Vector2(1, 0));

        backTriangles.Add(0);
        backTriangles.Add(2);
        backTriangles.Add(1);
        backTriangles.Add(0);
        backTriangles.Add(3);
        backTriangles.Add(2);

        if (activePoints == null) activePoints = new List<Vector2>();
        else activePoints.Clear();

        if (countIgnore == 0)
        {
            activePoints.Add(localPoints[0] + new Vector2(borderWidth, 0));
            activePoints.Add(localPoints[1]);
            activePoints.Add(localPoints[2]);
            activePoints.Add(localPoints[3]);
            activePoints.Add(localPoints[0] + new Vector2(0, borderWidth));
            DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
        }
        else if (countIgnore == 1)
        {
            int off = 0;
            if (ignoreTop) off = 3;
            else if (ignoreRight) off = 2;
            else if (ignoreBottom) off = 1;

            for (int i = 0; i < 4; i++)
            {
                int ci = i + off;
                if (ci > 3) ci -= 4;
                activePoints.Add(localPoints[ci]);
            }
            DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
        }
        else if (countIgnore == 2)
        {
            if (ignoreBottom && ignoreTop)
            {
                activePoints.Add(localPoints[1]);
                activePoints.Add(localPoints[2]);
                DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
                activePoints.Add(localPoints[3]);
                activePoints.Add(localPoints[0]);
                DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
            }
            else if (ignoreLeft && ignoreRight)
            {
                activePoints.Add(localPoints[0]);
                activePoints.Add(localPoints[1]);
                DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
                activePoints.Add(localPoints[2]);
                activePoints.Add(localPoints[3]);
                DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
            }
            else
            {
                DrawActivePointsCI3(control, ignoreTop, ignoreRight, ignoreBottom, activePoints, localPoints, ref vertices, ref normals, ref borderTriangles, ref uv);
            }
        }
        else if (countIgnore == 3)
        {
            DrawActivePointsCI3(control, ignoreTop, ignoreRight, ignoreBottom, activePoints, localPoints, ref vertices, ref normals, ref borderTriangles, ref uv);
        }
        else if (countIgnore == 4)
        {
            DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
        }

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
        mesh.subMeshCount = 2;

        mesh.SetTriangles(borderTriangles.ToArray(), 0);
        mesh.SetTriangles(backTriangles.ToArray(), 1);

        UpdateMaterialsQuote(control, index);
    }

    private void DrawActivePointsCI3(OnlineMapsTileSetControl control, bool ignoreTop, bool ignoreRight, bool ignoreBottom, List<Vector2> activePoints,
        List<Vector2> localPoints, ref List<Vector3> vertices, ref List<Vector3> normals, ref List<int> borderTriangles, ref List<Vector2> uv)
    {
        int off = 0;

        if (ignoreTop) off = 3;
        else if (ignoreRight) off = 2;
        else if (ignoreBottom) off = 1;

        for (int i = 0; i < 2; i++)
        {
            int ci = i + off;
            if (ci > 3) ci -= 4;
            activePoints.Add(localPoints[ci]);
        }
        DrawActivePoints(control, ref activePoints, ref vertices, ref normals, ref borderTriangles, ref uv, borderWidth);
    }

    public override bool HitTest(Vector2 positionLngLat, int zoom)
    {
        if (positionLngLat.x < x || positionLngLat.x > x + width) return false;
        if (positionLngLat.y < y || positionLngLat.y > y + height) return false;
        return true;
    }

    private void InitPoints()
    {
        points = new [] 
        {
            _x, _y,
            _x + _width, _y,
            _x + _width, _y + _height,
            _x, _y + _height
        };
        OnlineMaps.instance.needRedraw = true;
    }

    protected override void DisposeLate()
    {
        base.DisposeLate();

        points = null;
    }

    public override bool Validate()
    {
        return points != null;
    }
}