using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PurchaseTroops : MonoBehaviour
{
    public Transform troop1Prefab;
    public Transform troop2Prefab;
    public Transform troopParent;
    private Transform troop;

    private Transform spawnPoint;
    public float speed = 5;

    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string unitActionMap;

    [HideInInspector] public bool menuActive;

    private PlayerInput _playerInput;

    private static PurchaseTroops _instance;
    public static PurchaseTroops Instance { get { return _instance; } }

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
    }

    private void Start()
    {
        _playerInput = CameraHandler.Instance.playerInput;

        spawnPoint = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<Barracks>().spawnPoint;
    }

    public void SpawnTroop1()
    {
        if(PlayerStats.Instance.money >= 150)
        {
            troop = Instantiate(troop1Prefab, spawnPoint.position, Quaternion.identity, troopParent);
            troop.GetComponent<NavMeshAgent>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            PlayerStats.Instance.money -= 150;
        }
    }

    public void SpawnTroop2()
    {
        if (PlayerStats.Instance.money >= 200)
        {
            troop = Instantiate(troop2Prefab, spawnPoint.position, Quaternion.identity, troopParent);
            troop.GetComponent<NavMeshAgent>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            PlayerStats.Instance.money -= 200;
        }
    }
}
