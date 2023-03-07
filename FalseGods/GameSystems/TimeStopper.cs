using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TimeStopper : MonoBehaviour
{
    private Tween timeStopTween;
    public void PlayerParryStop()
    {
        timeStopTween.Kill();

        if (GameManager.instance.gameIsPaused) return;

        Time.timeScale = 0f;
        timeStopTween = DOTween.To(x => Time.timeScale = x, Time.timeScale, 1f, 0.15f).SetUpdate(true).SetEase(Ease.InSine);
    }
}
