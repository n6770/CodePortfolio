using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveToRangeState : EnemyBaseState
{
    private int evaluateDivisible = 5;
    private float xRandom = 0f;
    private float newRandomTime = 0f;

    public override void EnterState(EnemyCharacter character)
    {
        if (!character.leftSpawn) character.targetPositionX = PlayerGroupController.instance.enemyTargetRight.position.x;
        else character.targetPositionX = PlayerGroupController.instance.enemyTargetLeft.position.x;
    }

    public override void UpdateState(EnemyCharacter character)
    {
        float offset = character.leftSpawn ? -2f : 2f;

        //Skannataan onko muita vihollisia edessä
        EnemyCharacter friendlyFront = character.ScanFriendly(new Vector2(character.currentDirection.x, 0f));
        
        //Timers
        if (newRandomTime <= 0f)
        {
            newRandomTime = NewRandomTime(3f, 6f);
            xRandom = NewXRandom(0.2f);
        }
        else newRandomTime -= Time.deltaTime;

        if (Time.frameCount % evaluateDivisible != 0) return;

        if (character.PlayerIsInWeaponRange() && friendlyFront == null)
        {
            character.SwitchState(character.attackState);
        }
        //AbilityTsekkaus!!!

        if (friendlyFront != null)
        {
            if (EvaluateSwap(character, friendlyFront) && !friendlyFront.isBusy)
            {
                if (friendlyFront.swapTimer > 0f) return;

                friendlyFront.CancelCurrentState();
                friendlyFront.SwitchState(friendlyFront.swapBackState);
                character.SwitchState(character.swapFwdState);
            }
            else
            {
                if (Vector2.Dot(friendlyFront.currentDirection, character.currentDirection) == -1f) //Hahmot katsoo toisiaan
                {
                    character.Flip();
                }

                character.targetPositionX = friendlyFront.transform.position.x + offset + xRandom;
            }
        }
        else
        {
            if (!character.leftSpawn)
            {
                if (character.transform.position.x < character.targetPositionX - 1f)
                {
                    character.Flip();
                }
                else
                {
                    character.targetPositionX = PlayerGroupController.instance.enemyTargetRight.position.x;
                }
            }
            else
            {
                if (character.transform.position.x > character.targetPositionX + 1f)
                {
                    character.Flip();
                }
                else
                {
                    character.targetPositionX = PlayerGroupController.instance.enemyTargetLeft.position.x;
                }
            }
        }

        character.MoveCharacter();
 
    }

    public override void CancelState(EnemyCharacter character)
    {

    }

    private float NewXRandom(float deviation)
    {
        return Random.Range(-deviation, deviation);
    }

    private float NewRandomTime(float min, float max)
    {
        return Random.Range(min, max);
    }

    private bool EvaluateSwap(EnemyCharacter owner, EnemyCharacter friendly)
    {
        bool ownerMelee = owner.isMelee;
        bool friendlyMelee = friendly.isMelee;

        float weight = 0f;

        if (ownerMelee && !friendlyMelee) weight += 0.5f;
        if (friendly.health.GetHealthPercent() < owner.health.GetHealthPercent()) weight += 0.01f;

        return Random.value < weight;
    }

}
