using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CastBarScript : MonoBehaviour
{
    private Transform ownerTransform;
    [SerializeField]
    private Transform fillPivot;
    [SerializeField]
    private Vector3 barOffset;
    [SerializeField]
    private float fadeOutTime;
    [SerializeField]
    private float fadeInTime;
    [SerializeField]
    private SpriteRenderer frameSprite;
    [SerializeField]
    private SpriteRenderer fillSprite;
    [SerializeField]
    private SpriteRenderer imageRenderer;
    [SerializeField]
    private TextMeshPro barText;

    private void Update()
    {
        if (ownerTransform != null) transform.position = ownerTransform.position + barOffset;
    }

    public void StartCastBar(float time, Transform newParent, Sprite image, string text)
    {
        ownerTransform = newParent;
        imageRenderer.sprite = image;
        barText.text = text;
        FadeInBar();
        fillPivot.localScale = new Vector3(0f, 1f, 1f);
        fillPivot.DOScaleX(1f, time).SetEase(Ease.Linear).OnComplete(() => FadeOutBar());
    }
    private void FadeInBar()
    {
        fillSprite.DOFade(1f, fadeInTime);
        frameSprite.DOFade(1f, fadeInTime);
        imageRenderer.DOFade(1f, fadeInTime);
        barText.DOFade(1f, fadeInTime);
    }
    private void FadeOutBar()
    {
        imageRenderer.DOFade(0f, fadeOutTime);
        barText.DOFade(0f, fadeOutTime);
        fillSprite.DOFade(0f, fadeOutTime);
        frameSprite.DOFade(0f, fadeOutTime).OnComplete(() => DisableBar());
    }

    private void DisableBar()
    {
        gameObject.SetActive(false);
    }
}
