using UnityEngine;
using UnityEngine.UI;
public class LoadingBarManager : MonoBehaviour
{
    public Image fillImage;
    public float fillduration = 5;

    private float timer = 0f;
    private bool isFilled = false;

    public GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Loading Bar Manager Called");
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f; // Start from empty
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fillImage == null || isFilled) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / fillduration);
        fillImage.fillAmount = progress;

        if (progress >= 1f)
        {
            OpenMainMenu();
        }
    }

    public void OpenMainMenu()
    {
        gameManager.LoadMainMenu();
    }
}
