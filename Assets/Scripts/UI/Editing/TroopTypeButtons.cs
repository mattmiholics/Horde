using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TroopTypeButtons : MonoBehaviour
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

    public void onRoundEnd()
    {
        int i = gameObject.GetComponent<TowerData>().level;
        for(int j = 0; j < i; j++)
        {
            if(purchaseTroops.troopsActive < gameObject.GetComponent<TowerData>().level*10)
                purchaseTroops.trainNoCost();
        }
    }
}
