using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Color fullcolor, emptyColor;
    [SerializeField] Transform fillTransform, whiteFillTransform;
    [SerializeField] private SpriteRenderer fillSprite;

    public void SetFill(float health, float maxHealth)
    {
        float fillNormalized = health / maxHealth;
        fillTransform.localScale = new Vector3(fillNormalized, 1f, 1f);
        fillSprite.color = Color.Lerp(emptyColor, fullcolor, fillNormalized);
        TweenWhite(fillNormalized);
    }

    public void TweenWhite(float amount)
    {
        whiteFillTransform.transform.DOScaleX(amount, 0.5f);
    }
}
