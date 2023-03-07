using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CombatText : MonoBehaviour
{
    private TextMeshPro textMesh;

    [SerializeField] private Color damageColor;
    [SerializeField] private Color infoColor;
    [SerializeField] private Color healColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void ActivateText(Vector3 position, string text, bool info = false, bool heal = false, bool critical = false)
    {
        transform.position = position;
        textMesh.text = text;
        textMesh.alpha = 1f;

        if (info) textMesh.color = infoColor;
        else if (heal) textMesh.color = healColor;
        else textMesh.color = damageColor;

        if (critical) transform.localScale = Vector3.one * 1.5f;
        else transform.localScale = Vector3.one;
        transform.DOMove(transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(2.5f, 3.5f), 0f), 1.5f).SetEase(Ease.OutQuart);
        transform.DOPunchScale(transform.localScale * 1.2f, 0.25f);
        textMesh.DOFade(0f, 1.5f).OnComplete(() => gameObject.SetActive(false));
    }
}
