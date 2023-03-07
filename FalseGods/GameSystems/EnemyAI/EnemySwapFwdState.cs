using UnityEngine;

public class EnemySwapFwdState : EnemyBaseState
{
    private float swapTargetX;

    public override void EnterState(EnemyCharacter character)
    {
        EnemyCharacter friendlyFront = character.ScanFriendly(new Vector2(character.currentDirection.x, 0f));
        if (friendlyFront == null) friendlyFront = character.ScanFriendly(new Vector2(-character.currentDirection.x, 0f));

        swapTargetX = friendlyFront.transform.position.x;
        character.targetPositionX = swapTargetX;
    }

    public override void UpdateState(EnemyCharacter character)
    {
        float distance = swapTargetX - character.transform.position.x;
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
