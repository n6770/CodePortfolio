using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    //MUSIC
    [SerializeField] private AudioSource musicSource;

    //SOUNDS
    [SerializeField] private AudioSource sfxSource;

    public AudioClip[] enemyDamageSounds;
    public AudioClip[] xpSounds;
    public AudioClip healSound;
    public AudioClip[] playerDamageSounds;
    public AudioClip levelUpSound;
    public AudioClip upgradeSound;

    //GENERAL
    [Header("General")]
    [SerializeField] private float defaultVolume = 0.5f;
    [SerializeField] private float soundIgnoreTime = 0.05f;
    private float currentIgnoreTime;
    private bool canPlaySFX;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("music");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this);

    }

    private void Update()
    {
        if (currentIgnoreTime < soundIgnoreTime) currentIgnoreTime += Time.deltaTime;
        if (currentIgnoreTime > soundIgnoreTime && !canPlaySFX)
        {
            canPlaySFX = true;
        }
    }

    public void NextSong()
    {

    }

    public void PlaySound(AudioClip clip, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
        currentIgnoreTime = 0f;
    }

}
