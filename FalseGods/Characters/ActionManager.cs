using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ActionManager : MonoBehaviour
{
    private GameCharacter owner;

    [HideInInspector]
    public Weapon currentWeapon;
    private float weaponCooldown = 0f;

    private Quaternion slashEffectRotation;

    private int currentMeleeQueue;
    private int meleeQueue;
    private int meleeQueueMax;
    private bool attacking;

    [HideInInspector]
    public Ability currentAbility;
    private float abilityCooldown = 0f;

    private Coroutine castCoroutine;
    private Tween meleeMoveTW;

    private void Start()
    {
        owner = GetComponent<GameCharacter>();
    }
    private void Update()
    {
        if (weaponCooldown > 0f) weaponCooldown -= Time.deltaTime;
        if (abilityCooldown > 0f) abilityCooldown -= Time.deltaTime;
    }
    public void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        meleeQueueMax = currentWeapon.maxCombo;
    }

    public void SetAbility(Ability ability)
    {
        currentAbility = ability;
    }

    public void ResetCooldowns(bool _weapons = true, bool _abilities = false)
    {
        if (_weapons) weaponCooldown = 0f;
        if (_abilities) abilityCooldown = 0f;
    }

    public void AttemptAttack()
    {
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Melee:
                MeleeHitScan();
                break;

            case WeaponType.Projectile:
                ProjectileAttack();
                break;

            case WeaponType.Raycast:
                RaycastAttack();
                break;
        }
    }

    public void StartAttack(int attacks = 1)
    {
        owner.isBusy = true;
        meleeQueue = attacks;
        currentMeleeQueue = 0;
        attacking = true;
        owner.animator.SetBool("attack", true);
    }

    public void AddAttack()
    {
        if (meleeQueue < meleeQueueMax)
        {
            meleeQueue++;
        }
    }

    public void EndAttack()
    {
        currentMeleeQueue = 0;
        attacking = false;
        owner.animator.SetBool("attack", false);
        owner.isBusy = false;

        weaponCooldown = currentWeapon.attackCooldown;
    }


    public void MeleeHitScan()
    {
        LayerMask layer = owner.enemyLayer;

        currentMeleeQueue++;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(owner.attackPosition.position, currentWeapon.range, layer);
        
        if (targetsHit.Length > 0)
        {
            for (int i = 0; i < targetsHit.Length; i++)
            {
                int randomDmg = Random.Range(currentWeapon.damageMin, currentWeapon.damageMax + 1);
                targetsHit[i].GetComponent<GameCharacter>().TakeDamage(randomDmg, owner);
            }
        }

        if (currentMeleeQueue >= meleeQueue) 
        {
            EndAttack(); 
        }
    }

    private void ProjectileAttack()
    {
        GameObject projectile = GameObject.Instantiate(currentWeapon.projectilePrefab, owner.attackPosition.position, Quaternion.identity);
        int randomDamage = Random.Range(currentWeapon.damageMin, currentWeapon.damageMax + 1);
        projectile.GetComponent<ProjectileScript>().LaunchProjectile(currentWeapon.attackSpeed, owner.currentDirection, false, randomDamage, owner);
        if (currentWeapon.hasEffect)
        {
            StatusEffect newEffect = currentWeapon.effect.CloneStatus(currentWeapon.effect);
            projectile.GetComponent<ProjectileScript>().SetAttachedStatus(newEffect);
        }

        EndAttack();

    }

    private void RaycastAttack()
    {
        RaycastHit2D hit = Physics2D.Raycast(owner.attackPosition.position, owner.currentDirection * transform.localScale.x, currentWeapon.range);

        if (hit)
        {
            hit.collider.TryGetComponent(out EnemyScript hitEnemy);
            if (hitEnemy != null)
            {
                hitEnemy.TakeDamage(Random.Range(currentWeapon.damageMin, currentWeapon.damageMax + 1));
                if (currentWeapon.hasEffect)
                {
                    StatusEffect newEffect = currentWeapon.effect.CloneStatus(currentWeapon.effect);
                    hitEnemy.GetComponent<CharacterStatus>().NewStatus(newEffect);
                }
            }
        }
    }

    public void AttemptAbility()
    {
        if (abilityCooldown > 0f || currentAbility == null) return;

        castCoroutine = StartCoroutine(CastAbility());
    }

    private IEnumerator CastAbility()
    {
        currentAbility.OnStartCasting(owner);

        owner.AbilityStarted();
        SetCastBar();
        abilityCooldown = currentAbility.cooldown;
        yield return new WaitForSeconds(currentAbility.castTime);
        currentAbility.ActivateAbility(owner);
        if (currentAbility.finishAbilityOnCast) owner.AbilityFinished();
    }

    private void SetCastBar()
    {
        GameObject castBar = GameManager.instance.castBarPool.GetPooledObject();
        castBar.SetActive(true);
        castBar.GetComponent<CastBarScript>().StartCastBar(currentAbility.castTime, transform, currentAbility.abilityIcon, currentAbility.abilityName);
    }

    public bool WeaponIsOnCooldown()
    {
        if (weaponCooldown > 0f) return true;
        else return false;
    }

    public void StartWeaponCooldown()
    {
        weaponCooldown = currentWeapon.attackCooldown;
    }

    public bool GetAbilityCooldown()
    {
        if (abilityCooldown > 0f) return true;
        else return false;
    }

    public void StopAttack()
    {
        meleeMoveTW.Complete();
    }

    public void SetSlashRotation(int comboPoint)
    {
        slashEffectRotation = Quaternion.Euler(owner.comboRotation[comboPoint]);

        switch (currentWeapon.weaponType)
        {
            case WeaponType.Melee:
                //slash effect
                GameObject slashVfx = GameManager.instance.swordSlashPool.GetPooledObject();
                slashVfx.transform.localScale = new Vector3(owner.currentDirection.x, 1f, 1f);
                slashVfx.transform.position = owner.attackPosition.position;
                slashVfx.transform.rotation = slashEffectRotation;
                slashVfx.SetActive(true);
                slashVfx.GetComponent<ParticleSystem>().Play();
                break;
        }
    }
}
