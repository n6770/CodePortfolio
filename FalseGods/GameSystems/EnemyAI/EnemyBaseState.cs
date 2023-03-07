public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyCharacter character);
    public abstract void UpdateState(EnemyCharacter character);
    public abstract void CancelState(EnemyCharacter character);
}
