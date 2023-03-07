using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using DG.Tweening;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private Light2D menuLight;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject noVolumeImage;
    [SerializeField] private GameObject infoPanel, startButton, infoButton, quitButton;
    private bool sound = true;
    private bool infoOpen;


    void Start()
    {
        StartCoroutine(MenuAppearRoutine());
    }

    private IEnumerator MenuAppearRoutine()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.5f);

        yield return wfs;
        LightAppear();
        yield return wfs;
        TweenTitle();
        yield return new WaitForSeconds(2.5f);
        OpenUIElement(startButton);
        OpenUIElement(infoButton);
        OpenUIElement(quitButton);
    }

    private void TweenTitle()
    {
        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(titleText);

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < animator.textInfo.characterCount; ++i)
        {
            sequence.Append(animator.DOFadeChar(i, 1f, 0.075f));
        }
    }

    private void LightAppear()
    {
        DOTween.To(() => menuLight.pointLightOuterRadius, x => menuLight.pointLightOuterRadius = x, 10f, 5f).SetEase(Ease.InOutSine).OnComplete(() => LightUp());
    }
    private void LightUp()
    {
        DOTween.To(() => menuLight.pointLightOuterRadius, x => menuLight.pointLightOuterRadius = x, 14f, 3f).SetEase(Ease.InOutSine).OnComplete(() => LightDown());
    }

    private void LightDown()
    {
        DOTween.To(() => menuLight.pointLightOuterRadius, x => menuLight.pointLightOuterRadius = x, 10f, 3f).SetEase(Ease.InOutSine).OnComplete(() => LightUp());
    }


    public void OpenOptions()
    {
        if (!infoOpen) OpenUIElement(infoPanel);
        else CloseUIElement(infoPanel);
        infoOpen = !infoOpen;
    }

    public void OpenUIElement(GameObject gameObjectUI)
    {
        RectTransform rectTransform = gameObjectUI.GetComponent<RectTransform>();
        gameObjectUI.gameObject.SetActive(true);
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, 0.25f).SetUpdate(true);
    }
    public void CloseUIElement(GameObject gameObjectUI)
    {
        RectTransform rectTransform = gameObjectUI.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.DOScale(Vector3.zero, 0.25f).SetUpdate(true).OnComplete(() => gameObjectUI.gameObject.SetActive(false));
    }


    public void ToggleSound()
    {
        sound = !sound;
        if (sound)
        {
            AudioListener.volume = 1f;
            //noVolumeImage.SetActive(false);
        }
        else
        {
            AudioListener.volume = 0f;
            //noVolumeImage.SetActive(true);
        }
    }
}
