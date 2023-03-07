using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AbilityUIHandler : MonoBehaviour
{
    [SerializeField] private Image abilityImage;
    [SerializeField] private Image fillMask;
    [SerializeField] private Image flasher;
    [SerializeField] private TextMeshProUGUI cooldownText;
    private int cooldownTime;
    private int currentCooldown;
    private WaitForSeconds wfs;

    public void SetVariables(Sprite abilityIcon, float cooldown)
    {
        abilityImage.sprite = abilityIcon;
        cooldownTime = (int)cooldown;
        wfs = new WaitForSeconds(1f);
        cooldownText.text = "";
    }

    public void StartCooldown()
    {
        currentCooldown = cooldownTime;
        StartCoroutine(CooldownCounter());
        DOTween.To(x => fillMask.fillAmount = x, 0f, 1f, cooldownTime).SetEase(Ease.Linear);
    }

    public void HideThis()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator CooldownCounter()
    {
        cooldownText.text = currentCooldown.ToString();
        yield return wfs;
        currentCooldown--;
        if(currentCooldown <= 0)
        {
            CooldownReady();
        }
        else
        {
            StartCoroutine(CooldownCounter());
        }
    }

    private void CooldownReady()
    {
        cooldownText.text = "";
        flasher.DOFade(0.25f, 0.5f).SetLoops(2, LoopType.Yoyo);
    }
}
