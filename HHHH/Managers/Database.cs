using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    [HideInInspector] public List<WeaponInst> weaponsDB;
    public Weapon[] allWeapons;
    public Weapon[] baseWeapons;

    private void Awake()
    {
        weaponsDB = new List<WeaponInst>();
        foreach (var weapon in allWeapons)
        {
            WeaponInst newWeaponInst = BuildWeaponInst(weapon);
            weaponsDB.Add(newWeaponInst);
        }
    }

    public WeaponInst BuildWeaponInst(Weapon weapon)
    {
        WeaponInst newWeaponInst = new WeaponInst(weapon.weaponName, weapon.damageType, weapon.damage, weapon.lifeTime, weapon.piercing,
                weapon.speed, weapon.range, weapon.colliderRadius, weapon.projectileAmount, weapon.projectileDelay, weapon.projectileAngle,
                weapon.rotates, weapon.cooldown, weapon.weaponMovement, weapon.weaponDirection, weapon.areaOfEffect, weapon.sprites, weapon.baseWeapon , weapon.upgradeLevel);
        return newWeaponInst;
    }

    public bool PlayerOwnsWeapon(Weapon weapon)
    {
        for (int i = 0; i < GameManager.instance.player.weapons.Count; i++)
        {
            if (GameManager.instance.player.weapons[i].baseWeapon == weapon)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveWeaponFromPlayer(Weapon weaponToRemove)
    {
        PlayerScript player = GameManager.instance.player;
        int index = -1;
        for (int i = 0; i < player.weapons.Count; i++)
        {
            if (player.weapons[i].baseWeapon == weaponToRemove)
            {
                index = player.weapons.IndexOf(player.weapons[i]);
            }
        }
        if (index >= 0) player.weapons.RemoveAt(index);
    }
}
