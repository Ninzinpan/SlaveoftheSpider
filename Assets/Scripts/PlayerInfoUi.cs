using UnityEngine;
using UnityEngine.UI; // Slider用
using TMPro;          // TextMeshPro用

public class PlayerInfoUI : MonoBehaviour
{
    [Header("UI Parts")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI energyText;

    // HPバーの初期化（最大値を設定）
    public void SetupHP(int maxHP, int currentHP)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    // HPの更新
    public void UpdateHP(int currentHP)
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
    }

    // エナジー表示の更新
    public void UpdateEnergy(int currentEnergy)
    {
        if (energyText != null)
        {
            energyText.text = $"{currentEnergy}"; // "3" のように表示
            // "3/3" にしたい場合は別途最大値を受け取る必要あり
        }
    }
}