using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Halloween Hordes/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public DamageType damageType;
    public AudioClip weaponSound;
    public float damage;
    public float lifeTime;
    public bool piercing;
    public float speed;
    public float range;
    public float colliderRadius;

    public int projectileAmount;
    public float projectileDelay;
    public float projectileAngle;

    public bool rotates;

    public float cooldown;

    public WeaponMovement weaponMovement;
    public WeaponDirection weaponDirection;
    public AreaOfEffect areaOfEffect;

    public Weapon baseWeapon;
    public int upgradeLevel;
    public int maxUpgradeLevel;
    public Sprite[] sprites;
}
[System.Serializable]
public class WeaponInst
{
    public string weaponName;
    public DamageType damageType;
    public float damage;
    public float lifeTime;
    public bool piercing;
    public float speed;
    public float range;
    public float colliderRadius;

    public int projectileAmount;
    public float projectileDelay;
    public float projectileAngle;

    public bool rotates;

    public float cooldown;

    public WeaponMovement weaponMovement;
    public WeaponDirection weaponDirection;
    public AreaOfEffect areaOfEffect;

    /// <summary>
    /// Multiple if animated
    /// </summary>
    public Sprite[] sprites;
    public Weapon baseWeapon;
    public int upgradeLevel;

    public WeaponInst(string weaponName, DamageType damageType, float damage, float lifeTime, bool piercing, float speed, float range, float colliderRadius, int projectileAmount, float projectileDelay, float projectileAngle, 
        bool rotates, float cooldown, WeaponMovement weaponMovement, WeaponDirection weaponDirection, AreaOfEffect areaOfEffect, Sprite[] sprites, Weapon baseWeapon, int upgradeLevel)
    {
        this.weaponName = weaponName;
        this.damage = damage;
        this.damageType = damageType;
        this.lifeTime = lifeTime;
        this.piercing = piercing;
        this.speed = speed;
        this.range = range;
        this.colliderRadius = colliderRadius;
        this.projectileAmount = projectileAmount;
        this.projectileDelay = projectileDelay;
        this.projectileAngle = projectileAngle;
        this.rotates = rotates;
        this.cooldown = cooldown;
        this.weaponMovement = weaponMovement;
        this.weaponDirection = weaponDirection;
        this.areaOfEffect = areaOfEffect;
        this.sprites = sprites;
        this.baseWeapon = baseWeapon;
        this.upgradeLevel = upgradeLevel;
    }
}
