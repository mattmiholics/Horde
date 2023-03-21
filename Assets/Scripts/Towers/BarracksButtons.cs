using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BarracksButtons : MonoBehaviour
{
    private PurchaseTroops purchaseTroops;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(); //just in case the object is enabled after the scene load
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene = default, LoadSceneMode mode = default)
    {
        purchaseTroops = PurchaseTroops.Instance;
    }

    public void Troop1()
    {
        purchaseTroops.SpawnArcher();
    }
    public void Troop2()
    {
        purchaseTroops.SpawnKnight();
    }

    public void Troop3()
    {
        purchaseTroops.SpawnFireMage();
    }
}
