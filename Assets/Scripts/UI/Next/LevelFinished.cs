using UnityEngine;

public class LevelFinished : MonoBehaviour
{
    public GameObject nextLevelButton;
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string mainMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextLevel()
    {
        if (Application.CanStreamedLevelBeLoaded(LevelInfo.Instance.nextLevelString))
            SceneLoader.Instance.LoadWorldScene(LevelInfo.Instance.nextLevelString, true);
    }

    public void Menu()
    {
        SceneLoader.Instance.LoadWorldScene(mainMenu, true);
    }

    public void Continue()
    {
        GameObject UI = GameObject.Find("GamePausedUI");
        UI.SetActive(false);
        Time.timeScale = 1f;
    }
}
