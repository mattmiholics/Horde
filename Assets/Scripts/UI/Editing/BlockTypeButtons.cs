using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockTypeButtons : MonoBehaviour
{
    public Toggle startToggleDefault;
    public TextMeshProUGUI infoText;
    [SerializeField]
    ButtonBlockTypeDictionary serializableButtonBlockType;
    public IDictionary<Toggle, BlockType> buttonBlockType
    {
        get { return serializableButtonBlockType; }
        set { serializableButtonBlockType.CopyFrom(value); }
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
        foreach (var entry in buttonBlockType)
        {
            entry.Key.onValueChanged.RemoveAllListeners();
        }

        foreach (var entry in buttonBlockType)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeBlockType(entry.Value));
        }

        UpdateText();

        startToggleDefault.isOn = true;
        ChangeBlockType(buttonBlockType[startToggleDefault]);
    }

    public void ChangeBlockType(BlockType blockType)
    {
        TerrainEditor.Instance.playModeBlockType = blockType;
    }

    public void UpdateText()
    {
        infoText.text = string.Format("${0} - place\n${0} - remove", TerrainEditor.Instance.cost);
    }
}
