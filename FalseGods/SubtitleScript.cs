using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SubtitleScript : MonoBehaviour
{
    [SerializeField] private int[] storypoints;
    private int currentStorypoint;
    private bool voicelineFinished;
    private int voicelinesToPlay;
    [SerializeField] private float textSpeed;

    [SerializeField] private GameObject subtitlesParent;
    [SerializeField] private VoiceLine[] voiceLines;
    [SerializeField] private Image subtitleImage;
    [SerializeField] private Image frameFill;
    [SerializeField] private Image frame;
    [SerializeField] private Image topGradient;
    [SerializeField] private Image bottomGradient;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private SubtitleCharacter shephes; //luoja/mentor
    [SerializeField] private SubtitleCharacter kaure; //pelaajaHahmo
    [SerializeField] private SubtitleCharacter nuhor; //companionHahmo
    [SerializeField] private SubtitleCharacter boarGod; //boss1

    private Tween imageAppearTween;
    private Tween nameAppearTween;
    private Sequence sequence;
    private DOTweenTMPAnimator animator;
    private int currentVoiceLineIndex;

    public bool playingDialogue;

    private Coroutine nextFade;

    private void Start()
    {
        animator = new DOTweenTMPAnimator(subtitleText);
    }

    private void Update()
    {
        //skipping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (sequence.active)
            {
                if (!sequence.IsComplete())
                {
                    sequence.Complete();
                }
            }
            else
            {
                StopCoroutine(nextFade);
                HandleNextVoiceLine();
            }
        }
    }

    public void StartDialogue(int _storyPoint)
    {
        playingDialogue = true;
        GameManager.instance.SetPlayerControl(false);

        currentStorypoint = _storyPoint;
        currentVoiceLineIndex = storypoints[currentStorypoint];
        voicelinesToPlay = storypoints[currentStorypoint + 1] - storypoints[currentStorypoint];

        PlayNextVoiceLine();

        frameFill.DOFade(0.3f, 1f);
        frame.DOFade(1f, 1f);
        topGradient.DOFade(0.5f, 1f);
        bottomGradient.DOFade(0.5f, 1f);
    }

    public void SetNextVoiceline(int index)
    {
        currentVoiceLineIndex = index;
    }

    public void PlayNextVoiceLine()
    {
        PlayVoiceLine(currentVoiceLineIndex);
        currentVoiceLineIndex++;
    }

    public void PlayVoiceLine(int index)
    {
        voicelineFinished = false;

        VoiceLine voiceToPlay = voiceLines[index];
        SubtitleCharacter characterToPlay = null;

        imageAppearTween.Kill();
        switch (voiceToPlay.character)
        {
            case CharacterName.Shephes:
                characterToPlay = shephes;
                break;
            case CharacterName.Kaure:
                characterToPlay = kaure;
                break;
            case CharacterName.Nuhor:
                characterToPlay = nuhor;
                break;
            case CharacterName.BoarGod:
                characterToPlay = boarGod;
                break;
        }

        subtitleImage.sprite = characterToPlay.characterImage;
        nameText.text = characterToPlay.name;
        subtitleText.text = voiceToPlay.text;

        if (voiceToPlay.eventToTrigger != null)
        {
            voiceToPlay.eventToTrigger.Invoke();
        }

        StartCoroutine(StartAnimate(voiceToPlay.startDelay));

        IEnumerator StartAnimate(float delay)
        {
            yield return new WaitForSeconds(delay);
            AnimateSubtitles();
        }

    }
    private void AnimateSubtitles()
    {
        sequence.Kill();
        StopAllCoroutines();
        int charactersInLine = subtitleText.text.Length;
        float appearSpeed = textSpeed;

        animator.Refresh();
        sequence = DOTween.Sequence();
        for (int i = 0; i < animator.textInfo.characterCount; ++i)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            sequence.Append(animator.DOFadeChar(i, 1f, appearSpeed));
        }
        SetSubtitleImage(true);
        sequence.Play().OnComplete(() => nextFade = StartCoroutine(FadeOutSubtitles()));
    }

    private void SetSubtitleImage(bool state, float fadeTime = 1f)
    {
        float endValue = state ? 1f : 0f;
        imageAppearTween = subtitleImage.DOFade(endValue, fadeTime);
        nameAppearTween = nameText.DOFade(endValue, fadeTime);
    }

    private void FadeOutText(float time = 2f)
    {
        sequence = DOTween.Sequence();
        for (int i = 0; i < animator.textInfo.characterCount; ++i)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            sequence.Join(animator.DOFadeChar(i, 0f, time));
        }
        sequence.Play();

    }

    private IEnumerator FadeOutSubtitles()
    {
        float delay = 2f;
        yield return new WaitForSeconds(delay);

        HandleNextVoiceLine();
    }
    private void HandleNextVoiceLine()
    {
        voicelineFinished = true;
        voicelinesToPlay--;
        if (voicelinesToPlay > 0)
        {
            PlayNextVoiceLine();
        }
        else
        {
            SetSubtitleImage(false, 2f);
            FadeOutText();
            playingDialogue = false;
            GameManager.instance.SetPlayerControl(true);

            frameFill.DOFade(0f, 1f);
            frame.DOFade(0f, 1f);
            topGradient.DOFade(0f, 1f);
            bottomGradient.DOFade(0f, 1f);

        }
    }

}

[System.Serializable]
public class SubtitleCharacter
{
    public string name;
    public Sprite characterImage;
}
[System.Serializable]
public class VoiceLine
{
    public CharacterName character;
    public string text;
    public float startDelay;
    public bool useTrigger;
    [ShowIf("useTrigger")] public UnityEvent eventToTrigger;
}

public enum CharacterName { Shephes, Kaure, Nuhor, BoarGod };
