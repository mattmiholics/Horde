using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

[ExecuteAlways]
public class TowerLoader : MonoBehaviour
{
    //THIS SCRIPT IS UNFINISHED AND DOESN'T WORK

    /*public TowerDataManager towerDataManager;
    public TowerEditor towerEditor;
    [Space]
    public Transform towerParent;
    public Transform permanentTowerParent;
    [Space]
    public string towerFileName = "towerdata.txt";

    CancellationTokenSource taskTokenSource = new CancellationTokenSource();
    Progress<int> progress;

    private void OnEnable()
    {
        World.WorldSaved += SaveTowers;
        World.WorldLoaded += LoadTowers;
    }
    private void OnDisable()
    {
        World.WorldSaved -= SaveTowers;
        World.WorldLoaded -= LoadTowers;
    }

    private void SaveTowers(string directory, bool saveToResources)
    {
        List<SaveTowerData> positionTowerPrefabIndex = new List<SaveTowerData>();

        towerParent.GetComponentsInChildren<TowerData>().ForEach(td => positionTowerPrefabIndex.Add(new SaveTowerData(td.id, td.transform.position, td.rotation)));
        permanentTowerParent.GetComponentsInChildren<TowerData>().ForEach(td => positionTowerPrefabIndex.Add(new SaveTowerData(td.id, td.transform.position, td.rotation)));

        byte[] bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(positionTowerPrefabIndex, DataFormat.Binary);
        File.WriteAllBytes(directory + towerFileName, bytes);

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }

    private async void LoadTowers(string directory, bool loadFromResources)
    {
        List<SaveTowerData> positionTowerPrefabIndex = new List<SaveTowerData>();

        try
        {
            if (loadFromResources)
            {
                byte[] bytes = Resources.Load<TextAsset>(directory.Replace(Application.dataPath + "/Resources/", "") + Path.GetFileNameWithoutExtension(towerFileName)).bytes;
                positionTowerPrefabIndex = Sirenix.Serialization.SerializationUtility.DeserializeValue<List<SaveTowerData>>(bytes, DataFormat.Binary);
            }
            else
            {
                //if (!File.Exists(directory + towerFileName))
                    //Debug.LogWarning("Missing tower data files! No towers were loaded from \"directory + towerFileName\"!");
                // Logic for loading during runtime
            }
        }
        catch
        {
            Debug.LogWarning($"Missing tower data files! No towers were loaded from \"{directory}{towerFileName}\"!");
            return;
        }

        await GenerateTowers(positionTowerPrefabIndex);
    }

    private async Task GenerateTowers(List<SaveTowerData> positionTowerPrefabIndex)
    {
        progress = new Progress<int>();

        List<TowerData> towerDataList = new List<TowerData>();
        towerDataList.Concat(towerParent.GetComponentsInChildren<TowerData>().ToList());
        towerDataList.Concat(permanentTowerParent.GetComponentsInChildren<TowerData>().ToList());

        await DeleteTowers(towerDataList, progress);
        try
        {
            
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }

        await InstantiateTowers(positionTowerPrefabIndex, progress);
        try
        {
            
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }
    }

    private Task DeleteTowers(List<TowerData> towerDataList, IProgress<int> progress)
    {
        return Task.Run(() =>
        {

            for(int i = 0; i < towerDataList.Count; i++)
            {
                if (Application.isPlaying)
                    Destroy(towerDataList[i].gameObject);
                else
                    DestroyImmediate(towerDataList[i].gameObject);

                progress.Report(Mathf.RoundToInt((i/towerDataList.Count) * 50));
                Debug.Log(progress);
            }
        },
        taskTokenSource.Token
        );
    }

    private Task InstantiateTowers(List<SaveTowerData> positionTowerPrefabIndex, IProgress<int> progress)
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < positionTowerPrefabIndex.Count; i++)
            {
                TowerHelper.PlaceTower(towerEditor, towerDataManager.idTowerPrefab[positionTowerPrefabIndex[i].id], positionTowerPrefabIndex[i].position, positionTowerPrefabIndex[i].rotation);

                progress.Report(50 + (Mathf.RoundToInt((i / positionTowerPrefabIndex.Count) * 50)));
                Debug.Log(progress);
            }
        },
        taskTokenSource.Token
        );
    }

    [Serializable]
    private class SaveTowerData
    {
        public int id;
        public Vector3 position;
        public float rotation;

        public SaveTowerData(int id, Vector3 position, float rotation)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
    }*/
}
