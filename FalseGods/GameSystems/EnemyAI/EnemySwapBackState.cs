using UnityEngine;

public class EnemySwapBackState : EnemyBaseState
{
    private float swapTargetX;

    public override void EnterState(EnemyCharacter character)
    {
        EnemyCharacter friendlyBack = character.ScanFriendly(new Vector2(-character.currentDirection.x, 0f));
        if (friendlyBack == null) friendlyBack = character.ScanFriendly(new Vector2(character.currentDirection.x, 0f));

        swapTargetX = friendlyBack.transform.position.x;
        character.targetPositionX = swapTargetX;
    }

    public override void UpdateState(EnemyCharacter character)
    {
        character.targetPositionX = swapTargetX;
        float distance = character.transform.position.x - swapTargetX;
        character.swapTimer = 3f;

        if (Mathf.Abs(distance) < 0.1f)
        {
            character.SwitchState(character.moveRangeState);
        }
        else
        {
            character.MoveCharacter();
        }
    }

    public override void CancelState(EnemyCharacter character)
    {

    }
}
