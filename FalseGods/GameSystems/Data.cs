using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType { Melee, Projectile, Raycast }

[System.Serializable]
public class StatusEffect
{
    public StatusType type;
    public float timeRemaining;
    public float tickRate;
    public float amount;
    public bool tickAtStart;
    public bool tickAtEnd;
    public float nextTick;

    public StatusEffect(StatusType type, float timeRemaining, float tickRate, float amount, bool tickAtStart, bool tickAtEnd)
    {
        this.type = type;
        this.timeRemaining = timeRemaining;
        this.tickRate = tickRate;
        this.amount = amount;
        this.tickAtStart = tickAtStart;
        this.tickAtEnd = tickAtEnd;
        this.nextTick = timeRemaining - tickRate;

    }
    public StatusEffect CloneStatus(StatusEffect statusToClone)
    {
        StatusEffect newStatusEffect = new StatusEffect(statusToClone.type, statusToClone.timeRemaining, statusToClone.tickRate, statusToClone.amount, statusToClone.tickAtStart, statusToClone.tickAtEnd);
        return newStatusEffect;
    }
}
public enum StatusType { Bleed, Stun, Heal, Silence, Cripple, Weakness, Warded, Shielded, Parried }
public enum AIType { Defensive, Aggressive, Neutral }

public enum AIState { Idle, Moving, Retreating, Attacking, Ability }
public enum CharacterType { Player, Enemy, NPC }