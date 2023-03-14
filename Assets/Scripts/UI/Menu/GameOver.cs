using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public Text roundsText;
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string mainMenu;

    private void OnEnable()
    {
        SceneLoader.SceneLoaded += RefreshText;
    }

    private void OnDisable()
    {
        SceneLoader.SceneLoaded -= RefreshText;
    }

    public void RefreshText()
    {
        if (roundsText != null && PlayerStats.Instance != null)
            roundsText.text = PlayerStats.Instance.rounds.ToString();
    }

    public void Retry ()
    {
        SceneLoader.Instance.LoadWorldScene(SceneLoader.Instance.currentScene, true);
    }

    public void Menu ()
    {
        SceneLoader.Instance.Load(mainMenu, true);
    }

    public void Continue()
    {
        GameObject UI = GameObject.Find("GamePausedUI");
        UI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        GameObject UI = FindInActiveObjectByName("GamePausedUI");
        UI.SetActive(true);
    }

    public bool IsPaused()
    {
        if (Time.timeScale == 0f)
        {
            return true;
        }
        else
            return false;
    }

    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }
}

