using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject gamePausedUI;
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject generalOptions;
    [SerializeField] GameObject mainOptions;
#pragma warning disable 0108
    [SerializeField] GameObject audio;
#pragma warning restore 0108
    [SerializeField] GameObject graphics;
    [SerializeField] GameObject display;
    [SerializeField] GameObject controls;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject rules;
    [SerializeField] GameObject nextLevel;

    /*This is a quick and dirty way for escape functionality. 
     * While it does work, should this project be continued beyond this course 
     * I would *HIGHLY* recommend redoing this system to be more expandable. 
     * But for a fast solution for QoL, this is here. 
     */


    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string uiActionMap;
    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string escapeControl;
    private InputAction _escape;
    
    void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _escape = _playerInput.actions[escapeControl];
    }

    private void OnEnable()
    {
        _escape.performed += OnCancel;
    }

    private void OnDisable()
    {
        _escape.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext callbackContext)
    {
        if (sceneManager.GetComponent<SceneLoader>().currentScene != "MainMenu") //In a level
        {
            if (gamePausedUI.GetComponent<GameOver>().IsPaused())
            {
                if (gamePausedUI.activeSelf)
                {
                    gamePausedUI.GetComponent<GameOver>().Continue();
                }
                    if (mainOptions.activeSelf)
                    {
                        mainOptions.SetActive(false);
                        gamePausedUI.SetActive(true);
                    }
                    else if (audio.activeSelf || graphics.activeSelf || display.activeSelf || controls.activeSelf)
                    {
                        audio.SetActive(false);
                        graphics.SetActive(false);
                        display.SetActive(false);
                        controls.SetActive(false);
                        mainOptions.SetActive(true);
                    }
            }
            else
            {
                gamePausedUI.GetComponent<GameOver>().PauseGame();
            }
        }
        else //On the main menu
        {
            if (generalOptions.activeSelf)
            {
                if (mainOptions.activeSelf)
                {
                    mainOptions.SetActive(false);
                    generalOptions.SetActive(false);
                    mainMenu.SetActive(true);
                }
                else
                {
                    audio.SetActive(false);
                    graphics.SetActive(false);
                    display.SetActive(false);
                    controls.SetActive(false);
                    generalOptions.SetActive(true);
                    mainOptions.SetActive(true);
                }
            }
            if (levelSelect.activeSelf)
            {
                levelSelect.SetActive(false);
                mainMenu.SetActive(true);
            }
            if (rules.activeSelf)
            {
                rules.SetActive(false);
                mainMenu.SetActive(true);
            }
        }
    }
}
