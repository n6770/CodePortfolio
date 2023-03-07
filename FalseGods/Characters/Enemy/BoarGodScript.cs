using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Cinemachine;

public class BoarGodScript : GameCharacter
{
    //DEV
    public bool forceAttack;
    public int attackToForce;

    [SerializeField] private UnityEvent onCombatBegin;
    [SerializeField] private UnityEvent onCombatEnded;
    [SerializeField] private CinemachineConfiner2D cmCamConfiner;

    private ScreenShake shaker;

    private Material mat;

    private Transform currentTarget;
    private float distanceToTarget;
    private float targetReachedRange;
    private bool targetReached;

    private bool canActivate;
    private bool movingLeft;
    private bool active;
    private bool combatActive;
    private int nextAttack; // 0= slam, 1= breath, 2= cast, 3= jump
    private int lastAttack = -1;
    [SerializeField] private GameObject dissolveParticles;

    //Attacks
    [SerializeField] private GameObject slamParticles;
    [SerializeField] private Transform slamPosition;
    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private AudioClip slamSound;

    [SerializeField] private GameObject breathParticles;
    [SerializeField] private Transform breathPosition;
    [SerializeField] private float breathRange;
    [SerializeField] private StatusEffect breathStatus;
    [SerializeField] private AudioClip breathSound;

    [SerializeField] private GameObject spellParticles;
    [SerializeField] private GameObject spellProjectileParticles;
    [SerializeField] private Transform spellPosition;
    [SerializeField] private AudioClip spellStartSound;
    [SerializeField] private AudioClip spellEndSound;

    [SerializeField] private GameObject jumpUpParticles;
    [SerializeField] private GameObject jumpDownParticles;
    [SerializeField] private GameObject anticipateImpact;
    [SerializeField] private Transform jumpPosition;
    [SerializeField] private float jumpRange;
    [SerializeField] private int jumpDamage;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip jumpImpactSound;

    [SerializeField] private GameObject projectilePrefab;
    private int currentProjectile;
    [SerializeField] private int spellProjectilesToSpawn;
    [SerializeField] private float spellProjectileSpawnOffset;

    //UI
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthFrame;
    [SerializeField] private TextMeshProUGUI bossName;

    private void Awake()
    {
        InitializeCharacter();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        mat = GetComponentInChildren<SpriteRenderer>().material;
        shaker = GameManager.instance.GetComponent<ScreenShake>();
    }

    private void Update()
    {
        if (!active || health.isDead) return;
        if (!targetReached)
        {
            Scan();
            Move();
        }
        if (targetReached && !isBusy) StartCoroutine(TargetReached());
    }

    private void ChooseAction()
    {
        SoundManager.instance.PlayRandomSound(character.attackSounds, audioSource, 0.25f);

        targetReached = false;

        nextAttack = GetRandomAttackIndex();

        while(nextAttack == lastAttack)
        {
            nextAttack = GetRandomAttackIndex();
        }

        if (forceAttack) nextAttack = attackToForce;
        lastAttack = nextAttack;
        isBusy = false;
    }

    private int GetRandomAttackIndex()
    {
        int index = 0;

        float random = Random.value;

        if (random < 0.15f)
        {
            index = 3;
            targetReached = true;
        }
        else if (random < 0.50f)
        {
            index = 2;
            targetReached = true;
        }
        else if (random < 0.75f)
        {
            index = 1;
            targetReachedRange = 3f;
        }
        else
        {
            index = 0;
            targetReachedRange = 2f;
        }

        return index;
    }
    private IEnumerator TargetReached()
    {
        FacePlayer();
        isBusy = true;
        float waitTime = Random.Range(0.4f, 0.7f);
        yield return new WaitForSeconds(waitTime);

        animator.SetTrigger("attack" + nextAttack.ToString());
        
        if(nextAttack == 3) StartCoroutine(JumpHandler());
    }

    private void Scan()
    {
        currentTarget = PlayerGroupController.instance.master.position.x > transform.position.x ? PlayerGroupController.instance.enemyTargetLeft : PlayerGroupController.instance.enemyTargetRight;
        distanceToTarget = Mathf.Abs(transform.position.x - currentTarget.position.x);
        if (distanceToTarget < targetReachedRange && !targetReached) targetReached = true;
        currentDirection = GetDirection();
    }

    private void Move()
    {
        if (movingLeft && currentDirection.x < 0f) Flip();
        if (!movingLeft && currentDirection.x > 0f) Flip();
        
        if (!targetReached)
        {
            transform.Translate(moveSpeed * Time.deltaTime * currentDirection);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    private void Flip()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        movingLeft = !movingLeft;
    }

    private Vector2 GetDirection()
    {
        Vector2 returnVector = Vector2.right;
        if (transform.position.x > currentTarget.position.x) returnVector = Vector2.left;
        return returnVector;
    }

    public override void TakeDamage(int damageAmount, GameCharacter attacker = null)
    {
        if (!canActivate) return;

        if (!active && !combatActive) StartCoroutine(ActivateCoroutine());
        if (health.isDead) return;

        health.TakeDamage(damageAmount);
        if (attacker != null) attacker.actionManager.currentWeapon.PlayHitSound(audioSource);
        if (health.isDead) BossKilled();

        healthFill.fillAmount = health.GetHealthPercent();
    }

    private void BossKilled()
    {
        StopAllCoroutines();
        healthFill.DOFade(0f, 4f);
        healthFrame.DOFade(0f, 4f);
        bossName.DOFade(0f, 4f);
        animator.SetBool("dead", true);
        mat.DOFloat(1f, "_FadeAmount", 20f);
        GameObject dissolvePart = Instantiate(dissolveParticles);
        dissolvePart.transform.position = transform.position + new Vector3(4f * transform.localScale.x, 1f, 0f);

        BoarGodSpellProjectile[] currentProjcetiles = FindObjectsOfType<BoarGodSpellProjectile>();
        foreach (BoarGodSpellProjectile currentProjcetile in currentProjcetiles)
        {
            currentProjcetile.damage = 0;
        }
        foreach (PlayerCharacter player in PlayerGroupController.instance.allMembers)
        {
            player.status.ClearAll();
        }

        cmCamConfiner.enabled = false;
        onCombatEnded.Invoke();
    }


    private IEnumerator ActivateCoroutine()
    {
        combatActive = true;

        SoundManager.instance.PlayRandomSound(character.aggroSounds, audioSource, 0.5f);

        cmCamConfiner.enabled = true;
        onCombatBegin.Invoke();

        healthFill.DOFade(1f, 2f);
        healthFrame.DOFade(1f, 2f);
        bossName.DOFade(1f, 2f);

        yield return new WaitForSeconds(2f);

        active = true;
        ChooseAction();
    }

    //
    // ATTACKS
    //
    public void Attack0() //slam
    {
        //particle
        GameObject slamPart = Instantiate(slamParticles);
        slamPart.transform.position = slamPosition.position;

        //screen shake
        shaker.StartShake(0.5f, 2f);

        //damagescan
        Collider2D[] hits = Physics2D.OverlapCircleAll(slamPosition.position, slamRange, enemyLayer);
        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                hit.GetComponent<IDamageable>().TakeDamage(slamDamage);
            }
        }

        //sfx
        SoundManager.instance.PlaySound(slamSound, audioSource, 0.2f);

        StartCoroutine(WaitForNextAction(1f));
    }
    
    public void Attack1() //breath
    {
        //particle
        GameObject breathPart = Instantiate(breathParticles);
        breathPart.transform.position = breathPosition.position;
        if (transform.localScale.x < 0) breathPart.transform.rotation = Quaternion.Euler(180f, -90f, 90f);

        //sfx
        SoundManager.instance.PlaySound(breathSound, audioSource, 0.25f);

        //statuseffectScan
        bool direction = transform.localScale.x < 0f;
        StartCoroutine(BreathScan(breathPosition.position, direction));

        StartCoroutine(WaitForNextAction(1.5f));
    }

    private IEnumerator BreathScan(Vector2 position, bool right)
    {
        yield return new WaitForSeconds(1f);
        Vector2 direction = right ? Vector2.right : Vector2.left;
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)breathPosition.position + Vector2.down, direction, breathRange, enemyLayer);
        Debug.DrawLine(breathPosition.position, breathPosition.position + (Vector3)direction * breathRange, Color.green, 2f);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.GetComponent<CharacterStatus>().NewStatus(breathStatus.CloneStatus(breathStatus));
            }
        }

    }
    public void Attack2()
    {
        print("spell");
        GameObject spellProjectilePart = Instantiate(spellProjectileParticles);
        spellProjectilePart.transform.position = spellPosition.position;

        audioSource.Stop();
        SoundManager.instance.PlaySound(spellEndSound, audioSource, 0.3f);

        //screen shake
        shaker.StartShake(0.5f, 2f);

        StartCoroutine(WaitForNextAction(3f));
    }
    public void SpellStart()
    {
        GameObject spellCastPart = Instantiate(spellParticles);
        spellCastPart.transform.position = spellPosition.position;

        SoundManager.instance.PlaySound(spellStartSound, audioSource, 0.2f);
    }
    public void SpellProjectiles()
    {
        currentProjectile = 0;
        StartCoroutine(ProjectileSpawnDelayed());
    }

    private IEnumerator ProjectileSpawnDelayed()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        float xRandom = Random.Range(-spellProjectileSpawnOffset, spellProjectileSpawnOffset);
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, 180f + xRandom);
        projectile.transform.position = new Vector3(PlayerGroupController.instance.master.position.x + xRandom, 20f, 0f);
        currentProjectile++;

        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

        if (currentProjectile < spellProjectilesToSpawn)
        {
            StartCoroutine(ProjectileSpawnDelayed());
        }
    }
    public void Attack3() //JumpDamage
    {
        //particle
        GameObject slamPart = Instantiate(slamParticles);
        slamPart.transform.position = jumpPosition.position;

        //screen shake
        shaker.StartShake(0.5f, 2f);

        //damagescan
        Collider2D[] hits = Physics2D.OverlapCircleAll(jumpPosition.position, jumpRange, enemyLayer);
        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                hit.GetComponent<IDamageable>().TakeDamage(jumpDamage);
            }
        }

        //sfx
        SoundManager.instance.PlaySound(jumpImpactSound, audioSource,0.25f);

        StartCoroutine(WaitForNextAction(1f));
    }

    private IEnumerator JumpHandler()
    {
        SoundManager.instance.PlaySound(jumpSound, audioSource, 0.3f);
        yield return new WaitForSeconds(.8f);
        float xRandom = Random.Range(-1f, 1f);
        transform.position = new Vector3(PlayerGroupController.instance.master.position.x + xRandom, transform.position.y, transform.position.z);
        GameObject hitAnticipate = Instantiate(anticipateImpact);
        RaycastHit2D hitPoint = Physics2D.Raycast(jumpPosition.position, Vector2.down, Mathf.Infinity, groundLayer);
        hitAnticipate.transform.position = hitPoint.point;
        //yield return new WaitForSeconds(0.2f);
        SoundManager.instance.PlaySound(jumpSound, audioSource, 0.3f);
        animator.SetTrigger("jumpTrigger");
    }

    public void JumpUp()
    {
        transform.DOMoveY(transform.position.y + 15f, 0.5f).SetEase(Ease.Linear);
    }
    public void JumpDown()
    {
        transform.DOMoveY(transform.position.y - 15f, 0.5f).SetEase(Ease.Linear);
    }


    public void AttackFinished()
    {
        animator.SetTrigger("attackFinished");
        bool idle2 = Random.value < 0.5f;
        animator.SetBool("idle2", idle2);
    }

    private IEnumerator WaitForNextAction(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ChooseAction();
    }

    public void DissolveSprite(float endValue, float time)
    {
        Ease ease = endValue > 0.75f ? Ease.OutSine : Ease.InSine;
        mat.DOFloat(endValue, "_FadeAmount", time).SetEase(ease);
    }

    private void FacePlayer() 
    {
        currentTarget = PlayerGroupController.instance.master.position.x > transform.position.x ? PlayerGroupController.instance.enemyTargetLeft : PlayerGroupController.instance.enemyTargetRight;
        distanceToTarget = Mathf.Abs(transform.position.x - currentTarget.position.x);

        if (PlayerGroupController.instance.master.position.x > transform.position.x && !movingLeft) Flip();
        if (PlayerGroupController.instance.master.position.x < transform.position.x && movingLeft) Flip();
        
        if (nextAttack < 3 && distanceToTarget < 1.5f) JumpBack();
    }

    private void JumpBack()
    {
        transform.DOMoveX(transform.position.x + transform.localScale.x * 2f, 0.5f);
    }

    public void SetCanActivateTrue()
    {
        canActivate = true;
    }
}
