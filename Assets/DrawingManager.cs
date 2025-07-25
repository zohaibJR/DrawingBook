using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private Color savedColor;              // Color before slider drag
    private bool isAdjustingBrush = false; // Flag to track slider drag state

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

    // Called from color buttons
    public void SetPaintColor(Color newColor)
    {
        currentColor = newColor;
        isEraser = false;

        if (!isAdjustingBrush)
            savedColor = newColor;
    }

    // Called from eraser button
    public void ToggleEraser()
    {
        isEraser = !isEraser;
    }

    // Called from rollback button
    public void RollbackLastStroke()
    {
        if (strokes.Count > 0)
        {
            strokes.RemoveAt(strokes.Count - 1);
            RedrawAllStrokes();
        }
    }

    // Redraw everything from strokes list
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

    // Clear canvas to white
    void ClearCanvas()
    {
        Color[] fillColor = new Color[tex.width * tex.height];
        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = Color.white;

        tex.SetPixels(fillColor);
        tex.Apply();
    }

    // Call this when slider drag starts (OnPointerDown or OnValueChanged)
    public void OnBrushSizeDragStart()
    {
        if (!isAdjustingBrush)
        {
            savedColor = currentColor;  // Save current color
            currentColor = Color.white; // Temporarily use white
            Debug.Log("White Color ");
            isAdjustingBrush = true;
        }
    }

    // Call this when slider drag ends (via EventTrigger EndDrag)

    public void OnBrushSizeDragEnd()
    {
        if (isAdjustingBrush)
        {
            StartCoroutine(RestoreColorAfterDelay(0.5f)); // Start coroutine
        }
    }

    private IEnumerator RestoreColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentColor = savedColor;  // Restore saved color
        isAdjustingBrush = false;
    }
}

// Stroke class
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

// Stroke point
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
