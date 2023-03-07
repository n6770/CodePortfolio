using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXpBar : MonoBehaviour
{
    [SerializeField] private Image xpFillImage;

    public void SetXpFill(float amount, float maxAmount)
    {
        xpFillImage.fillAmount = amount / maxAmount;
    }

}
