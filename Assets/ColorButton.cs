using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        Image image = GetComponent<Image>();

        if (button != null && image != null)
        {
            Color buttonColor = image.color;
            button.onClick.AddListener(() => DrawingManager.Instance.SetPaintColor(buttonColor));
        }
    }
}
