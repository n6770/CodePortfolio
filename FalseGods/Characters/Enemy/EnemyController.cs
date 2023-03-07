using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;

    private List<EnemyCharacter> enemiesInCombat = new();
    private List<EnemyCharacter> enemiesInRange = new();
    [Required]
    public Transform enemyMoveTargetRight;
    [Required]
    public Transform enemyMoveTargetLeft;

    [SerializeField] private float difficultyModifier = 1f;

    [HideInInspector] public float currentGlobalAttackCooldown;
    public float globalAttackCooldown = 0.5f;

    [HideInInspector] public float timeSinceLastAttack;

    public GameObject healthOrbPrefab;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (GameManager.instance.inCombat)
        {
            RunTimers();
        }
    }

    private void RunTimers()
    {
        if (currentGlobalAttackCooldown > 0f) currentGlobalAttackCooldown -= Time.deltaTime;
        if (GameManager.instance.inCombat) timeSinceLastAttack += Time.deltaTime;
    }

    public void GlobalCooldownTrigger()
    {
        currentGlobalAttackCooldown = globalAttackCooldown;
    }

    public void AddEnemyToCombat(EnemyCharacter enemy)
    {
        enemiesInCombat.Add(enemy);
        if (enemiesInCombat.Count > 0 && !GameManager.instance.inCombat)
        {
            CombatStarted();
        }
    }

    public void RemoveEnemyFromCombat(EnemyCharacter enemy)
    {
        int index = enemiesInCombat.IndexOf(enemy);
        enemiesInCombat.RemoveAt(index);
        if (enemiesInCombat.Count == 0)
        {
            CombatEnded();
        }
    }

    public void CombatStarted()
    {
        currentGlobalAttackCooldown = globalAttackCooldown;
        timeSinceLastAttack = 0f;
        GameManager.instance.inCombat = true;
    }

    private void CombatEnded()
    {
        GameManager.instance.inCombat = false;
        foreach(PlayerCharacter member in PlayerGroupController.instance.allMembers)
        {
            if (member != null)
            {
                if (member.health.isDead)
                {
                    member.Resurrect();
                }
            }
        }
    }

    public void SetMoveTargets()
    {
        int playersAliveCount = 0;
        for (int i = 0; i < PlayerGroupController.instance.activeMembers.Count; i++)
        {
            if (PlayerGroupController.instance.activeMembers[i] != null)
            {
                if (!PlayerGroupController.instance.activeMembers[i].health.isDead)
                {
                    playersAliveCount++;
                }
            }
        }
        if (playersAliveCount > 0) playersAliveCount--;
        enemyMoveTargetLeft.localPosition = new Vector3(playersAliveCount * -PlayerGroupController.instance.membersOffset - 2.5f, enemyMoveTargetLeft.localPosition.y, 0f);
    }

}

