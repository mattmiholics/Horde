using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PurchaseTroops : MonoBehaviour
{
    public Transform troop1Prefab;
    public Transform troop2Prefab;
    public Transform troop3Prefab;
    public Transform troopParent;
    private Transform troop;

    private Transform spawnPoint;
    public float speed = 5;
    public GameObject barracks;
    public int troopsActive = 0;
    public Text troops;

    public float troopSpawnTimer = 6f;
    private bool trainingTroop = false;
    private List<int> troopQueue = new List<int>();
    private int spawnQueueLimit = 5;

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
        if (PlayerStats.Instance.money >= 200 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;
            troopQueue.Add(1);
            PlayerStats.Instance.money -= 200;
            if (!trainingTroop)
            {
                trainingTroop = true;
            }
        }
    }

    public void SpawnTroop2()
    {
        if (PlayerStats.Instance.money >= 200 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;

            troopQueue.Add(2);
            PlayerStats.Instance.money -= 200;
            if (!trainingTroop)
            {
                trainingTroop = true;
            }
        }
    }

    public void SpawnTroop3()
    {
        if (PlayerStats.Instance.money >= 250 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;

            troopQueue.Add(3);
            PlayerStats.Instance.money -= 250;
            if (!trainingTroop)
            {
                trainingTroop = true;
            }
        }
    }

    public void troopDeath()
    {
        troopsActive--;
    }

    public void Update()
    {
        troops.text = troopsActive + "/" + (barracks.GetComponent<TowerData>().level * 10);

        if (trainingTroop)
        {
            if (troopSpawnTimer <= 0)
            {
                switch (troopQueue[0])
                {
                    case 1:
                        troopQueue.RemoveAt(0);
                        troop = Instantiate(troop1Prefab, spawnPoint.position, Quaternion.identity, troopParent);
                        troop.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                        break;
                    case 2:
                        troopQueue.RemoveAt(0);
                        troop = Instantiate(troop2Prefab, spawnPoint.position, Quaternion.identity, troopParent);
                        troop.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                        break;
                    case 3:
                        troopQueue.RemoveAt(0);
                        troop = Instantiate(troop3Prefab, spawnPoint.position, Quaternion.identity, troopParent);
                        troop.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                        break;
                };
                troopSpawnTimer = 6f;
                if (troopQueue.ToArray().Length <= 0)
                {
                    trainingTroop = false;
                }
            }
            else
            {
                troopSpawnTimer -= Time.deltaTime;
            }
        }
    }
}


