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
public class TowerDataManager : SerializedMonoBehaviour
{
    public TowerDataSO towerDataSO;
}
