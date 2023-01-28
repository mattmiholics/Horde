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
    [OnValueChanged("@NewSelectedTower(towerType, false)")]
    [OnInspectorInit("@NewSelectedTower(towerType, true)")]
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
    [ReadOnly]
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

        tdList = towerParent.GetComponentsInChildren<TowerData>(true).ToList();

        editing = false;
    }

    private void Start()
    {
        //NewSelectedTower(TowerTypeButtons.Instance.serializableButtonGameObject.Values.FirstOrDefault());
    }

    private void OnEnable()
    {
        _playerInput = CameraHandler.Instance.playerInput;

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];
        _rotate = _playerInput.actions[rotateControl];

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

    public void NewSelectedTower(GameObject prefab, bool checkPlaying = false)
    {
        if (checkPlaying && Application.isPlaying)
            return;

        if (selectedTower != null)
        {
            if (Application.isPlaying)
                Destroy(selectedTower);
            else
                DestroyImmediate(selectedTower);
        }

        selectedTower = Application.isPlaying ? Instantiate(prefab, towerProxyParent) :
            #if UNITY_EDITOR 
            (GameObject)PrefabUtility.InstantiatePrefab(prefab, towerProxyParent)
            #else
            null
            #endif
            ;
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
                    tdList = tdList.Where(m_td => m_td != null).ToList(); // Remove null tower datas
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

                        RemoveTower(n_td);
                    }
                }
            }
            else //placing towers section
            {
                if (proxiesActive) //turn proxies back to normal towers if remove button released
                {
                    proxiesActive = false;
                    tdList = tdList.Where(m_td => m_td != null).ToList(); // Remove null tower datas
                    tdList.ForEach(m_td => { m_td.main.SetActive(true); m_td.proxy.SetActive(false); });
                }
                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    selectedTower.SetActive(true);

                    GetTowerVolumeCorners(td, hit, VolumeType.Main, td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2);
                    GetTowerVolumeCorners(td, hit, VolumeType.Main, false, out Vector3 m_basePosition, out Vector3 m_center, out Vector3Int m_corner1, out Vector3Int m_corner2);
                    GetTowerVolumeCorners(td, hit, VolumeType.Ground, td.useChecker, out Vector3 g_basePosition, out Vector3 g_center, out Vector3Int g_corner1, out Vector3Int g_corner2);

                    selectedTower.transform.position = m_basePosition;

                    if (world.GetBlockVolume(g_corner1, g_corner2, false) && world.GetBlockVolume(corner1, corner2, true)) //check ground then empty
                    {
                        if (!materialActive)
                        {
                            renderers.ForEach(r => r.materials = r.materials.Select(m => m = placeMaterial).ToArray()); //show place proxy material
                            materialActive = true;
                        }

                        if (_click.WasPerformedThisFrame() && td.cost <= PlayerStats.Instance.money) //tower placed
                        {
                            StartCoroutine(PlacingTower(selectedTower, m_basePosition, corner1, corner2, m_corner1, m_corner2));
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

    private IEnumerator PlacingTower(GameObject selectedTower, Vector3 position, Vector3Int corner1, Vector3Int corner2, Vector3Int m_corner1, Vector3Int m_corner2)
    {
        //instantiate tower
        GameObject newTower = Instantiate(selectedTower, position, selectedTower.transform.rotation, towerParent);
        TowerData n_td = newTower.GetComponent<TowerData>();
        n_td.main.SetActive(false);
        n_td.proxy.SetActive(false);
        yield return 1;

        //check if path valid
        bool pathValid = EnemyTargetPathChecker.Instance.CheckPathFromTargetToEnemy();

        //spawn tower
        if (pathValid && n_td.cost <= PlayerStats.Instance.money)
        {
            if (td.useChecker)
                world.SetBlockVolume(corner1, corner2, BlockType.Soft_Barrier); // Spawn soft barriers

            if (n_td.placeBarriers)
                world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); //spawn barriers

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

    public void GetTowerVolumeCorners(TowerData towerData, Vector3 position, VolumeType volumeType, bool useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2)
    {
        RaycastHit hit = new RaycastHit();
        hit.point = position;
        GetTowerVolumeCorners(towerData, hit, volumeType, useChecker, out basePosition, out center, out corner1, out corner2);
    }

    public void GetTowerVolumeCorners(TowerData towerData, RaycastHit hit, VolumeType volumeType, bool useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2)
    {
        int rotation = towerData.rotation;

        // Get any offsets
        OffsetRotation(towerData, rotation, out Vector3 offset);

        // Get correct size
        Vector3Int size = useChecker ? towerData.checkerSize : towerData.size;
        int width = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? size.z : size.x;
        int length = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? size.x : size.z;
        size = new Vector3Int(width, size.y, length);

        // Check for position
        hit.point += useChecker ? offset : Vector3.zero;
        basePosition = world.GetBlockPos(hit, true, new int[2] { size.x, size.z }) + new Vector3(0, -0.5f, 0);

        switch (volumeType)
        {
            case VolumeType.Ground:
                // Check valid ground
                center = basePosition + new Vector3(0, -0.5f, 0);
                corner1 = Vector3Int.RoundToInt(new Vector3(center.x - ((size.x / 2f) - 0.5f), center.y, center.z - ((size.z / 2f) - 0.5f)));
                corner2 = Vector3Int.RoundToInt(new Vector3(center.x + ((size.x / 2f) - 0.5f), center.y, center.z + ((size.z / 2f) - 0.5f)));
                break;

            default:
                // Check valid empty space
                center = basePosition + new Vector3(0, size.y / 2f, 0);
                corner1 = Vector3Int.RoundToInt(center - ((Vector3)size / 2f - Vector3.one / 2f));
                corner2 = Vector3Int.RoundToInt(center + ((Vector3)size / 2f - Vector3.one / 2f));
                break;
        }
    }

    public void OffsetRotation(TowerData towerData, float rotation, out Vector3 offset)
    {
        offset = !towerData.useChecker ? Vector3.zero :
                 (rotation >= 225 && rotation < 315) ? new Vector3(-towerData.checkerOffset.z, towerData.checkerOffset.y, -towerData.checkerOffset.x) :
                 (rotation >= 135 && rotation < 225) ? new Vector3(-towerData.checkerOffset.x, towerData.checkerOffset.y, -towerData.checkerOffset.z) :
                 (rotation >= 45 && rotation < 135) ? new Vector3(towerData.checkerOffset.z, towerData.checkerOffset.y, towerData.checkerOffset.x) :
                 towerData.checkerOffset;
    }

    public void RemoveTower(TowerData td)
    {
        try { tdList.Remove(td); }
        catch { }

        OffsetRotation(td, td.rotation, out Vector3 offset);
        // NOTE - I add half of the offset to the position to fix rounding issues
        GetTowerVolumeCorners(td, td.transform.position + (offset / 2) + (Vector3.up / 2), VolumeType.Main, td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2);

        world.SetBlockVolume(corner1, corner2, BlockType.Air);

        if (Application.isPlaying)
            Destroy(td.gameObject);
        else
            DestroyImmediate(td.gameObject);
    }
}

public enum VolumeType
{
    Main,
    Ground,
}
