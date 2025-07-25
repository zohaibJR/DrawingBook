using UnityEngine;

public class DrawingButtonManager : MonoBehaviour
{
    void Start()
    {
        if (!PlayerPrefs.HasKey("DrawingImage"))
        {
            PlayerPrefs.SetString("DrawingImage", " "); // Set default DrawingImage instead of SelectedLevel
        }
    }

    public void CornButtonClicked()
    {
        PlayerPrefs.SetString("DrawingImage", "Corn");
        Debug.Log("Corn button clicked, DrawingImage set to Corn");
    }
}
