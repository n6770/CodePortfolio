using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Ability : ScriptableObject
{
    [HorizontalGroup("General/Info", Width = 50), PreviewField(50)]
    public Sprite abilityIcon;
    [BoxGroup("General")]
    public string abilityName;
    [BoxGroup("General")]
    [TextArea]
    public string abilityDescription;
    [BoxGroup("General")]
    public float castTime;
    [BoxGroup("General")]
    public float cooldown;
    [BoxGroup("General")]
    public float cost;
    [BoxGroup("General")]
    public bool finishAbilityOnCast;

    public virtual void ActivateAbility(GameCharacter owner) { }

    public virtual void OnStartCasting(GameCharacter owner) { }
}
