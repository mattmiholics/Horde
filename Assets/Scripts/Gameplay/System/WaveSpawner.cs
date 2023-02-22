using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;

public class WaveSpawner : MonoBehaviour
{
    public GameObject progressBar;

    public Transform enemyPrefab;
    public Transform fastEnemyPrefab;
    public Transform slowEnemyPrefab;
    private Transform spawnPoint;
    public Transform parent;
    public Transform effectParent;

    public UnityEvent startWave;
    public UnityEvent endWave;

    public Text waveCountdownText;

    public float intermissionTime = 5.5f;
    private float countdown = 2f;
    private static int waveNum = 1;

    private bool waveStarted;
    private int activeCoRoutines = 0;
    private float totalEnemyAmount = 0;

    private static WaveSpawner _instance;
    public static WaveSpawner Instance { get { return _instance; } }

    [SerializeField]
    private List<WaveData> waveDataList;

    [Serializable]
    private class WaveData
    {
        [SerializeField]
        [TableList(ShowIndexLabels = true)]
        public List<SpawnData> spawnDataList;

        public List<SpawnData> getSpawnData()
        {
            return spawnDataList;
        }
    }

    [Serializable]
    private class SpawnData
    {
        [MinValue(0)]
        public float time;
        [MinValue(0)]
        public float duration;
        [MinValue(0)]
        public int enemyCount;

        [TableColumnWidth(133, false)]
        [AssetList(CustomFilterMethod = "HasEnemyMovementComponent")]
        public GameObject enemyType;

        public bool boss;

        private bool HasEnemyMovementComponent(GameObject obj)
        {
            return obj.GetComponentInChildren<EnemyMovement>() != null;
        }

        public int spawn;
    }

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

        waveStarted = false;
    }

    // Update is called once per frame
    /*void Update()
    {

        if (countdown <= 0)
        {
            //Check for any MarketBuildings
            MarketBuilding[] markets = FindObjectsOfType(typeof(MarketBuilding)) as MarketBuilding[];
            foreach (MarketBuilding item in markets)
            {
                item.PayPlayer(item.buildingLevel);
            }

            StartCoroutine(spawnWave());
            if(waveNum > 5)
            {
                StartCoroutine(spawnWaveFast());
            }
            countdown = intermissionTime;
            waveNum++;
        }

        countdown -= Time.deltaTime;

        countdown = Mathf.Clamp(countdown, 0f, Mathf.Infinity);

        waveCountdownText.text = string.Format("{0:00.00}", countdown); ;
    }
    */

    private void Start()
    {
        spawnPoint = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<Altar>().spawnPoint;

        NextButtons.Instance.nextLevelButton.SetActive(false);

        // Check if there are any waves
        if (waveDataList != null && waveDataList.Count > 0)
            NextButtons.Instance.nextWaveButton.SetActive(true);
        else
            NextButtons.Instance.nextWaveButton.SetActive(false);
    }
    private void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (activeCoRoutines >= 1 && !waveStarted) //wave started
        {
            waveStarted = true;
            EditButtons.Instance.DisableButtons();
            startWave.Invoke();

            NextButtons.Instance.nextWaveButton.SetActive(false);
        }
        else if (enemies.Length == 0 && activeCoRoutines == 0 && waveStarted) //wave ended
        {
            waveStarted = false;
            endWave.Invoke();
            EditButtons.Instance.EnableButtons();

            // Check if there are any more waves
            if (waveNum > waveDataList.Count)
                NextButtons.Instance.nextLevelButton.SetActive(true);
            else
                NextButtons.Instance.nextWaveButton.SetActive(true);
        }
    }

    
    private void calcTotalEnemiesInWave (WaveData wave_data)
    {
        this.totalEnemyAmount = 0;

        foreach (SpawnData spawn_data in wave_data.getSpawnData())
        {
            this.totalEnemyAmount += spawn_data.enemyCount;
        }
    }

    private void updateProgressBar()
    {
        Slider slider = progressBar.GetComponent<Slider>();
        float percentageToAdd = 1 / this.totalEnemyAmount;
        slider.value += percentageToAdd;
    }


    public void DisableUI(string UIName)
    {
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == UIName).SingleOrDefault();
        if (UI != null)
            UI.SetActive(false);
    }
    public void EnableUI(string UIName)
    {
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == UIName).SingleOrDefault();
        if (UI != null)
            UI.SetActive(true);
    }

    public static string getWaveNum()
    {
        return string.Format("{0}", waveNum - 1);
    }

    public void SpawnNextWave()
    {
        PlayerStats.Instance.rounds++;
        Slider slider = progressBar.GetComponent<Slider>();
        slider.value = 0f;

        WaveData currWave = waveDataList.ElementAtOrDefault(waveNum-1);
        calcTotalEnemiesInWave(currWave);
        spawnWave(currWave);
        waveNum++;
    }
    private void spawnWave(WaveData currWave)
    {
        List < SpawnData > spawnData = currWave.getSpawnData();

        foreach (SpawnData spawn_data in spawnData)
        {
            activeCoRoutines++;
            StartCoroutine(spawn(spawn_data));
        }
    }

    IEnumerator spawn(SpawnData spawn)
    {
        // wait until the given time to spawn
        yield return new WaitForSeconds(spawn.time);

        // calculate the interval at which to spawn enemies so they will spawn over the duration
        float interval = spawn.duration / spawn.enemyCount;

        for (int enemy_count = 1; enemy_count <= spawn.enemyCount; enemy_count++)
        {
            spawnEnemy(spawn.enemyType);
            yield return new WaitForSeconds(interval);
        }
        activeCoRoutines--;
    }

    // changed from game object to transform? can change back wasnt sure
    void spawnEnemy(GameObject prefab)
    {
        updateProgressBar();
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parent);
    }
}