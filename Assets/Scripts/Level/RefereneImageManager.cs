using UnityEngine;

public class RefereneImageManager : MonoBehaviour
{
    public GameObject[] ReferenceImages;

    void Start()
    {
        Debug.Log("Reference Image Script Started");

        string selectedImage = PlayerPrefs.GetString("DrawingImage", " "); // Default is blank
        Debug.Log("Selected DrawingImage: " + selectedImage);

        if (selectedImage == "Corn")
        {
            ActivateImageByIndex(0); // Activate 1st index
        }
        else
        {
            Debug.LogWarning("No matching DrawingImage found: " + selectedImage);
            DeactivateAllImages(); // Optional: deactivate all if no match
        }
    }

    void ActivateImageByIndex(int index)
    {
        for (int i = 0; i < ReferenceImages.Length; i++)
        {
            ReferenceImages[i].SetActive(i == index);
        }
    }

    void DeactivateAllImages()
    {
        for (int i = 0; i < ReferenceImages.Length; i++)
        {
            ReferenceImages[i].SetActive(false);
        }
    }
}
