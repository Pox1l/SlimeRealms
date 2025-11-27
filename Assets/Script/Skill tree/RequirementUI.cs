using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequirementUI : MonoBehaviour
{
    public Image iconImg;
    public TextMeshProUGUI amountText;

    public void Setup(Sprite icon, int currentAmount, int requiredAmount)
    {
        iconImg.sprite = icon;
        amountText.text = $"{currentAmount} / {requiredAmount}";
        amountText.color = (currentAmount < requiredAmount) ? Color.red : Color.white;
    }
}