using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    private TextMeshProUGUI lives;
    private void Start()
    {
        lives = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (PlayerStats.Instance != null)
            lives.text = string.Format("{0}", PlayerStats.Instance.lives);
    }
}
