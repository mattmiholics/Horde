using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    public Slider slider;
    private static ProgressUI _instance;
    public static ProgressUI Instance { get { return _instance; } }
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
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
