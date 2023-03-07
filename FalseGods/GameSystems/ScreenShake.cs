using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    public CinemachineVirtualCamera cmCam;
    private CinemachineBasicMultiChannelPerlin noise;
    private Tween shakeTween;

    private void Start()
    {
        noise = cmCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void StartShake(float shakeTime, float intensity = 1f, float amplitude = 1f)
    {
        shakeTween.Kill();
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = intensity;

        DOTween.To(x => noise.m_FrequencyGain = x, noise.m_FrequencyGain, 0f, shakeTime).SetEase(Ease.InSine);
    }
}
