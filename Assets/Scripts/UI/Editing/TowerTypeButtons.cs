using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerTypeButtons : MonoBehaviour
{
    public Toggle startToggleDefault;
    [SerializeField]
    public ButtonGameObjectDictionary serializableButtonGameObject;
    public IDictionary<Toggle, GameObject> buttonGameObject
    {
        get { return serializableButtonGameObject; }
        set { serializableButtonGameObject.CopyFrom(value); }
    }

    private static TowerTypeButtons _instance;
    public static TowerTypeButtons Instance { get { return _instance; } }

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


    private void OnEnable()
    {
        SceneLoader.SceneLoaded += UpdateButtons;
    }

    private void OnDisable()
    {
        SceneLoader.SceneLoaded -= UpdateButtons;
    }

    private void UpdateButtons()
    {
        foreach (var entry in buttonGameObject)
        {
            entry.Key.onValueChanged.RemoveAllListeners();
        }

        foreach (var entry in buttonGameObject)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeTowerType(entry.Value));
        }

        UpdateText();

        startToggleDefault.isOn = true;
        ChangeTowerType(buttonGameObject[startToggleDefault]);
    }

    public void ChangeTowerType(GameObject tower)
    {
        TowerEditor.Instance.NewSelectedTower(tower);
    }

    public void UpdateText()
    {
        foreach (var entry in buttonGameObject)
        {
            if (entry.Value == null || !entry.Value.TryGetComponent<TowerData>(out TowerData td))
                return;
            if (entry.Key == null || !entry.Key.TryGetComponent<ButtonData>(out ButtonData bd))
                return;
            bd.subText.text = string.Format("${0}", td.cost);
        }
    }
}
