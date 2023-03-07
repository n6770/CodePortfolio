using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

public class EnemyCharacter : GameCharacter
{
    private Rigidbody2D rb;
    private EnemyController controller;
    private Transform gfxObject;

    public bool isParrying;
    public bool isMelee;
    public bool isRanged;
    public bool isKnockedBack;

    public float targetPositionX;
    public bool leftSpawn;

    [SerializeField] private float rayCastOffset;

    //AI states
    public bool isActive;
    private AIType aiType;
    public float swapTimer;
    
    private EnemyBaseState currentState;

    public EnemyIdleState idleState = new EnemyIdleState();
    public EnemyMoveToRangeState moveRangeState = new EnemyMoveToRangeState();
    public EnemySwapFwdState swapFwdState = new EnemySwapFwdState();
    public EnemySwapBackState swapBackState = new EnemySwapBackState();
    public EnemyAbilityState abilityState = new EnemyAbilityState();
    public EnemyAttackState attackState = new EnemyAttackState();
    public EnemyStunnedState stunnedState = new EnemyStunnedState();

    private void Awake()
    {
        InitializeCharacter();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        gfxObject = transform.Find("SpriteRenderer.Animator");
    }

    private void Start()
    {
        useFlip = true;
        controller = EnemyController.instance;
        SetVariables();
    }

    private void SetVariables()
    {
        aiType = character.aiType;
        currentState = idleState;
        actionManager.SetWeapon(character.startingWeapon);
        if (character.abilities.Count > 0) actionManager.SetAbility(character.abilities[0]);

        switch (actionManager.currentWeapon.weaponType)
        {
            case WeaponType.Melee:
                isMelee = true;
                break;
            case WeaponType.Projectile:
                isRanged = true;
                break;
        }


        currentDirection = Vector2.left;
        animator.SetFloat("direction", 0f);
        if (PlayerGroupController.instance.firstMemberInLine.transform.position.x > transform.position.x)
        {
            Flip();
        }
    }

    public void ActivateEnemy()
    {
        isActive = true;
        currentState = idleState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        if (currentState == null) return;
        if (health.isDead) return;
        if (isKnockedBack) return;
        currentState.UpdateState(this);
        swapTimer -= Time.deltaTime;
    }

    public void SwitchState(EnemyBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public void CancelCurrentState()
    {
        currentState.CancelState(this);
    }

    public bool PlayerIsInWeaponRange()
    {
        bool inRange = Physics2D.Raycast(transform.position, currentDirection, actionManager.currentWeapon.range + 0.5f, enemyLayer);

        return inRange;
    }

    public void AddEnemyToCombat()
    {
        EnemyController.instance.AddEnemyToCombat(this);
        ActivateEnemy();
    }

    public void MoveCharacter()
    {
        bool _move = false;
        float distance = transform.position.x - targetPositionX;

        if (Mathf.Abs(distance) > 0.1f)
        {
            _move = true;
        }
        else
        {
            _move = false;
        }

        if (_move && !isBusy && hasControl)
        {
            if (transform.position.x > targetPositionX)
            {
                rb.velocity = Vector2.left * moveSpeed;
            }
            else if (transform.position.x < targetPositionX)
            {
                rb.velocity = Vector2.right * moveSpeed;
            }
            animator.SetBool("isWalking", true);
        }
        else
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }

    }

    public void Attack()
    {
        if (!hasControl) return;
        actionManager.StartAttack(Random.Range(0, actionManager.currentWeapon.maxCombo + 1));
    }


    public void AttackTrigger()
    {
        actionManager.currentWeapon.PlaySwingSound(audioSource);
        actionManager.AttemptAttack();
    }

    public void IncomingAttack(float _windupTime)
    {
        if (health.isStaggered || isParrying || isBusy || !hasControl) return;

        if (Random.value > 0.8f)
        {
            currentState.CancelState(this);
            StartCoroutine(ParryCoroutine());
        }
    }

    private IEnumerator ParryCoroutine()
    {
        isParrying = true;
        animator.SetBool("parry", true);
        yield return new WaitForSeconds(0.5f);
        isParrying = false;
        animator.SetBool("parry", false);
    }

    public override void FinishAttack()
    {
        animator.SetTrigger("finishAttack");
        isBusy = false;
    }

    public void Flip()
    {
        gfxObject.localScale = new Vector3(-gfxObject.localScale.x, gfxObject.localScale.y, gfxObject.localScale.z);
        attackPosition.localPosition = new Vector3(-attackPosition.localPosition.x, attackPosition.localPosition.y, 0f);
        leftSpawn = !leftSpawn;
        currentDirection = new Vector2(-currentDirection.x, currentDirection.y);
    }

    public override void TakeDamage(int damageAmount, GameCharacter attacker)
    {
        if (health.isDead) return;

        //parry
        if (isParrying && actionManager.currentWeapon.canParry && attacker != null)
        {
            health.TakeDamage(0, "Parried");
            if (attacker != null)
            {
                StatusEffect parryEffect = new StatusEffect(StatusType.Parried, 0.5f, 1f, 0, true, false);
                attacker.status.NewStatus(parryEffect);
                attacker.actionManager.EndAttack();
                actionManager.currentWeapon.PlayParrySound(audioSource);

                //Parry effect
                GameObject parryVFX = GameManager.instance.parryEffectPool.GetPooledObject();
                parryVFX.transform.position = transform.position + new Vector3(transform.localScale.x * 1.5f, 1.8f, 0f);
                parryVFX.SetActive(true);
                parryVFX.GetComponent<ParticleSystem>().Play();

                //Counterattack?
                float attackRandom = Random.value;
                if (attackRandom > 0.5f)
                {
                    Attack();
                    attackState.attackTimer = Random.Range(actionManager.currentWeapon.attackCooldown, actionManager.currentWeapon.attackCooldown * 1.5f);
                }
            }
        }
        //no parry
        else
        {
            health.TakeDamage(damageAmount);
            if (attacker != null) attacker.actionManager.currentWeapon.PlayHitSound(audioSource);
            if (health.isDead) EnemyKilled();

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
        isParrying = false;
    }

    public override void CharacterStaggered(float _duration)
    {
        foreach (var trigger in animator.parameters)
        {
            if (trigger.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(trigger.name);
            }
        }

        StopAllCoroutines();
        StartCoroutine(StaggerCoroutine(_duration));
    }

    private IEnumerator StaggerCoroutine(float _duration)
    {
        status.NewStatus(new StatusEffect(StatusType.Weakness, _duration, 1f, 0.5f, true, false));

        currentState.CancelState(this);
        currentState = stunnedState;
        currentState.EnterState(this);
        animator.SetBool("stagger", true);
        health.SetStaggered(true);
        yield return new WaitForSeconds(_duration);
        currentState = idleState;
        currentState.EnterState(this);
        animator.SetBool("stagger", false);
        health.SetStaggered(false);
    }

    private void EnemyKilled()
    {
        EnemyController.instance.RemoveEnemyFromCombat(this);
        actionManager.StopAttack();
        rb.velocity = Vector3.zero;
        animator.SetBool("dead", true);
        isActive = false;
        GetComponent<Collider2D>().enabled = false;

        if (Random.value > 0.65f)
        {
            Instantiate(EnemyController.instance.healthOrbPrefab, transform.position, Quaternion.identity);
        }

        //death knockback
        float xRandom = Random.Range(1f, 2f);
        transform.DOMoveX(transform.position.x - currentDirection.x * xRandom, 0.5f);

        //Death VFX
        StartCoroutine(DeathVFX());
    }

    private IEnumerator DeathVFX()
    {
        Material mat = GetComponentInChildren<SpriteRenderer>().material;
        
        yield return new WaitForSeconds(1f);
        
        GameObject particles = GameManager.instance.dissolvePool.GetPooledObject();
        particles.transform.position = transform.position;
        particles.SetActive(true);
        particles.GetComponent<ParticleSystem>().Play();
        mat.DOFloat(1f, "_DissolveAmount", 3f).SetEase(Ease.Linear);
    }

    public EnemyCharacter ScanFriendly(Vector2 _direction)
    {
        RaycastHit2D _enemyHit = Physics2D.Raycast((Vector2)transform.position + _direction * rayCastOffset, _direction, 3f, friendlyLayer);

        Debug.DrawLine((Vector2)transform.position + _direction * rayCastOffset, (Vector2)transform.position + _direction * rayCastOffset + (Vector2)_direction * 3f, Color.red);

        if (_enemyHit.collider != null)
        {
            var hitChar = _enemyHit.collider.GetComponent<EnemyCharacter>();
            if (hitChar.hasControl) return hitChar;
            else return null;
        }
        else return null;
    }

    public EnemyCharacter[] ScanFriendliesAll(Vector2 _direction)
    {
        EnemyCharacter[] gameCharacters = null;
        RaycastHit2D[] enemies = Physics2D.RaycastAll((Vector2)transform.position + _direction * rayCastOffset, _direction, 3f, friendlyLayer);

        if (enemies.Length > 0)
        {
            gameCharacters = new EnemyCharacter[enemies.Length];

            for (int i = 0; i < enemies.Length; i++)
            {
                gameCharacters[i] = enemies[i].collider.GetComponent<EnemyCharacter>();
            }
        }

        return gameCharacters;
    }

    public List<bool> CheckForMeleesAll(EnemyCharacter[] arrayToCheck)
    {
        List<bool> result = new List<bool>();

        if (arrayToCheck != null)
        {
            for (int i = 0; i < arrayToCheck.Length; i++)
            {
                if (arrayToCheck[i].actionManager.currentWeapon.weaponType == WeaponType.Melee)
                {
                    result.Add(true);
                }
            }
        }

        return result;
    }
}
