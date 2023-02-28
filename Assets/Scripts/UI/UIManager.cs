using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    GameObject currentMenu;
    GameObject previousMenu;
    [SerializeField] GameObject gamePausedUI;
    [SerializeField] GameObject sceneManager;
    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string uiActionMap;
    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string escapeControl;
    private InputAction _escape;

    private LinkedList<GameObject> menus;
    
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
        if (sceneManager.GetComponent<SceneLoader>().currentScene != "MainMenu")
        {
            if (gamePausedUI.GetComponent<GameOver>().IsPaused())
            {
                if (gamePausedUI.active)
                {
                    gamePausedUI.GetComponent<GameOver>().Continue();
                }
                //else go back
            }
            else
            {
                gamePausedUI.GetComponent<GameOver>().PauseGame();
            }
        }
    }
}
