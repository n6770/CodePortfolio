using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameCharacter : MonoBehaviour, IDamageable
{
    //
    //  BASE CLASS FOR PLAYER/ENEMY/NPC CHARACTERS
    //  note to self: abstract ens kerralla

    [HideInInspector] public Animator animator;
    [HideInInspector] public Health health;
    [HideInInspector] public CharacterStatus status;
    [HideInInspector] public ActionManager actionManager;

    public Character character;
    [HideInInspector] public Transform moveTarget;

    public LayerMask enemyLayer;
    public LayerMask friendlyLayer;
    public LayerMask bossLayer;

    public Vector3[] comboRotation;

    [HideInInspector] public Vector2 currentDirection;

    [HideInInspector] public CharacterType characterType;
    [HideInInspector] public bool isBusy;

    [HideInInspector] public AudioSource audioSource;

    [SerializeField] private bool hasSubanimator;
    [ShowIf("hasSubanimator", true)][SerializeField] private Animator subAnimator;

    public bool useFlip;
    public float moveSpeed;
    public Transform attackPosition;
    public bool hasControl = true;

    public void InitializeCharacter()
    {
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        status = GetComponent<CharacterStatus>();
        actionManager = GetComponent<ActionManager>();
        health.SetHealth(character.health);
        health.SetStaggerPoints(character.staggerPoint, character.staggerRegen);
        characterType = character.characterType;
    }

    public void SetBusy(bool state) { isBusy = state; }

    public virtual void TakeDamage(int damageAmount, GameCharacter attacker = null) { }

    public virtual void TakeHealing(int healAmount) { }

    public virtual void FinishAttack() { }

    public virtual void AbilityStarted() { }

    public virtual void AbilityFinished() { }

    public virtual void AbilityCancelled() { }

    public virtual void CharacterStaggered(float duration) { }

    public void PlayFootstep()
    {
        SoundManager.instance.PlayRandomSound(character.footsteps, audioSource, 0.1f);
    }
}
