using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu(fileName = "Tower Data", menuName = "Data/Tower Data")]
public class TowerDataSO : SerializedScriptableObject
{
    [SerializeField]
    [TableList]
    [OnInspectorInit("DictionaryToList")]
    [OnInspectorDispose("ListToDictionaryDispose")]
    [ValidateInput("ValidateListAsDictionary")]
    [Searchable]
    private List<TowerPrefabInspectorClass> towerPrefabInspectorClasses;
#pragma warning disable 0414
    private bool listDictError = false;
#pragma warning restore 0414

    [HideInInspector]
    public Dictionary<int, GameObject> idTowerPrefab;

#if UNITY_EDITOR
    private void DictionaryToList()
    {
        if (!listDictError)
        {
            towerPrefabInspectorClasses = new List<TowerPrefabInspectorClass>();

            if (idTowerPrefab != null && idTowerPrefab.Count >= 0)
                idTowerPrefab.ToList().ForEach(keyval => { towerPrefabInspectorClasses.Add(new TowerPrefabInspectorClass(keyval.Key, keyval.Value)); });
            else
                towerPrefabInspectorClasses.Add(new TowerPrefabInspectorClass(0, null));
        }
    }

    private void ListToDictionaryDispose()
    {
        ListToDictionary();
    }

    private void ListToDictionary(UnityEngine.SceneManagement.Scene scene = default, string path = default)
    {
        if (!listDictError)
        {
            idTowerPrefab = new Dictionary<int, GameObject>();
            towerPrefabInspectorClasses.ForEach(tpic =>
            {
                idTowerPrefab.Add(tpic.id, tpic.towerPrefab);

                if (tpic.towerPrefab.GetComponent<TowerData>().id == tpic.id)
                    return;

                Debug.Log(AssetDatabase.GetAssetPath(tpic.towerPrefab));
                string path = AssetDatabase.GetAssetPath(tpic.towerPrefab);
                var root = PrefabUtility.LoadPrefabContents(path);

                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(root);
                root.GetComponent<TowerData>().id = tpic.id;

                PrefabUtility.SaveAsPrefabAsset(root, path);

                PrefabUtility.UnloadPrefabContents(root);
            });
        }
    }
#endif

    private bool ValidateListAsDictionary(List<TowerPrefabInspectorClass> towerPrefabInspectorClasses, ref string message, ref InfoMessageType? messageType)
    {
        List<int> duplicateIds = towerPrefabInspectorClasses.GroupBy(x => x.id)
                                        .Where(g => g.Count() > 1)
                                        .Select(x => x.Key)
                                        .ToList();

        if (duplicateIds.Count > 0) // Tower prefabs share ids
        {
            towerPrefabInspectorClasses.ForEach(tpic => { if (duplicateIds.Contains(tpic.id)) tpic.idColor = Color.red; else tpic.idColor = Color.white; });

            message = "One or more tower prefabs share the ID: " + String.Join(", ", duplicateIds);
            messageType = InfoMessageType.Error;
            listDictError = true;
            return false;
        }

        towerPrefabInspectorClasses.ForEach(tpic => { tpic.idColor = Color.white; });
        listDictError = false;
        return true;
    }



    [Serializable]
    private class TowerPrefabInspectorClass
    {
        [HideInInspector]
        public Color idColor = Color.white;

        [GUIColor("idColor")]
        [MinValue(0)]
        [VerticalGroup("ID", PaddingTop = 14, PaddingBottom = 14)]
        [HideLabel]
        [TableColumnWidth(60, false)]
        [InspectorName("ID")]
        public int id;

        [AssetList(CustomFilterMethod = "HasTowerDataComponent")]
        public GameObject towerPrefab;

        private bool HasTowerDataComponent(GameObject obj)
        {
            return obj.GetComponentInChildren<TowerData>() != null;
        }

        public TowerPrefabInspectorClass(int id, GameObject towerPrefab)
        {
            this.id = id;
            this.towerPrefab = towerPrefab;
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaving += ListToDictionary;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaving -= ListToDictionary;
#endif
    }
}