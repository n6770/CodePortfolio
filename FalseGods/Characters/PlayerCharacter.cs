using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;


public class PlayerCharacter : GameCharacter
{
    private Rigidbody2D rb;
    private Collider2D capsuleCollider;
    private PlayerGroupController controller;

    //Commands
    private Vector2 moveVector;
    private bool dodge;
    private bool parry;
    private bool attack;
    private bool ability;

    //parry/dodge
    private bool isParrying;
    private bool isDodging;
    private bool isKnockedBack;
    private Coroutine parryCoroutine;
    private float immuneTime;

    [SerializeField]
    private float rayCastOffset;

    [HideInInspector]
    public int positionInGroupList;

    private bool facingRight = true;
    
    private void Awake()
    {
        InitializeCharacter();
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentDirection = Vector2.right;
        controller = PlayerGroupController.instance;
        SetVariables();
        controller.groupUI.SetHealthBar(health.GetHealthPercent(), positionInGroupList);
    }

    private void SetVariables()
    {
        moveSpeed = controller.characterMoveSpeed;
        moveTarget = controller.GiveMoveTarget(this);
        actionManager.SetWeapon(character.startingWeapon);
        if (character.abilities.Count > 0) actionManager.SetAbility(character.abilities[0]);
        positionInGroupList = controller.activeMembers.IndexOf(this);
        animator.SetFloat("direction", 1f);
    }

    public void GetCommand(bool _dodge, bool _parry, bool _attack, bool _ability)
    {
        dodge = _dodge;
        parry = _parry;
        attack = _attack;
        ability = _ability;
    }

    private void Update()
    {
        if (health.isDead) return;
        if (immuneTime > 0f) immuneTime -= Time.deltaTime;
        MoveToTarget();
        HandleCommand();
    }
    private void HandleCommand()
    {
        if (!hasControl) return;

        if (attack) Attack();
        if (ability) Ability();
        if (dodge && !isDodging) Dodge();
        if (parry && !isParrying) Parry();

        dodge = false;
        parry = false;
        attack = false;
    }
    private void MoveToTarget()
    {
        if (!hasControl || isKnockedBack) return;

        float distance = moveTarget.position.x - transform.position.x;
        float distanceAbs = Mathf.Abs(distance);

        if (distanceAbs > 0.2f)
        {
            if (distanceAbs > 0.5f) moveSpeed = 7.5f;
            else moveSpeed = 5f;

            moveVector.x = moveTarget.position.x - transform.position.x;
            moveVector.Normalize();
        }
        else
        {
            moveVector.x = 0f;
        }
        MoveCharacter(moveVector);
    }

    private void MoveCharacter(Vector2 _moveVector)
    {
        Vector2 velocity = new(_moveVector.x, 0f);
        if (!health.isDead || !hasControl || !isBusy)
        {
            rb.velocity = velocity.normalized * moveSpeed;
        }
        else rb.velocity = Vector2.zero;

        if (velocity.x > 0.1f && !facingRight) Flip();
        if (velocity.x < -0.1f && facingRight) Flip();

        if (Mathf.Abs(velocity.x) > 0.001f && !isKnockedBack) animator.SetBool("isWalking", true);
        else if (controller.inputVector.x != 0f && controller.Slowed()) animator.SetBool("isWalking", true);
        else animator.SetBool("isWalking", false);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector2 newDirection = currentDirection;
        newDirection.x *= -1;
        currentDirection = newDirection;
        if (!useFlip)
        {
            attackPosition.localPosition = new Vector3(-attackPosition.localPosition.x, attackPosition.localPosition.y, 0f);
            float animFloat = 0f;
            if (currentDirection.x > 0f) animFloat = 1f;
            animator.SetFloat("direction", animFloat);
        }
        else
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    public override void TakeDamage(int damageAmount, GameCharacter attacker)
    {
        if (health.isDead) return;
        if (isParrying && attacker != null)
        {
            health.TakeDamage(0, "Parried");
            if (attacker != null)
            {
                StatusEffect parryEffect = new StatusEffect(StatusType.Parried, 0.5f, actionManager.currentWeapon.parriedTime, 0, true, false);
                attacker.status.NewStatus(parryEffect);
                attacker.actionManager.EndAttack();
                actionManager.currentWeapon.PlayParrySound(audioSource);
                controller.GainOrLoseStamina(actionManager.currentWeapon.staminaGainParry);
                controller.GroupInstantIdle();
                SuccesfulParry();

                //Parry effect
                GameObject parryVFX = GameManager.instance.parryEffectPool.GetPooledObject();
                parryVFX.transform.position = transform.position + new Vector3(transform.localScale.x * 1.5f, 1.8f, 0f);
                parryVFX.SetActive(true);
                parryVFX.GetComponent<ParticleSystem>().Play();

                //timestop
                GameManager.instance.timeStopper.PlayerParryStop();
            }
        }
        else if (immuneTime <= 0f)
        {
            health.TakeDamage(damageAmount);
            if (attacker != null) attacker.actionManager.currentWeapon.PlayHitSound(audioSource);
            controller.groupUI.SetHealthBar(health.GetHealthPercent(), positionInGroupList);
            if (health.isDead) MemberStunned();
            immuneTime = 0.3f;

            //knockback
            if (attacker != null)
            {
                Vector2 attackerDirection = transform.position - attacker.transform.position;
                attackerDirection.Normalize();
                float knockbackDirection = transform.position.x + attackerDirection.x * 0.25f;
                isKnockedBack = true;
                transform.DOMoveX(knockbackDirection, 0.05f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).OnComplete(() => isKnockedBack = false);
            }
        }
    }

    private void SuccesfulParry()
    {
        CancelParry();
        actionManager.ResetCooldowns();
        Attack();
    }

    private void MemberStunned()
    {
        //death knockback
        float xRandom = Random.Range(1f, 2f);
        transform.DOMoveX(transform.position.x - currentDirection.x * xRandom, 0.5f);

        rb.velocity = Vector2.zero;
        actionManager.EndAttack();
        animator.SetBool("dead", true);
        capsuleCollider.enabled = false;
        controller.MemberStunned(this);
    }

    public void Revive()
    {
        animator.SetBool("dead", false);
        capsuleCollider.enabled = true;
    }

    public void SetIdle()
    {
        if (isBusy) return;
        animator.SetTrigger("idle");
        isParrying = false;
        actionManager.ResetCooldowns();
    }

    private void SwapPositionToFirst(bool last)
    {
        PlayerCharacter oldFirst = null;

        controller.GetFirstAndLast();

        int index = controller.activeMembers.IndexOf(this);

        if (!last) oldFirst = controller.firstMemberInLine;
        else
        {
            oldFirst = controller.lastMemberInLine;
        }

        if (oldFirst == null)
        {
            print("Member not found, aborting");
            return;
        }

        Transform tempTarget = oldFirst.moveTarget;
        oldFirst.moveTarget = moveTarget;
        moveTarget = tempTarget;
        controller.activeMembers[index].transform.DOMove(oldFirst.transform.position, 0.15f);
        oldFirst.hasControl = false;
        oldFirst.transform.DOMove(controller.activeMembers[index].transform.position, 0.15f).OnComplete(() => oldFirst.hasControl = true);

        controller.SwapGroupPositions(this, oldFirst);
    }

    private void Attack()
    {
        if (controller.groupStamina < actionManager.currentWeapon.staminaCostAttack) return;
        if (actionManager.WeaponIsOnCooldown()) return; 

        if (!isBusy)
        {
            if (actionManager.currentWeapon.weaponType == WeaponType.Melee)
            {
                if (controller.lastDirection.x > 0f)
                {
                    SwapPositionToFirst(false);
                }
                else if (controller.lastDirection.x < 0f)
                {
                    SwapPositionToFirst(true);
                }
            }

            actionManager.StartAttack();
        }
        else if (actionManager.currentWeapon.weaponType == WeaponType.Melee)
        {
            actionManager.AddAttack();
        }

        InformAttackToEnemy();
    }

    public void AttackTrigger()
    {
        actionManager.currentWeapon.PlaySwingSound(audioSource);
        actionManager.AttemptAttack();
    }


    private void InformAttackToEnemy()
    {
        Vector2 _direction = new Vector2(transform.localScale.x, 0f);

        RaycastHit2D _enemyHit = Physics2D.Raycast((Vector2)transform.position + _direction * rayCastOffset, _direction, actionManager.currentWeapon.range + 1f, enemyLayer);
        Debug.DrawLine((Vector2)transform.position + _direction * rayCastOffset, (Vector2)transform.position + _direction * rayCastOffset + _direction * 3f, Color.red);

        if (_enemyHit.collider != null)
        {
            if (_enemyHit.collider.TryGetComponent<EnemyCharacter>(out EnemyCharacter minionCharacter))
            {
                minionCharacter.IncomingAttack(actionManager.currentWeapon.windupTime);
            }
        }
    }

    private void Ability()
    {
        actionManager.AttemptAbility();
    }
    
    private void Dodge()
    {
        if (isDodging) return;

        SetDodgeState(true);
        transform.DOMoveX(transform.position.x + currentDirection.x * 6f, 0.5f).OnComplete(() => SetDodgeState(false));
    }
    private void Parry()
    {
        if (isParrying) return;

        if (actionManager.currentWeapon.canParry && !isBusy)
        {
            parryCoroutine = StartCoroutine(ParryCoroutine());
        }
    }

    private IEnumerator ParryCoroutine()
    {
        SoundManager.instance.PlayRandomSound(SoundManager.instance.parryClips, audioSource, 0.5f);
        animator.SetBool("parry", true);
        SetParryState(true);
        yield return new WaitForSeconds(actionManager.currentWeapon.parryTimeWindow);
        animator.SetBool("parry", false);
        SetParryState(false);
    }

    private void CancelParry()
    {
        StopCoroutine(parryCoroutine);
        animator.SetBool("parry", false);
        isBusy = false;
        SetParryState(false);
    }

    public override void AbilityStarted()
    {
        isBusy = true;
        controller.groupUI.StartCooldown(positionInGroupList);
        animator.SetTrigger("abilityStart");
    }

    public override void AbilityFinished()
    {
        isBusy = false;
        animator.SetTrigger("abilityFinish");
    }

    public void SetParryState(bool _state)
    {
        isParrying = _state;
        isBusy = _state;
    }

    public void SetDodgeState(bool _state)
    {
        isDodging = _state;
        isBusy = _state;
        animator.SetBool("dodge", _state);

    }

    public override void TakeHealing(int healAmount)
    {
        health.TakeHealing(healAmount);
        controller.groupUI.SetHealthBar(health.GetHealthPercent(), positionInGroupList);
    }

    public void Resurrect()
    {
        StartCoroutine(ResurrectCR());

        IEnumerator ResurrectCR()
        {
            animator.SetBool("dead", false);

            yield return new WaitForSeconds(1.5f);
            
            capsuleCollider.enabled = true;
            health.isDead = false;
            health.TakeHealing(20);
            controller.groupUI.SetHealthBar(health.GetHealthPercent(), positionInGroupList);
            controller.AddToGroup(this);
            controller.NewTargets();
            controller.GetFirstAndLast();
            EnemyController.instance.SetMoveTargets();
        }
    }
    
}
