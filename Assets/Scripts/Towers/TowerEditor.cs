using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEditor;
using Sirenix.Utilities;

public class TowerEditor : MonoBehaviour
{
    public World world;
    [SerializeField]
    public LayerMask groundMask;
    [SerializeField]
    public LayerMask towerMask;
    [Space]
    public Transform towerParent;
    public Transform towerProxyParent;
    public Transform permanentTowerParent;

    [Space]
    public Material placeMaterial;
    public Material removeMaterial;

    [Space]
    [Required]
    public TowerDataManager towerDataManager;
    [OnValueChanged("@NewSelectedTower(towerType)")]
    [ValueDropdown("GetTowerTypes", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, CopyValues = false, OnlyChangeValueOnConfirm = true)]
    public GameObject towerType;

    private IEnumerable GetTowerTypes()
    {
        return towerDataManager.idTowerPrefab.Select(keyval => new ValueDropdownItem(keyval.Value.GetComponent<TowerData>().type, keyval.Value)); // Just pull the string name because otherwise if we grab a copy it won't work
    }

    private PlayerInput _playerInput;
    [Header("Controls")]
    [PropertySpace(5, 5)]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string editingActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string clickControl;
    private InputAction _click;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string removeControl;
    private InputAction _remove;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rotateControl;
    private InputAction _rotate;

    [Space]
    [Header("Debug")]
    [OnInspectorDispose("DisableSelectedTower")]
    public GameObject selectedTower;

    private void DisableSelectedTower()
    {
        if (selectedTower != null)
            selectedTower.SetActive(false);
    }

    [HideInInspector]
    public TowerData td;

    [HideInInspector]
    public List<TowerData> tdList;
    [HideInInspector]
    public Renderer[] renderers;
    [HideInInspector]
    public bool proxiesActive;
    [HideInInspector]
    public bool materialActive;

    [HideInInspector]
    public bool editing;

    private Coroutine editCoroutine;

    private static TowerEditor _instance;
    public static TowerEditor Instance { get { return _instance; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }

        _playerInput = CameraHandler.Instance.playerInput;

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];
        _rotate = _playerInput.actions[rotateControl];

        tdList = towerParent.GetComponentsInChildren<TowerData>(true).ToList();

        editing = false;
    }


    private void OnEnable()
    {
        _rotate.performed += RotateTower;
    }

    private void OnDisable()
    {
        _rotate.performed -= RotateTower;
    }

    private void RotateTower(InputAction.CallbackContext ctx)
    {
        td.rotation = td.rotation > 359 ? td.rotation - 270 : td.rotation + 90;
        selectedTower.transform.localEulerAngles = new Vector3(0, td.rotation, 0);
    }

    public void EnableTowerEditing()
    {
        editing = true;
        editCoroutine = StartCoroutine(Editing());
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = new InputActionMap[] { _playerInput.actions.FindActionMap(editingActionMap, true) };
        }
        else
            _playerInput.actions.FindActionMap(editingActionMap, true).Enable();
    }
    public void DisableTowerEditing()
    {
        editing = false;
        if (editCoroutine != null)
            StopCoroutine(editCoroutine);
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = EditButtons.Instance.popupHandler.disabledActionMaps;
        }
        else
            _playerInput.actions.FindActionMap(editingActionMap, true).Disable();

        if (proxiesActive)
        {
            proxiesActive = false;
            foreach (TowerData m_td in tdList)
            {
                m_td.main.SetActive(true);
                m_td.proxy.SetActive(false);
            }
        }
    }

    public void NewSelectedTower(GameObject prefab)
    {
        if (selectedTower != null)
        {
            if (Application.isPlaying)
                Destroy(selectedTower);
            else
                DestroyImmediate(selectedTower);
        }

        selectedTower = Application.isPlaying ? Instantiate(prefab, towerProxyParent) : (GameObject)PrefabUtility.InstantiatePrefab(prefab, towerProxyParent);
        td = selectedTower.GetComponent<TowerData>();

        td.main.SetActive(false);
        td.proxy.SetActive(true);
        selectedTower.SetActive(false);
        materialActive = false;

        renderers = td.proxy.GetComponentsInChildren<Renderer>(true);
    }

    private IEnumerator Editing()
    {
        for (; ; )
        {
            if (selectedTower == null)
            {
                yield return null;
                continue;
            }

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (_remove.IsPressed()) //removing towers section
            {
                selectedTower.SetActive(false);

                if (!proxiesActive) // acitvate proxies
                {
                    proxiesActive = true;
                    tdList.ForEach(m_td => { m_td.main.SetActive(false); m_td.proxy.SetActive(true); });
                }

                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
                {
                    //tower removal
                    if (_click.WasPerformedThisFrame())
                    {
                        TowerData n_td = hit.transform.GetComponentInParent<TowerData>(true);

                        //reimburse player
                        PlayerStats.Instance.money += n_td.cost;

                        tdList.Remove(n_td);

                        int rotation = n_td.rotation;
                        int width = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? n_td.size.z : n_td.size.x;
                        int length = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? n_td.size.x : n_td.size.z;
                        Vector3Int size = new Vector3Int(width, n_td.size.y, length);

                        //fill with air/remove barriers
                        Vector3 center = n_td.transform.position + new Vector3(0, size.y / 2f, 0);
                        Vector3Int corner1 = Vector3Int.RoundToInt(center - ((Vector3)size / 2f - Vector3.one / 2f));
                        Vector3Int corner2 = Vector3Int.RoundToInt(center + ((Vector3)size / 2f - Vector3.one / 2f));
                        world.SetBlockVolume(corner1, corner2, BlockType.Air);

                        Destroy(n_td.gameObject);
                    }
                }
            }
            else //placing towers section
            {
                if (proxiesActive) //turn proxies back to normal towers if remove button released
                {
                    proxiesActive = false;
                    tdList.ForEach(m_td => { m_td.main.SetActive(true); m_td.proxy.SetActive(false); });
                }
                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    selectedTower.SetActive(true);
                    int rotation = td.rotation;
                    int width = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? td.size.z : td.size.x;
                    int length = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? td.size.x : td.size.z;
                    Vector3Int size = new Vector3Int(width, td.size.y, length);
                    //check for valid placement
                    Vector3 pos = world.GetBlockPos(hit, true, new int[2] { size.x, size.z }) + new Vector3(0, -0.5f, 0);
                    selectedTower.transform.position = pos;
                    //check valid ground first
                    Vector3 g_center = pos + new Vector3(0, -0.5f, 0);
                    Vector3Int g_corner1 = Vector3Int.RoundToInt(new Vector3(g_center.x - ((size.x / 2f) - 0.5f), g_center.y, g_center.z - ((size.z / 2f) - 0.5f)));
                    Vector3Int g_corner2 = Vector3Int.RoundToInt(new Vector3(g_center.x + ((size.x / 2f) - 0.5f), g_center.y, g_center.z + ((size.z / 2f) - 0.5f)));
                    //check valid empty space
                    Vector3 center = pos + new Vector3(0, size.y / 2f, 0);
                    Vector3Int corner1 = Vector3Int.RoundToInt(center - ((Vector3)size / 2f - Vector3.one / 2f));
                    Vector3Int corner2 = Vector3Int.RoundToInt(center + ((Vector3)size / 2f - Vector3.one / 2f));

                    if (world.GetBlockVolume(g_corner1, g_corner2, false) && world.GetBlockVolume(corner1, corner2, true)) //check ground then empty
                    {
                        if (!materialActive)
                        {
                            renderers.ForEach(r => r.materials = r.materials.Select(m => m = placeMaterial).ToArray()); //show place proxy material
                            materialActive = true;
                        }

                        if (_click.WasPerformedThisFrame() && td.cost <= PlayerStats.Instance.money) //tower placed
                        {
                            StartCoroutine(PlacingTower(selectedTower, corner1, corner2, pos));
                        }
                    }
                    else //if space is invalid show red proxy material
                    {
                        if (materialActive)
                        {
                            renderers.ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray()); //show remove proxy material
                            materialActive = false;
                        }
                    }
                }
                else
                    selectedTower.SetActive(false);
            }

            yield return null;
        }
    }

    private IEnumerator PlacingTower(GameObject selectedTower, Vector3Int corner1, Vector3Int corner2, Vector3 pos)
    {
        //instantiate tower
        GameObject newTower = Instantiate(selectedTower, pos, selectedTower.transform.rotation, towerParent);
        TowerData n_td = newTower.GetComponent<TowerData>();
        n_td.main.SetActive(false);
        n_td.proxy.SetActive(false);
        yield return 1;

        //check if path valid
        bool pathValid = EnemyTargetPathChecker.Instance.CheckPathFromTargetToEnemy();

        //spawn tower
        if (pathValid && n_td.cost <= PlayerStats.Instance.money)
        {
            if (n_td.placeBarriers)
                world.SetBlockVolume(corner1, corner2, BlockType.Barrier); //spawn barriers

            tdList.Add(n_td);
            n_td.proxy.GetComponentsInChildren<Renderer>().ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray());
            n_td.main.SetActive(true);
            n_td.proxy.SetActive(false);

            //remove money from player
            PlayerStats.Instance.money -= n_td.cost;
        }
        else //otherwise remove tower
        {
            Destroy(newTower);
        }
    }
}
