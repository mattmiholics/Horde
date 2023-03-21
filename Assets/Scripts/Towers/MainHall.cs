using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainHall : MonoBehaviour
{
    public Transform target;

    public Image healthBar;
    public int maxHealth;

    private void Start()
    {
        maxHealth = PlayerStats.Instance.startLives;
    }

    public void Update()
    {
        healthBar.fillAmount = (float) PlayerStats.Instance.lives / maxHealth;
    }


}
