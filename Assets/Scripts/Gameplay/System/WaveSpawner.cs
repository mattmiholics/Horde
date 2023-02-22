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
    public Transform parent;
    public Transform effectParent;
    [ValidateInput("@(spawns != null && spawns.Count > 0)")]
    public List<Altar> spawns;
    [ValidateInput("@(enemies != null && enemies.Count > 0)")]
    public List<GameObject> enemies;
    [Space]
    public GameObject progressBar;
    public Text waveCountdownText;

    public float intermissionTime = 5.5f;
    private float countdown = 2f;
    private static int waveNum = 1;

    private bool waveStarted;
    private int activeCoRoutines = 0;
    private float totalEnemyAmount = 0;

    [Space]
    public UnityEvent startWave;
    public UnityEvent endWave;

    private static WaveSpawner _instance;
    public static WaveSpawner Instance { get { return _instance; } }

    [OnValueChanged("@RefreshSpawnData()")]
    [OnInspectorInit("@RefreshSpawnData()")]
    [SerializeField]
    private List<WaveData> waveDataList;

    private void RefreshSpawnData()
    {
        waveDataList.ForEach(wd => wd.spawnDataList.ForEach(sd => { sd.waveSpawner = this; if (sd.spawn == null) sd.spawn = spawns.FirstOrDefault(); }));
    }

    [Serializable]
    private class WaveData
    {
        [SerializeField]
        [TableList(ShowIndexLabels = true)]
        public List<SpawnData> spawnDataList;
    }

    [Serializable]
    private class SpawnData
    {
        [HideInInspector]
        public WaveSpawner waveSpawner; // This is just to know which instance the data is apart of

        [MinValue(0)]
        public float time;
        [MinValue(0)]
        public float duration;
        [MinValue(0)]
        public int enemyCount;

        [TableColumnWidth(34, false)]
        [HorizontalGroup("Boss", PaddingLeft = 8)]
        [HideLabel]
        public bool boss;

        [ValueDropdown("GetEnemyTypes", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, CopyValues = false, OnlyChangeValueOnConfirm = true)]
        public GameObject enemyType;

        private IEnumerable GetEnemyTypes()
        {
            return waveSpawner.enemies.Select(enemy => new ValueDropdownItem(enemy.name, enemy));
        }

        private bool HasEnemyMovementComponent(GameObject obj)
        {
            return obj.GetComponentInChildren<EnemyMovement>() != null;
        }

        [ValueDropdown("GetAltars", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, CopyValues = false, OnlyChangeValueOnConfirm = true)]
        public Altar spawn;

        private IEnumerable GetAltars()
        {
            return waveSpawner.spawns.Select((spawn, index) => new ValueDropdownItem($"{index + 1}. Altar", spawn));
        }
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

    private void Start()
    {
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

    
    private void CalcTotalEnemiesInWave (WaveData wave_data)
    {
        this.totalEnemyAmount = 0;

        foreach (SpawnData spawn_data in wave_data.spawnDataList)
        {
            this.totalEnemyAmount += spawn_data.enemyCount;
        }
    }

    private void UpdateProgressBar()
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

        WaveData currWave = waveDataList.ElementAtOrDefault(waveNum-1);
        CalcTotalEnemiesInWave(currWave);
        spawnWave(currWave);
        waveNum++;

        if (progressBar == null)
            return;

        Slider slider = progressBar.GetComponent<Slider>();
        slider.value = 0f;
    }
    private void spawnWave(WaveData currWave)
    {
        List < SpawnData > spawnData = currWave.spawnDataList;

        foreach (SpawnData spawn_data in spawnData)
        {
            activeCoRoutines++;
            StartCoroutine(Spawn(spawn_data));
        }
    }

    IEnumerator Spawn(SpawnData spawn)
    {
        // wait until the given time to spawn
        yield return new WaitForSeconds(spawn.time);

        // calculate the interval at which to spawn enemies so they will spawn over the duration
        float interval = spawn.duration / spawn.enemyCount;

        for (int enemy_count = 1; enemy_count <= spawn.enemyCount; enemy_count++)
        {
            SpawnEnemy(spawn.enemyType, spawn.spawn);
            yield return new WaitForSeconds(interval);
        }
        activeCoRoutines--;
    }

    // changed from game object to transform? can change back wasnt sure
    void SpawnEnemy(GameObject prefab, Altar altar)
    {
        GameObject enemy = Instantiate(prefab, altar.spawnPoint.position, Quaternion.identity, parent);
        // Set target path for the agent
        if (enemy.TryGetComponent<Agent>(out Agent agent) && altar.TryGetComponent<StaticAgent>(out StaticAgent staticAgent))
        {
            agent.SetTarget(staticAgent.pathPoints);
        }

        if (progressBar != null)
            UpdateProgressBar();
    }
}