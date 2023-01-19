using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void Select(string levelName)
    {
        SceneLoader.Instance.LoadWorldScene(levelName, true);
    }
}
