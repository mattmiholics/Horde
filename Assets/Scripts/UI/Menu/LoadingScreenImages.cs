using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class LoadingScreenImages : SerializedMonoBehaviour
{
    [SerializeField] private Image imageReference;
    [Space]
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private Dictionary<string, Sprite> levelImages;

    public void SetImage(string levelName)
    {
        Sprite sprite = levelImages.GetValueOrDefault(levelName);

        if (sprite)
            imageReference.sprite = sprite;
        else
            imageReference.sprite = defaultImage;
    }
}
