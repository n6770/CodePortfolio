using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterStatus : MonoBehaviour
{
    private Health health;
    private GameCharacter owner;
    private readonly float tickRate = 0.1f;
    private WaitForSeconds statusTickRate;

    //Status effects
    [SerializeField]
    [BoxGroup("Status Effects")]
    private List<StatusEffect> effects = new();
    //Modifiers
    [BoxGroup("Modifiers")]
    [HorizontalGroup("Modifiers/Left")]
    public bool stunned;
    [HorizontalGroup("Modifiers/Left")]
    public bool silence;
    [HorizontalGroup("Modifiers/Right")]
    public bool cripple;
    [HorizontalGroup("Modifiers/Right")]
    public bool parried;

    [BoxGroup("Damage modifiers")]
    [GUIColor(1f, 0.4f, 0.4f)]
    public float weakness = 0.0f;
    [BoxGroup("Damage modifiers")]
    [GUIColor(0.4f, 1f, 0.4f)]
    public float warded = 0.0f;
    [BoxGroup("Damage modifiers")]
    [GUIColor(0.6f, 0.6f, 1f)]
    public bool shielded;
    [BoxGroup("Damage modifiers")]
    public float dmgModifier = 1.0f;

    private void Awake()
    {
        health = GetComponent<Health>();
        owner = GetComponent<GameCharacter>();
    }
    private void Start()
    {
        statusTickRate = new WaitForSeconds(tickRate);
        StartCoroutine(DepleteStatus());
    }

    private IEnumerator DepleteStatus()
    {
        List<StatusEffect> removeList = new();
        
        if (effects.Count > 0)
        {
            for(int i = 0; i < effects.Count; i++)
            {
                effects[i].timeRemaining -= tickRate;
                if (effects[i].timeRemaining <= effects[i].nextTick) 
                {
                    ApplyStatus(effects[i]); 
                    effects[i].nextTick = effects[i].timeRemaining - effects[i].tickRate;
                }
                if (effects[i].timeRemaining <= 0f)
                {
                    removeList.Add(effects[i]);
                    StatusExpired(effects[i]);
                }
            }
        }
        if(removeList.Count > 0)
        {
            for(int i =0; i < removeList.Count; i++)
            {
                int index = effects.IndexOf(removeList[i]);
                effects.RemoveAt(index);
            }
        }

        yield return statusTickRate;

        StartCoroutine(DepleteStatus());

    }

    private void ApplyStatus(StatusEffect _effect)
    {
        switch (_effect.type)
        {
            case StatusType.Bleed:
                owner.TakeDamage((int)_effect.amount);
                break;

            case StatusType.Stun:
                stunned = true;
                owner.hasControl = false;
                owner.animator.SetBool("stagger", true);
                owner.health.SetCombatText(0, "stunned", true);
                break;

            case StatusType.Heal:
                owner.TakeHealing((int)_effect.amount);
                break;

            case StatusType.Silence:
                silence = true;
                break;

            case StatusType.Cripple:
                cripple = true;
                break;

            case StatusType.Weakness:
                weakness += _effect.amount;
                break;

            case StatusType.Warded:
                warded -= _effect.amount;
                break;

            case StatusType.Shielded:
                shielded = true;
                break;

            case StatusType.Parried:
                parried = true;
                weakness += 0.5f;
                break;

        }
    }

    private void StatusExpired(StatusEffect _effect)
    {
        switch (_effect.type)
        {
            case StatusType.Bleed:
                break;

            case StatusType.Stun:
                stunned = false;
                owner.hasControl = true;
                owner.animator.SetBool("stagger", false);
                break;

            case StatusType.Heal:
                break;

            case StatusType.Silence:
                silence = false;
                break;

            case StatusType.Cripple:
                cripple = false;
                break;

            case StatusType.Weakness:
                weakness -= _effect.amount;
                break;

            case StatusType.Warded:
                warded += _effect.amount;
                break;

            case StatusType.Shielded:
                shielded = false;
                break;

            case StatusType.Parried:
                parried = false;
                weakness -= 0.5f;
                break;
        }
    }



public void NewStatus(StatusEffect _effect)
    {
        //Check if same status is already applied
        if (effects.Count > 0)
        {
            StatusEffect sameEffect = null;

            bool sameStatusFound = false;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].type == _effect.type)
                {
                    sameStatusFound = true;
                    sameEffect = effects[i];
                }
            }
            if (sameStatusFound)
            {
                if(_effect.type == StatusType.Weakness || _effect.type == StatusType.Warded)
                {
                    sameStatusFound = false;
                }
                if (sameStatusFound)
                {
                    if (_effect.timeRemaining > sameEffect.timeRemaining || _effect.amount > sameEffect.amount)
                    {
                        sameEffect = _effect;
                    }
                }
            }
            if (!sameStatusFound)
            {
                if (_effect.tickAtStart)
                {
                    ApplyStatus(_effect);
                    _effect.nextTick = _effect.timeRemaining - _effect.tickRate;
                }
                effects.Add(_effect);
            }
        }
        else
        {
            if (_effect.tickAtStart)
            {
                ApplyStatus(_effect);
                _effect.nextTick = _effect.timeRemaining - _effect.tickRate;
            }
            effects.Add(_effect);
        }
    }

    public float DamageModifier()
    {
        float modifier = 1f + weakness + warded;
        return modifier;
    }

    public void ClearAll()
    {
        effects.Clear();
    }
}
