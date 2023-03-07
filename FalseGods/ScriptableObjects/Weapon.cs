using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon", order = 2)]
public class Weapon : ScriptableObject
{
    [HorizontalGroup("Info", Width = 50), HideLabel, PreviewField(50)]
    public Sprite weaponIcon;

    [VerticalGroup("Info/Type")]
    public string weaponName;

    [VerticalGroup("Info/Type")]
    public WeaponType weaponType;

    public int damageMin;
    public int damageMax;

    [Tooltip("Max attacks in a single combo")]
    public int maxCombo;
    [Tooltip("Damage multiplier for last attack if full combo")]
    public float comboEndMultiplier;

    [Tooltip("Melee: how far the attacker moves while attacking \nRanged: range of projectile")]
    public float range;

    [Tooltip("Delay until attack is triggered. Keep lowish for player weapons, larger for enemy weapons")]
    public float windupTime;

    [Tooltip("Melee: Movespeed while attacking, \nRanged: projectile velocity")]
    public float attackSpeed;

    public float attackCooldown;

    public float staminaCostAttack;

    public bool canParry;

    [ShowIf("canParry")]
    [Tooltip("How much stamina is gained after a succesful parry")]
    public float staminaGainParry;

    [ShowIf("canParry")]
    [Tooltip("Parry time winodw -> how long the parry animation plays")]
    public float parryTimeWindow;

    [ShowIf("canParry")]
    [Tooltip("How long the parried target stays stunned after a succesful parry")]
    public float parriedTime;

    [ShowIf("weaponType", WeaponType.Projectile)]
    [AssetsOnly]
    public GameObject projectilePrefab;

    public bool hasEffect;
    [ShowIf("hasEffect")]
    public StatusEffect effect;

    public enum WeaponSoundGroup { Sword, Bow }
    public WeaponSoundGroup soundGroup;

    public void PlaySwingSound(AudioSource _source, float _volume = 0.7f) 
    {
        WeaponSounds target = null;
        switch (soundGroup)
        {
            case WeaponSoundGroup.Sword:
                target = SoundManager.instance.swordSounds;
                break;
            case WeaponSoundGroup.Bow:
                target = SoundManager.instance.bowSounds;
                break;
        }

        if (target != null)
        {
            SoundManager.instance.PlayRandomSound(target.swingSounds, _source, _volume);
        }
    }
    public void PlayParrySound(AudioSource _source, float _volume = 0.7f)
    {
        WeaponSounds target = null;
        switch (soundGroup)
        {
            case WeaponSoundGroup.Sword:
                target = SoundManager.instance.swordSounds;
                break;
            case WeaponSoundGroup.Bow:
                target = SoundManager.instance.bowSounds;
                break;
        }

        if (target != null)
        {
            SoundManager.instance.PlayRandomSound(target.parrySounds, _source, _volume);
        }
    }
    public void PlayHitSound(AudioSource _source, float _volume = 0.7f)
    {
        WeaponSounds target = null;
        switch (soundGroup)
        {
            case WeaponSoundGroup.Sword:
                target = SoundManager.instance.swordSounds;
                break;
            case WeaponSoundGroup.Bow:
                target = SoundManager.instance.bowSounds;
                break;
        }

        if (target != null)
        {
            SoundManager.instance.PlayRandomSound(target.hitSounds, _source, _volume);
        }
    }


}
