using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunningArrow", menuName = "Abilities/StunningArrow")]
public class AbilityStunningArrow : Ability
{
    [SerializeField] private GameObject stunArrowPrefab;
    [SerializeField] private float stunTime;
    [SerializeField] private int damage;

    [SerializeField] private AudioClip sfx;
    [SerializeField] private AudioClip windupSfx;
    [SerializeField] private GameObject windupVfx;


    public override void ActivateAbility(GameCharacter owner)
    {
        SoundManager.instance.PlaySound(sfx, owner.audioSource, 0.25f);

        GameObject projectile = Instantiate(stunArrowPrefab, owner.attackPosition.position, Quaternion.identity);
        StunArrowProjectile projectileScript = projectile.GetComponent<StunArrowProjectile>();
        projectileScript.SetStunArrowVariables(damage, stunTime);
        projectileScript.LaunchProjectile(40f, owner.currentDirection, false, damage);
    }

    public override void OnStartCasting(GameCharacter owner)
    {
        SoundManager.instance.PlaySound(windupSfx, owner.audioSource, 0.25f);

        Quaternion _rotation = Quaternion.Euler(0f, 0f, 180f);
        GameObject vfx = Instantiate(windupVfx, owner.attackPosition.position, Quaternion.identity);
        vfx.transform.SetParent(owner.transform, true);
        vfx.transform.rotation = _rotation;
    }
}
