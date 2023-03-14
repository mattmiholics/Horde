using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class UpgradeManager : MonoBehaviour
{
    private GameObject upgradeMenu;
    private GameObject infoText;
    private int cost;
    private int lvl;
    private string turretType;
    private GameObject target;
    private string upgradeInfo = "";

    public UnityEvent notEnoughGold;

    private bool active = false;

    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string selectionControl;
    private InputAction _selection;

    private static UpgradeManager _instance;
    public LayerMask towerMask;

    TowerData towerDataHovered;
    TowerData towerDataSelected;

    public static UpgradeManager Instance { get { return _instance; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
            Debug.Log("destroyed");
        }
        else
        {
            // Make this the instance
            _instance = this;
        }

        _playerInput = FindObjectOfType<PlayerInput>();
        _selection = _playerInput.actions[selectionControl];
    }

    private void OnEnable()
    {
        _selection.performed += OnClick;
    }

    private void OnDisable()
    {
        _selection.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
        {
            TowerData td = hit.transform.parent.GetComponent<TowerData>();
            if (td.selectable)
            {
                if (active && upgradeMenu != null)
                {
                    upgradeMenu.SetActive(false);
                }
                active = true;

                if (td != towerDataSelected)
                {
                    if (towerDataSelected)
                    {
                        if (towerDataSelected.Main.TryGetComponent<Outline>(out Outline tdoutline))
                        {
                            tdoutline.OutlineColor = Color.blue;
                            tdoutline.enabled = false;
                        }
                        if (towerDataSelected.rangeSphere != null)
                            towerDataSelected.rangeSphere.SetActive(false);
                    }

                    towerDataSelected = td;

                    if (towerDataSelected.Main.TryGetComponent<Outline>(out Outline outline))
                    {
                        outline.OutlineColor = Color.yellow;
                        outline.enabled = true;
                        
                    }
                    if (towerDataSelected.rangeSphere != null)
                        towerDataSelected.rangeSphere.SetActive(true);
                }
            }
            hit.transform.parent.GetComponent<TowerData>().BeginUpgrade();
        }
        else if (!CanvasHitDetector.Instance.IsPointerOverUI() && towerDataSelected)
        {
            if (towerDataSelected.Main.TryGetComponent<Outline>(out Outline outline))
            {
                outline.OutlineColor = Color.blue;
                outline.enabled = false;
            }
            if (active && upgradeMenu != null)
            {
                upgradeMenu.SetActive(false);
            }
            if (towerDataSelected.rangeSphere != null)
                towerDataSelected.rangeSphere.SetActive(false);

            towerDataSelected = null;
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
        {
            TowerData td = hit.transform.GetComponentInParent<TowerData>();
            if (td.selectable)
            {
                if (td != towerDataHovered)
                {
                    if (towerDataHovered && towerDataHovered != towerDataSelected)
                    {
                        if (towerDataHovered.Main.TryGetComponent<Outline>(out Outline tdoutline))
                        {
                            tdoutline.enabled = false;
                        }
                    }

                    towerDataHovered = td;

                    if (towerDataHovered.Main.TryGetComponent<Outline>(out Outline outline))
                    {
                        outline.OutlineColor = Color.blue;
                        outline.enabled = true;
                    }
                }
            }
        }
        else if (towerDataHovered && towerDataHovered != towerDataSelected)
        {
            if (towerDataHovered.Main.TryGetComponent<Outline>(out Outline outline))
            {
                outline.enabled = false;
            }

            towerDataHovered = null;
        }
    }

    public void GetInfo(int cost, GameObject target, int currentLevel, string turretType, GameObject upgradeMenu, GameObject infoText)
    {
        this.cost = cost;
        this.lvl = currentLevel;
        this.turretType = turretType;
        this.target = target;
        this.upgradeMenu = upgradeMenu;
        this.infoText = infoText;
        UpdateInfo();
    }

    public void GetInfo(int cost, GameObject target, int currentLevel, string turretType, GameObject upgradeMenu, GameObject infoText, string upgInfo)
    {
        this.cost = cost;
        this.lvl = currentLevel;
        this.turretType = turretType;
        this.target = target;
        this.upgradeMenu = upgradeMenu;
        this.infoText = infoText;
        this.upgradeInfo = upgInfo;
        UpdateInfo();
    }

    /* 
    public void SetPosition(Vector3 newPos)
    {
        Vector3 updatedPos = new Vector3(newPos.x, newPos.y + 80, newPos.z);
        upgradeMenu.transform.position = updatedPos;
    } */

    private void UpdateInfo()
    {
        infoText.GetComponent<Text>().text = "Turret Type: " + turretType + "\nCurrent Level: " + lvl +"\n"+ upgradeInfo +  "\n<b>Cost To Level-Up: $" + cost + " </b>";
    }

    public void UpgradeTarget()
    {
        if (PlayerStats.Instance.money >= cost && target != null)
        {
            target.GetComponent<TowerData>().Upgrade();
            PlayerStats.Instance.GetComponent<PlayerStats>().money -= cost;
            upgradeMenu.SetActive(false);
        }else if (PlayerStats.Instance.money < cost)
        {
            notEnoughGold.Invoke();
        }
    }

    public void Cancel()
    {
        upgradeMenu.SetActive(false);
        this.active = false;
    }

}
