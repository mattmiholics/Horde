using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    private TextMeshProUGUI money;
    private void Start()
    {
        money = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (PlayerStats.Instance != null)
            money.text = "$" + PlayerStats.Instance.money.ToString("n0");
    }
}
