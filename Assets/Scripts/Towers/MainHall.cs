using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainHall : MonoBehaviour
{
    public Transform target;

    public Image healthBar;
    public int maxHealth;

    public GameObject gameManager;

    public void Start()
    {
        maxHealth = gameManager.GetComponent<PlayerStats>().startLives;
    }

    public void Update()
    {
        healthBar.fillAmount = (float) gameManager.GetComponent<PlayerStats>().lives / maxHealth;
       
    }


}
