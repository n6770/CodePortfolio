using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public override void EnterState(EnemyCharacter character)
    {
    }

    public override void UpdateState(EnemyCharacter character)
    {
        if (character.isActive) character.SwitchState(character.moveRangeState);
    }

    public override void CancelState(EnemyCharacter character)
    {

    }
}
