using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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
    public TextMeshProUGUI troopsText;

    public float troopSpawnTimer = 6f;
    public bool trainingTroop = false;
    public List<int> troopQueue = new List<int>();
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
        barracks = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<Barracks>().GetComponentInParent<TowerData>().gameObject;
        spawnPoint = barracks.GetComponentInChildren<Barracks>().spawnPoint;
        troopsText = barracks.GetComponentInChildren<Barracks>().troopText;
    }

    public void SpawnArcher()
    {
        if (PlayerStats.Instance.money >= 65 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;
            troopQueue.Add(1);
            PlayerStats.Instance.money -= 65;
            if (!trainingTroop)
            {
                trainingTroop = true;
            }
        }
    }

    public void SpawnKnight()
    {
        if (PlayerStats.Instance.money >= 50 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;

            troopQueue.Add(2);
            PlayerStats.Instance.money -= 50;
            if (!trainingTroop)
            {
                trainingTroop = true;
            }
        }
    }

    public void SpawnFireMage()
    {
        if (PlayerStats.Instance.money >= 80 && troopsActive < barracks.GetComponent<TowerData>().level * 10 && troopQueue.ToArray().Length < spawnQueueLimit)
        {
            troopsActive++;

            troopQueue.Add(3);
            PlayerStats.Instance.money -= 80;
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

    public void trainNoCost()
    {
        troopsActive++;
        troop = Instantiate(troop1Prefab, spawnPoint.position, Quaternion.identity, troopParent);
        troop.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }

    public void Update()
    {
        troopsText.text = troopsActive + "/" + (barracks.GetComponent<TowerData>().level * 10);

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


