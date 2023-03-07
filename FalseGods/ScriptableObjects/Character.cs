using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    [HorizontalGroup("Info", Width = 50), HideLabel, PreviewField(50)]
    public Sprite portraitImage;

    [VerticalGroup("Info/Type"), Indent(2)]
    public string characterName;

    [VerticalGroup("Info/Type"), Indent(2)]
    [EnumToggleButtons]
    public CharacterType characterType;

    [VerticalGroup("Info/Type"), Indent(2)]
    [ShowIf("characterType", CharacterType.Enemy)]
    [EnumToggleButtons]
    public AIType aiType;

    public GameObject prefab;

    public Weapon startingWeapon;

    [InfoBox("If an player character starts with multible abilities, first ability is equipped at start")]
    public List<Ability> abilities;

    public int health;

    [Tooltip("How much the character can withstand damage until staggered")]
    public float staggerPoint;

    [Tooltip("Stagger regeneration rate")]
    public float staggerRegen;

    //ÄÄNET
    public AudioClip[] footsteps;
    public AudioClip[] aggroSounds;
    public AudioClip[] damageSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] deathSounds;

}
