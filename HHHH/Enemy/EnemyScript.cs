using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //General
    [SerializeField] private EnemyCharacter enemyCharacter;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteFlasher flasher;

    //Movement
    private float movementSpeed;
    private float turnSpeed;
    private float localAvoidanceRadius;
    public Vector2 moveDirection;
    public Vector2 localAvoidance;
    public Vector2 lastAvoidance;
    private List<Vector2> avoidanceList;

    //Combat
    private float health;
    private float damage;

    //Sprites
    private float spriteSpeed = 0.3f;
    private float spriteSpeedElapsed;
    private int currentFrame = 0;
    private bool spriteFacingLeft;

    private bool alive = true;

    private void Start()
    {
        avoidanceList = new List<Vector2>();
        SetupEnemy(enemyCharacter);
    }

    private void Update()
    {
        SetSprite();
        MoveTowardsPlayer();
    }

    private void SetSprite()
    {
        if (!alive) return;
        spriteSpeedElapsed += Time.deltaTime;
        if (spriteSpeedElapsed > spriteSpeed)
        {
            spriteSpeedElapsed = 0f;

            if (currentFrame == enemyCharacter.sprites.Length - 1) currentFrame = 0;
            else currentFrame++;

            spriteRenderer.sprite = enemyCharacter.sprites[currentFrame];
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!alive) return;
        Vector2 playerDirection = Vector2.Lerp(moveDirection, transform.position - GameManager.instance.player.transform.position, turnSpeed * Time.deltaTime);

        if (Time.frameCount % 5 == 0) localAvoidance = LocalAvoidance();
        Vector2 directionToMove = ((playerDirection.normalized) - (localAvoidance)).normalized;
        transform.Translate(-movementSpeed * Time.deltaTime * directionToMove);
        SetSpriteDirection();
        moveDirection = directionToMove;
    }

    private void SetSpriteDirection() 
    {
        if (GameManager.instance.player.transform.position.x < transform.position.x && transform.localScale.x > 0f) Flip();
        if (GameManager.instance.player.transform.position.x > transform.position.x && transform.localScale.x < 0f) Flip();
    }

    private void Flip()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            GameManager.instance.player.TakeDamage(damage, enemyCharacter);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        SpawnBlood();
        if (health <= 0f && alive)
        {
            KillEnemy();
        }
        else
        {
            if(gameObject.activeSelf) flasher.StartFlashing(spriteRenderer);
        }
    }

    private void SpawnBlood()
    {
        GameObject bloodObj = GameManager.instance.bloodPool.GetPooledObject();
        if (bloodObj != null)
        {
            bloodObj.SetActive(true);
            bloodObj.transform.position = transform.position;
            bloodObj.GetComponent<ParticleSystem>().Play();
        }

    }

    private void KillEnemy()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.enemyDamageSounds[Random.Range(0, SoundManager.instance.enemyDamageSounds.Length)], 0.2f);

        if (enemyCharacter.healthDrop != null && Random.value < enemyCharacter.healthDropChance)
        {
            GameObject healthPickup = GameManager.instance.pickupPool.GetPooledObject();
            healthPickup.transform.position = gameObject.transform.position;
            healthPickup.SetActive(true);
            healthPickup.GetComponent<Pickup>().ActivateObject(enemyCharacter.healthDrop);
        }
        else if (enemyCharacter.xpDrop != null && Random.value < enemyCharacter.xpDropChance)
        {
            GameObject xpPickup = GameManager.instance.pickupPool.GetPooledObject();
            xpPickup.transform.position = gameObject.transform.position;
            xpPickup.SetActive(true);
            xpPickup.GetComponent<Pickup>().ActivateObject(enemyCharacter.xpDrop);
        }


        alive = false;
        gameObject.SetActive(false);
    }

    private Vector2 LocalAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        avoidanceList.Clear();

        Collider2D[] nearEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayer);
        if (nearEnemies.Length > 1) {
            for (int i = 0; i < nearEnemies.Length; i++)
            {
                avoidanceList.Add((transform.position - nearEnemies[i].transform.position) - (transform.position - GameManager.instance.player.transform.position).normalized);
            }

            for (int i = 0; i < avoidanceList.Count; i++)
            {
                avoidance += avoidanceList[i];
            }
            avoidance = Vector2.Lerp(lastAvoidance, avoidance, turnSpeed * Time.deltaTime).normalized;
        }

        lastAvoidance = avoidance;
        return avoidance;
    }

    public void SetupEnemy(EnemyCharacter enemyCharacterSO)
    {
        enemyCharacter = enemyCharacterSO;
        currentFrame = 0;
        alive = true;
        health = enemyCharacterSO.health * Random.Range(0.9f, 1.1f);
        damage = enemyCharacterSO.damage * Random.Range(0.9f, 1.1f);
        spriteSpeed = enemyCharacter.animationSpeed;
        spriteFacingLeft = enemyCharacterSO.spriteFacingLeft;
        movementSpeed = enemyCharacterSO.speed * Random.Range(0.9f, 1.1f);
        turnSpeed = enemyCharacterSO.turnSpeed * Random.Range(0.9f, 1.1f);
        localAvoidanceRadius = enemyCharacterSO.localAvoidanceRadius;
        flasher.SetNormal(spriteRenderer);
    }
}
