using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossSlider : MonoBehaviour
{
    public GameObject bossSliderPrefab;
    private List<GameObject> bossSliders = new List<GameObject>();
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
            createBossSlider(boss);
        }
    }

    private void createBossSlider(float spawnLoc)
    {
        GameObject newBoss = Instantiate(bossSliderPrefab, this.gameObject.transform);
        this.bossSliders.Add(newBoss);
        Slider slider = newBoss.GetComponent<Slider>();
        slider.value = spawnLoc;
    }

    public void destroyBossIcons()
    {
        foreach (GameObject bossSlider in this.bossSliders)
        {
            GameObject.Destroy(bossSlider);
        }
        this.bossSliders.Clear();
    }
}
