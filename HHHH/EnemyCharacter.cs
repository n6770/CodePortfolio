using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Character", menuName = "Halloween Hordes/Enemy Character", order = 2)]
public class EnemyCharacter : ScriptableObject
{
    public EnemyType enemyType;

    public string enemyName;
    public int levelToSpawn;
    public int levelToStopSpawning;
    public float speed = 5f;
    public float health;
    public float damage;
    public float turnSpeed;
    public Sprite[] sprites;
    public bool spriteFacingLeft;
    public float animationSpeed = 0.3f;
    public float localAvoidanceRadius;

    public PickupScriptableObject xpDrop;
    public float xpDropChance;
    public PickupScriptableObject healthDrop;
    public float healthDropChance;
}
