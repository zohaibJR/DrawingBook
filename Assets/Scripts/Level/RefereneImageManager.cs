using UnityEngine;

public class RefereneImageManager : MonoBehaviour
{
    public GameObject[] ReferenceImages;

    void Start()
    {
        Debug.Log("Reference Image Script Started");
        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1); // 1 to 5
        ActivateLevel(selectedLevel - 1); // Convert to 0-based index
    }

    void ActivateLevel(int index)
    {
        for (int i = 0; i < ReferenceImages.Length; i++)
        {
            ReferenceImages[i].SetActive(i == index);
        }
    }
}
