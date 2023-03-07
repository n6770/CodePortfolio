using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScript : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerHealthBar healthBar;
    [SerializeField] private PlayerXpBar xpBar;
    [SerializeField] private Transform gfxTransform;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteFlasher flasher;
    [SerializeField] private float immuneTime;
    [SerializeField] private TextMeshProUGUI deathInfoText;
    [SerializeField] private ParticleSystem walkParticles;
    [SerializeField] private GameObject pausePanel;

    public List<WeaponInst> weapons;
    private float[] cooldownList;
    [SerializeField] private ObjectPool projectilePool;
    [SerializeField] private ObjectPool swingPool;
    [SerializeField] private ObjectPool rotatorPool;
    [SerializeField] private int[] killTracker;

    public float movementSpeed;
    public float health;
    public float maxHealth;

    private float startHealth;
    private float startSpeed;

    public bool paused;
    public bool canPause = true;

    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;

    private float currentImmuneTime;
    private bool immune;

    private int level = 1;
    private float currentXp = 0f;
    private float nextLevelXp = 6f;


    private bool alive = true;

    private Sprite idleSprite;
    private Sprite[] walkSprites;
    private float spriteSpeedElapsed;
    private float spriteSpeed = 0.2f;
    private int currentFrame = 0;
    private bool spriteFacingRight;

    private float physicalDmgBonus = 0f;
    private float spellDmgBonus = 0f;

    private Vector2 moveDirection;
    private Vector2 lastDirection;
    private Vector2 closestEnemyDirection;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetUpPlayer(playerCharacter);
        GameManager.instance.upgradeScript.UpdateStatsText();
        killTracker = new int[GameManager.instance.database.baseWeapons.Length];
    }

    
    private void Update()
    {
        if (!alive) return;
        Inputs();
        MovePlayer();
        SetSprite();
        SetSpriteDirection();
        AttemptAttack();
        SetTimers();

        if (Input.GetKeyDown(KeyCode.F9)) PickupXP(500);
    }

    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if (paused && Input.GetKeyDown(KeyCode.X))
        {
            GameManager.instance.levelManager.LoadMenuScene();
        }
    }

    private void TogglePause()
    {
        if(!canPause) return;
        paused = !paused;
        if (paused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void SetTimers()
    {
        if (currentImmuneTime < immuneTime)
        {
            immuneTime += Time.deltaTime;
        }
        else if (currentImmuneTime > immuneTime && immune)
        {
            immune = false;
        }
    }

    private void AttemptAttack()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (cooldownList[i] > 0f) cooldownList[i] -= Time.deltaTime;
            else
            {
                Vector2 directionToSpawn = Vector2.zero;
                switch (weapons[i].weaponDirection)
                {
                    case WeaponDirection.LastMove:
                        directionToSpawn = lastDirection;
                        break;
                    case WeaponDirection.ClosestEnemy:
                        FindClosestEnemy(weapons[i].range);
                        directionToSpawn = closestEnemyDirection;
                        break;
                    case WeaponDirection.Random:
                        directionToSpawn = Random.insideUnitCircle.normalized;
                        break;
                }

                if (directionToSpawn != Vector2.zero) SpawnWeapon(weapons[i], directionToSpawn);
                cooldownList[i] = weapons[i].cooldown;
            }
        }
    }

    private void MovePlayer()
    {
        transform.Translate(moveDirection.normalized * Time.deltaTime * movementSpeed);

        Vector2 boundsPos = transform.position;
        if (transform.position.y > 24f) boundsPos.y = 24f;
        else if (transform.position.y < -24f) boundsPos.y = -24f;
        if (transform.position.x > 24f) boundsPos.x = 24f;
        else if (transform.position.x < -24f) boundsPos.x = -24f;

        transform.position = boundsPos;

        if (moveDirection.magnitude > 0.1f && !walkParticles.isEmitting) walkParticles.Play();
        else if (moveDirection.magnitude < 0.1f && walkParticles.isEmitting) walkParticles.Stop();
    }

    private void SetSprite()
    {
        if (!alive) return;
        moveDirection = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (moveDirection == Vector2.zero && Input.GetMouseButton(0))
        {
            //mouseinput
            Vector2 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (((Vector2)transform.position - mousepos).magnitude > 0.5f) moveDirection = -((Vector2)transform.position - mousepos).normalized;
        }

        if (moveDirection != Vector2.zero)
        {
            lastDirection = moveDirection;
            if (moveDirection != Vector2.zero)
            {
                spriteSpeedElapsed += Time.deltaTime;
                if (spriteSpeedElapsed > spriteSpeed)
                {
                    spriteSpeedElapsed = 0f;

                    if (currentFrame == playerCharacter.walkSprites.Length - 1) currentFrame = 0;
                    else currentFrame++;

                    spriteRenderer.sprite = playerCharacter.walkSprites[currentFrame];
                }
            }
            else if (spriteRenderer.sprite != idleSprite) spriteRenderer.sprite = idleSprite;
        }
    }

    private void SetSpriteDirection()
    {
        if (moveDirection.x > 0f && gfxTransform.localScale.x > 0f) Flip();
        if (moveDirection.x < 0f && gfxTransform.localScale.x < 0f) Flip();
    }

    private void FindClosestEnemy(float range)
    {
        Vector2 newClosestEnemy = Vector2.zero;
        float nearestDistance = float.MaxValue;
        float distance;

        Collider2D[] enemiesNear = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (enemiesNear.Length > 0)
        {
            for (int i = 0; i < enemiesNear.Length; i++) 
            {
                distance = Vector2.SqrMagnitude(transform.position - enemiesNear[i].transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    newClosestEnemy = -(transform.position - enemiesNear[i].transform.position).normalized;
                }
            }
        }
        else newClosestEnemy = Vector2.zero;
        closestEnemyDirection = newClosestEnemy;
    }

    private void Flip()
    {
        gfxTransform.localScale = new Vector3(-gfxTransform.localScale.x, gfxTransform.localScale.y, gfxTransform.localScale.z);
    }

    public void TakeDamage(float damage, EnemyCharacter attacker)
    {
        SoundManager.instance.PlaySound(SoundManager.instance.playerDamageSounds[Random.Range(0, SoundManager.instance.playerDamageSounds.Length)], 1f);

        health -= damage;
        healthBar.SetFill(health, maxHealth);

        currentImmuneTime = 0f;
        immune = true;

        GameManager.instance.upgradeScript.UpdateStatsText();

        if (health > 0f)
        {
            flasher.StartFlashing(spriteRenderer);
        }
        else if (alive)
        {
            alive = false;
            PlayerKilled(attacker);
        }
    }

    private void PlayerKilled(EnemyCharacter killer)
    {
        deathInfoText.text =
            "Level reached - " + level.ToString() +
            "\nKilled by - " + killer.enemyName;
        GameManager.instance.OpenUIElement(GameManager.instance.deathPanel);
        Time.timeScale = 0f;
    }

    private void SpawnWeapon(WeaponInst weapon, Vector2 direction)
    {
        ObjectPool poolToUse = null;
        switch (weapon.weaponMovement)
        {
            case WeaponMovement.Projectile:
                poolToUse = projectilePool;
                break;
            case WeaponMovement.Swing:
                poolToUse = swingPool;
                break;
            case WeaponMovement.Rotator:
                poolToUse = rotatorPool;
                break;
        }

        SoundManager.instance.PlaySound(weapon.baseWeapon.weaponSound, 0.5f);

        bool switchDirection = false;
        int cycle = 1;
        for (int i = 0; i < weapon.projectileAmount; i++)
        {
            float angle = weapon.projectileAngle;
            
            GameObject weaponObj = poolToUse.GetPooledObject();
            if (weaponObj != null)
            {
                weaponObj.SetActive(true);
                weaponObj.transform.position = transform.position;

                //if projectileamount is even number
                if (weapon.projectileAmount % 2 == 0)
                {
                    if (switchDirection) angle = -angle;
                    if (i < 2)
                    {
                        angle = angle / 2;
                    }
                    else
                    {
                        angle = (angle / 2) + (angle * (cycle - 1));
                    }
                    weaponObj.GetComponent<IWeapon>().ActivateWeapon(direction, weapon, angle);

                    switchDirection = !switchDirection;
                    if ((i + 1) % 2 == 0) cycle++;
                }
                //if projectileamount is odd number
                else
                {
                    if (i == 0) weaponObj.GetComponent<IWeapon>().ActivateWeapon(direction, weapon, 0f);
                    else
                    {
                        angle = angle * cycle;
                        if (switchDirection) angle = -angle;
                        weaponObj.GetComponent<IWeapon>().ActivateWeapon(direction, weapon, angle);
                        switchDirection = !switchDirection;
                        if (i % 2 == 0) cycle++;
                    }
                }
            }
        }

    }

    public void SetUpPlayer(PlayerCharacter playerCharacterSO)
    {
        playerCharacter = playerCharacterSO;
        movementSpeed = playerCharacterSO.speed;
        health = playerCharacterSO.health;
        maxHealth = health;
        idleSprite = playerCharacterSO.idleSprite;
        walkSprites = playerCharacterSO.walkSprites;
        spriteFacingRight = playerCharacterSO.spriteFacingRight;

        startHealth = maxHealth;
        startSpeed = movementSpeed;

        weapons = new List<WeaponInst>();
        foreach (var weapon in playerCharacterSO.startingWeapons)
        {
            WeaponInst newWeapon = GameManager.instance.database.BuildWeaponInst(weapon);
            weapons.Add(newWeapon);
        }

        cooldownList = new float[weapons.Count];
        for (int i = 0; i < cooldownList.Length; i++)
        {
            cooldownList[i] = weapons[i].cooldown;
        }

        if (spriteFacingRight) spriteRenderer.flipX = true;
        spriteRenderer.sprite = idleSprite;
    }

    public void RefreshCooldownList()
    {
        cooldownList = new float[weapons.Count];
        for (int i = 0; i < cooldownList.Length; i++)
        {
            cooldownList[i] = weapons[i].cooldown;
        }

        if (weapons.Count == GameManager.instance.database.baseWeapons.Length)
        {
            GameManager.instance.upgradeScript.canGetNewWeapon = false;
        }

    }

    public void PickupHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        healthBar.SetFill(health, maxHealth);
        GameManager.instance.upgradeScript.UpdateStatsText();
    }

    public void PickupXP(float amount)
    {
        currentXp += amount;
        xpBar.SetXpFill(currentXp, nextLevelXp);
        if (currentXp >= nextLevelXp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXp -= nextLevelXp;
        nextLevelXp *= 1.5f;
        level++;
        GameManager.instance.PlayerLevelUp(level);
        SoundManager.instance.PlaySound(SoundManager.instance.levelUpSound, 0.5f);
        xpBar.SetXpFill(currentXp, nextLevelXp);
    }

    public void UpdateStats()
    {
        maxHealth = (startHealth + (level * 2f)) * healthMultiplier;
        movementSpeed = (startSpeed + (level * 0.2f)) * speedMultiplier;
    }
}
