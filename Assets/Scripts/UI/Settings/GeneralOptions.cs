using UnityEngine;

public class GeneralOptions : MonoBehaviour
{
    bool isMain = true;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject optionsMenu;

    public void GoBack()
    {
        if (isMain)
        {
            mainMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(true);
        }
        optionsMenu.SetActive(false);
    }

    public void SetPreviousMain()
    {
        isMain = true;
    }

    public void SetPreviousInGame()
    {
        isMain = false;
    }
}
