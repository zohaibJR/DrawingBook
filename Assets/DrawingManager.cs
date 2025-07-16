using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawingManager : MonoBehaviour
{
    public static DrawingManager Instance;

    public RawImage drawingArea;
    public Color currentColor = Color.black;

    public Slider brushSizeSlider;
    public bool isEraser = false;

    private Texture2D tex;
    private RectTransform rectTransform;

    private Vector2? lastDrawPos = null;
    private List<Stroke> strokes = new List<Stroke>();
    private Stroke currentStroke;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        rectTransform = drawingArea.GetComponent<RectTransform>();

        tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        drawingArea.texture = tex;
        drawingArea.material = null;

        ClearCanvas();
    }

    void Update()
    {
        Color drawColor = isEraser ? Color.white : currentColor;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            DrawAtPosition(Input.mousePosition, drawColor);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            FinishStroke();
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            DrawAtPosition(touch.position, drawColor);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                FinishStroke();
        }
        else
        {
            FinishStroke();
        }
#endif
    }

    void DrawAtPosition(Vector2 screenPos, Color drawColor)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out Vector2 localPoint))
        {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            float x = localPoint.x + width / 2f;
            float y = localPoint.y + height / 2f;

            int px = Mathf.Clamp((int)(x / width * tex.width), 0, tex.width - 1);
            int py = Mathf.Clamp((int)(y / height * tex.height), 0, tex.height - 1);

            Vector2 currentPos = new Vector2(px, py);
            int brushSize = Mathf.RoundToInt(brushSizeSlider.value);

            if (lastDrawPos == null)
            {
                lastDrawPos = currentPos;
                currentStroke = new Stroke(drawColor, brushSize);
            }

            DrawLine(lastDrawPos.Value, currentPos, brushSize, drawColor, currentStroke);
            lastDrawPos = currentPos;

            tex.Apply();
        }
    }

    void FinishStroke()
    {
        if (currentStroke != null && currentStroke.points.Count > 0)
        {
            strokes.Add(currentStroke);
        }

        lastDrawPos = null;
        currentStroke = null;
    }

    void DrawLine(Vector2 start, Vector2 end, int radius, Color color, Stroke stroke)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = (int)Mathf.Lerp(start.x, end.x, t);
            int y = (int)Mathf.Lerp(start.y, end.y, t);
            DrawCircle(x, y, radius, color);
            stroke.points.Add(new StrokePoint(x, y));
        }
    }

    void DrawCircle(int x, int y, int radius, Color color)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    int px = Mathf.Clamp(x + i, 0, tex.width - 1);
                    int py = Mathf.Clamp(y + j, 0, tex.height - 1);
                    tex.SetPixel(px, py, color);
                }
            }
        }
    }

    public void SetPaintColor(Color newColor)
    {
        currentColor = newColor;
        isEraser = false; // switch back to brush
    }

    public void ToggleEraser()
    {
        isEraser = !isEraser;
    }

    public void RollbackLastStroke()
    {
        if (strokes.Count > 0)
        {
            strokes.RemoveAt(strokes.Count - 1);
            RedrawAllStrokes();
        }
    }

    void RedrawAllStrokes()
    {
        ClearCanvas();

        foreach (Stroke stroke in strokes)
        {
            foreach (StrokePoint point in stroke.points)
            {
                DrawCircle(point.x, point.y, stroke.brushSize, stroke.color);
            }
        }

        tex.Apply();
    }

    void ClearCanvas()
    {
        Color[] fillColor = new Color[tex.width * tex.height];
        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = Color.white;

        tex.SetPixels(fillColor);
        tex.Apply();
    }
}

public class Stroke
{
    public List<StrokePoint> points = new List<StrokePoint>();
    public Color color;
    public int brushSize;

    public Stroke(Color c, int size)
    {
        color = c;
        brushSize = size;
    }
}

public class StrokePoint
{
    public int x;
    public int y;

    public StrokePoint(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
