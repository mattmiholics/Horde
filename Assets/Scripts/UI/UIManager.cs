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
    [SerializeField] GameObject audio;
    [SerializeField] GameObject graphics;
    [SerializeField] GameObject display;
    [SerializeField] GameObject controls;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject rules;




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
                if (gamePausedUI.active)
                {
                    gamePausedUI.GetComponent<GameOver>().Continue();
                }
                    if (mainOptions.active)
                    {
                        mainOptions.SetActive(false);
                        gamePausedUI.SetActive(true);
                    }
                    else if (audio.active || graphics.active || display.active || controls.active)
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
            if (generalOptions.active)
            {
                if (mainOptions.active)
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
            if (levelSelect.active)
            {
                levelSelect.SetActive(false);
                mainMenu.SetActive(true);
            }
            if (rules.active)
            {
                rules.SetActive(false);
                mainMenu.SetActive(true);
            }
        }
    }
}
