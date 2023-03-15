using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEditor;
using Sirenix.Utilities;
using System;
using UnityEngine.Events;

public class TowerEditor : MonoBehaviour
{
    public World world;
    [SerializeField]
    public LayerMask groundMask;
    [SerializeField]
    public LayerMask towerMask;
    [SerializeField]
    public LayerMask unitMask;
    [Space]
    public Transform towerParent;
    public Transform towerProxyParent;
    public Transform permanentTowerParent;

    [Space]
    public Material placeMaterial;
    public Material removeMaterial;
    [ColorUsage(false, true)]
    public Color unableToEditColor;

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

    [FoldoutGroup("Events")]
    public UnityEvent towerPlaced;
    [FoldoutGroup("Events")]
    public UnityEvent towerRemoved;
    [FoldoutGroup("Events")]
    public UnityEvent unableToEdit;

    [Space]
    [Header("Debug")]
    [OnInspectorDispose("DisableSelectedTower")]
    [ReadOnly]
    public GameObject selectedTower;
    [HideInInspector]
    public GameObject selectedTowerPrefab;

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
    private Coroutine unableToPlaceCoroutine;

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

        if (CameraHandler.Instance != null)
            DelayedStart();
    }

    private void OnEnable()
    {
        if (CameraHandler.Instance == null)
            CameraHandler.SingletonInstanced += DelayedStart;  
    }

    private void OnDisable()
    {
        CameraHandler.SingletonInstanced -= DelayedStart;
    }

    private void OnDestroy()
    {
        _rotate.performed -= RotateTower;
    }

    private void DelayedStart()
    {
        _playerInput = CameraHandler.Instance.playerInput;

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];
        _rotate = _playerInput.actions[rotateControl];

        _rotate.performed += RotateTower;

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
                m_td.Main.SetActive(true);
                m_td.Proxy.SetActive(false);
            }
        }

        selectedTower.SetActive(false);
    }

    public void NewSelectedTower(GameObject prefab, bool checkPlaying = false)
    {
        if (checkPlaying && Application.isPlaying)
            return;

        if (selectedTower.GetComponent<TowerData>().rangeSphere != null)
            selectedTower.GetComponent<TowerData>().rangeSphere.SetActive(false);
        SmartDestroy(selectedTower);

        selectedTower = SmartInstantiate(prefab, Vector3.zero, Quaternion.identity, towerProxyParent);
        selectedTowerPrefab = prefab;

        td = selectedTower.GetComponent<TowerData>();

        td.Main.SetActive(false);
        td.Proxy.SetActive(true);
        if (td.rangeSphere != null)
            td.rangeSphere.SetActive(true);
        selectedTower.SetActive(false);
        materialActive = false;

        renderers = td.Proxy.GetComponentsInChildren<Renderer>(true);
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
                    tdList.ForEach(m_td => { m_td.Main.SetActive(false); m_td.Proxy.SetActive(true); });
                }

                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
                {
                    //tower removal
                    if (_click.WasPerformedThisFrame())
                    {
                        TowerData n_td = hit.transform.GetComponentInParent<TowerData>(true);

                        //reimburse player
                        PlayerStats.Instance.money += n_td.cost;

                        TowerHelper.RemoveTower(this, n_td);

                        towerRemoved?.Invoke();
                    }
                }
            }
            else //placing towers section
            {
                if (proxiesActive) //turn proxies back to normal towers if remove button released
                {
                    proxiesActive = false;
                    tdList = tdList.Where(m_td => m_td != null).ToList(); // Remove null tower datas
                    tdList.ForEach(m_td => { m_td.Main.SetActive(true); m_td.Proxy.SetActive(false); });
                }
                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    selectedTower.SetActive(true);

                    TowerHelper.GetTowerVolumeCorners(world, td, hit, VolumeType.Main, td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2);
                    TowerHelper.GetTowerVolumeCorners(world, td, hit, VolumeType.Main, false, out Vector3 m_basePosition, out Vector3 m_center, out Vector3Int m_corner1, out Vector3Int m_corner2);
                    TowerHelper.GetTowerVolumeCorners(world, td, hit, VolumeType.Ground, td.useChecker, out Vector3 g_basePosition, out Vector3 g_center, out Vector3Int g_corner1, out Vector3Int g_corner2);

                    selectedTower.transform.position = m_basePosition;

                    if (world.GetBlockVolume(g_corner1, g_corner2, false) 
                        && world.GetBlockVolume(corner1, corner2, true) // Check ground then empty
                        && !Physics.CheckBox(m_center, new Vector3(Mathf.Abs(m_center.x - m_corner1.x), Mathf.Abs(m_center.y - m_corner1.y), Mathf.Abs(m_center.z - m_corner1.z)) + Vector3.one / 2, Quaternion.identity, unitMask)) 
                    {
                        if (!materialActive)
                        {
                            if (unableToPlaceCoroutine != null)
                                StopCoroutine(unableToPlaceCoroutine);

                            renderers.ForEach(r => r.materials = r.materials.Select(m => m = placeMaterial).ToArray()); //show place proxy material
                            materialActive = true;
                        }

                        if (_click.WasPerformedThisFrame() && td.cost <= PlayerStats.Instance.money) //tower placed
                        {
                            if (td.placeBarriers)
                                world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); // Spawn barriers

                            if (EnemyPathChecker.Instance != null && EnemyPathChecker.Instance.PathExists()) // Instantiate tower
                            {
                                GameObject newTower = Instantiate(selectedTower, m_basePosition, selectedTower.transform.rotation, towerParent);
                                TowerData n_td = newTower.GetComponent<TowerData>();

                                if (n_td.useChecker)
                                    world.SetBlockVolume(corner1, corner2, BlockType.Soft_Barrier); // Spawn soft barriers
                                if (n_td.placeBarriers)
                                    world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); // Spawn barriers overriding soft barriers

                                tdList.Add(n_td);
                                n_td.Proxy.GetComponentsInChildren<Renderer>().ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray());
                                n_td.Main.SetActive(true);
                                n_td.Proxy.SetActive(false);

                                //remove money from player
                                PlayerStats.Instance.money -= n_td.cost;
                                towerPlaced?.Invoke();
                            }
                            else
                            {
                                if (td.placeBarriers)
                                    world.SetBlockVolume(m_corner1, m_corner2, BlockType.Air); // Return to air
                                UnableToEdit();
                            }
                        }
                        else if (_click.WasPerformedThisFrame())
                            UnableToEdit();
                    }
                    else //if space is invalid show red proxy material
                    {
                        if (materialActive)
                        {
                            if (unableToPlaceCoroutine != null)
                                StopCoroutine(unableToPlaceCoroutine);

                            renderers.ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray()); //show remove proxy material
                            materialActive = false;
                        }
                        if (_click.WasPerformedThisFrame()) //tower tried to be placed in invalid spot
                            UnableToEdit(false);
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
        n_td.Main.SetActive(false);
        n_td.Proxy.SetActive(false);

        if (td.useChecker)
            world.SetBlockVolume(corner1, corner2, BlockType.Soft_Barrier); // Spawn soft barriers

        if (n_td.placeBarriers)
            world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); //spawn barriers

        tdList.Add(n_td);
        n_td.Proxy.GetComponentsInChildren<Renderer>().ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray());
        n_td.Main.SetActive(true);
        n_td.Proxy.SetActive(false);

        //remove money from player
        PlayerStats.Instance.money -= n_td.cost;
        towerPlaced?.Invoke();

        yield return null;
        /*yield return 1;

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
            n_td.Proxy.GetComponentsInChildren<Renderer>().ForEach(r => r.materials = r.materials.Select(m => m = removeMaterial).ToArray());
            n_td.Main.SetActive(true);
            n_td.Proxy.SetActive(false);

            //remove money from player
            PlayerStats.Instance.money -= n_td.cost;
        }
        else //otherwise remove tower
        {
            Destroy(newTower);
        }*/
    }

    public void UnableToEdit(bool place = true)
    {
        unableToEdit?.Invoke();

        if (unableToPlaceCoroutine != null)
            StopCoroutine(unableToPlaceCoroutine);
        unableToPlaceCoroutine = StartCoroutine(UnableToEditAnimation(selectedTower, place ? placeMaterial.GetColor("_Wireframe_Color") : removeMaterial.GetColor("_Wireframe_Color")));
    }

    private IEnumerator UnableToEditAnimation(GameObject proxy, Color origColor)
    {
        Renderer[] rs = proxy.GetComponentsInChildren<Renderer>().ToArray();

        float currentTime = 0;

        while (currentTime <= 0.5f)
        {
            rs.ForEach(r => r.materials.ForEach(m => m.SetColor("_Wireframe_Color", Color.Lerp(unableToEditColor, origColor, currentTime / 0.5f))));

            currentTime += Time.deltaTime;

            yield return null;
        }

        rs.ForEach(r => r.materials.ForEach(m => m.SetColor("_Wireframe_Color", origColor)));
    }

    public void SmartDestroy(GameObject gameObject)
    {
        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }

    public GameObject SmartInstantiate(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (Application.isPlaying)
            return Instantiate(gameObject, position, rotation, parent);
        else
        {
#if UNITY_EDITOR
            GameObject prefabGameObject = (GameObject)PrefabUtility.InstantiatePrefab(gameObject, parent);
            prefabGameObject.transform.position = position;
            prefabGameObject.transform.rotation = rotation;
            return prefabGameObject;
#else
            return null;
#endif
        }
    }
}

public enum VolumeType
{
    Main,
    Ground,
}
