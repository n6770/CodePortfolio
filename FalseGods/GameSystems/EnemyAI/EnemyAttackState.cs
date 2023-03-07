using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    public float attackTimer;
    private float evaluateDivisible = 5;

    public override void EnterState(EnemyCharacter character)
    {
        attackTimer = Random.Range(0f, 0.75f);
    }

    public override void UpdateState(EnemyCharacter character)
    {
        if (Time.frameCount % evaluateDivisible == 0)
        {
            EnemyCharacter friendlyFront = character.ScanFriendly(new Vector2(character.currentDirection.x, 0f));

            if (friendlyFront != null)
            {
                character.SwitchState(character.moveRangeState);
            }

            if (!character.PlayerIsInWeaponRange())
            {
                character.SwitchState(character.moveRangeState);
            }
        }

        if (attackTimer <= 0f && !character.isBusy)
        {
            character.Attack();
            attackTimer = Random.Range(character.actionManager.currentWeapon.attackCooldown, character.actionManager.currentWeapon.attackCooldown * 1.5f);
        }
        else if (!character.isBusy)
        {
            attackTimer -= Time.deltaTime;
        }

        character.MoveCharacter();
    }

    public override void CancelState(EnemyCharacter character)
    {
        attackTimer += 0.5f;
        character.animator.SetBool("attack", false);
    }
}
