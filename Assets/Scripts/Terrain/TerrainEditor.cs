using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(World))]
public class TerrainEditor : MonoBehaviour
{
    [SerializeField]
    public LayerMask groundMask;
    [SerializeField]
    private List<BlockType> blockModifyBlacklist = new BlockType[] { BlockType.Bedrock, BlockType.Barrier }.ToList();
    public BlockType blockType = BlockType.Dirt;
    [HideInInspector]
    public BlockType playModeBlockType;

    [Space]
    [OnInspectorDispose("DisableBlockProxy")]
    public GameObject placeProxy;
    public GameObject removeProxy;
    public GameObject modifiabilityProxy;
    [ColorUsage(false, true)]
    public Color unableToEditColor;

    private Color placeProxyColor;
    private Color removeProxyColor;

    private void DisableBlockProxy()
    {
        placeProxy.SetActive(false);
        removeProxy.SetActive(false);
        modifiabilityProxy.SetActive(false);
    }

    [Space]
    public int cost;
    [ReadOnly]
    public float costMultiplier;

    private PlayerInput _playerInput;
    [Header("Controls")]
    [PropertySpace(5, 5)]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string editingActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string clickControl;
    private InputAction _click;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string removeControl;
    private InputAction _remove;

    [HideInInspector]
    public World world;
    [HideInInspector]
    public bool editing;

    private Coroutine editCoroutine;
    private Coroutine unableToPlaceCoroutine;
    private Coroutine unableToRemoveCoroutine;

    private static TerrainEditor _instance;
    public static TerrainEditor Instance { get { return _instance; } }

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

        placeProxy.transform.position = Vector3.zero;
        removeProxy.transform.position = Vector3.zero;

        placeProxyColor = placeProxy.GetComponent<Renderer>().material.GetColor("_EmissionColor");
        removeProxyColor = removeProxy.GetComponent<Renderer>().material.GetColor("_EmissionColor");

        placeProxy.SetActive(false);
        removeProxy.SetActive(false);

        editing = false;
    }

    public void ModifyTerrain(RaycastHit hit, BlockType blockType = BlockType.Air, bool place = false)
    {
        if (world.GetBlock(hit, place) != BlockType.Bedrock)
            world.SetBlock(hit, blockType, place);
    }

    public void ModifyModifiabilityEditor(RaycastHit hit, bool unlock)
    {
        world.SetModifiability(hit, unlock);
    }

    public void EnableTerrainEditing()
    {
        editing = true;
        editCoroutine = StartCoroutine(Editing());
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = new InputActionMap[] { _playerInput.actions.FindActionMap(editingActionMap, true) };
        }
        else
        {
            _playerInput.actions.FindActionMap(editingActionMap, true).Enable();
        }
    }
    public void DisableTerrainEditing()
    {
        editing = false;
        if (editCoroutine != null)
            StopCoroutine(editCoroutine);
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = EditButtons.Instance.popupHandler.disabledActionMaps;
        }
        else
        {
            _playerInput.actions.FindActionMap(editingActionMap, true).Disable();
        }

        placeProxy.SetActive(false);
        removeProxy.SetActive(false);
    }

    private IEnumerator Editing()
    {
        for (; ; )
        {
            if (playModeBlockType != BlockType.Nothing)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    Vector3 pos;

                    if (_remove.IsPressed())
                    {
                        placeProxy.SetActive(false);
                        removeProxy.SetActive(true);

                        pos = world.GetBlockPos(hit);
                        removeProxy.transform.position = pos;

                        if (_click.WasPerformedThisFrame() && Mathf.RoundToInt(cost) <= PlayerStats.Instance.money) //removed
                        {
                            StartCoroutine(PlacingTerrain(hit, BlockType.Air));
                            
                        }
                    }
                    else
                    {
                        placeProxy.SetActive(true);
                        removeProxy.SetActive(false);

                        pos = world.GetBlockPos(hit, true);
                        placeProxy.transform.position = pos;

                        if (_click.WasPerformedThisFrame() && Mathf.RoundToInt(cost) <= PlayerStats.Instance.money) //placed
                        {
                            StartCoroutine(PlacingTerrain(hit, playModeBlockType, true));
                        }
                    }
                }
                else
                {
                    placeProxy.SetActive(false);
                    removeProxy.SetActive(false);
                }

            }
            yield return null;
        }
    }

    // This is the workaround to make sure the navmesh will bake to check if there are still valid paths !!!!!!!THIS SHOULD BE REWORKED AFTER PATHFINDING IS CHANGED
    private IEnumerator PlacingTerrain(RaycastHit hit, BlockType blockType, bool place = false)
    {
        //fill with dummy
        BlockType origional = world.GetBlock(hit, place);
        Vector3Int blockPos = Vector3Int.RoundToInt(world.GetBlockPos(hit, place));
        BlockType blockAbove = WorldDataHelper.GetBlock(world, blockPos + Vector3Int.up);

        if (world.IsBlockModifiable(blockPos) && !blockModifyBlacklist.Contains(world.GetBlock(hit, place)) //check if column is modifiable, if its not in the blacklist,
            && (place || (blockAbove != BlockType.Barrier && blockAbove != BlockType.Soft_Barrier)))   //and not destroying blocks below towers (barriers)
        {
            ModifyTerrain(hit, BlockType.Barrier, place);
            yield return 1;

            //check if path valid
            bool pathValid = EnemyTargetPathChecker.Instance.CheckPathFromTargetToEnemy();

            //spawn tower
            if (pathValid && Mathf.RoundToInt(cost) <= PlayerStats.Instance.money)
            {
                ModifyTerrain(hit, blockType, place);

                //remove money from player
                PlayerStats.Instance.money -= Mathf.RoundToInt(cost); //update money
            }
            else //otherwise remove barriers
            {
                ModifyTerrain(hit, origional, place);
                if (unableToPlaceCoroutine != null)
                    StopCoroutine(unableToPlaceCoroutine);
                unableToPlaceCoroutine = StartCoroutine(UnableToEditAnimation(placeProxy, placeProxyColor));
                if (unableToRemoveCoroutine != null)
                    StopCoroutine(unableToRemoveCoroutine);
                unableToRemoveCoroutine = StartCoroutine(UnableToEditAnimation(removeProxy, removeProxyColor));
            }
        }
        else
        {
            if (unableToPlaceCoroutine != null)
                StopCoroutine(unableToPlaceCoroutine);
            unableToPlaceCoroutine = StartCoroutine(UnableToEditAnimation(placeProxy, placeProxyColor));
            if (unableToRemoveCoroutine != null)
                StopCoroutine(unableToRemoveCoroutine);
            unableToRemoveCoroutine = StartCoroutine(UnableToEditAnimation(removeProxy, removeProxyColor));
        }
        yield return null;
    }

    private IEnumerator UnableToEditAnimation(GameObject proxy, Color origColor)
    {
        Renderer r = proxy.GetComponent<Renderer>();

        float currentTime = 0;

        while (currentTime <= 0.5f)
        {
            r.material.SetColor("_EmissionColor", Color.Lerp(unableToEditColor, origColor, currentTime / 0.5f));

            currentTime += Time.deltaTime;

            yield return null;
        }

        r.material.SetColor("_EmissionColor", origColor);


    }
}
