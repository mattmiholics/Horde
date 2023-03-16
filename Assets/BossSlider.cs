using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossSlider : MonoBehaviour
{
    public GameObject bossSliderPrefab;
    private static BossSlider _instance;
    public static BossSlider Instance { get { return _instance; } }

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createBossIcons(List<float> bossSpawns)
    {
        foreach (float boss in bossSpawns)
        {
            Debug.Log("boss:" + boss);
            createBossSlider(boss);
        }
    }

    private void createBossSlider(float spawnLoc)
    {
        GameObject newBoss = Instantiate(bossSliderPrefab, this.gameObject.transform);
        Slider slider = newBoss.GetComponent<Slider>();
        slider.value = spawnLoc;
    }
}
