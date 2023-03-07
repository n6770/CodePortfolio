using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Halloween Hordes/Player Character", order = 1)]
public class PlayerCharacter : ScriptableObject
{
    public string characterName;
    public PlayerClass characterClass;

    public float speed;
    public float health;

    public Weapon[] startingWeapons;

    public Sprite[] walkSprites;
    public Sprite idleSprite;
    public bool spriteFacingRight;
}
