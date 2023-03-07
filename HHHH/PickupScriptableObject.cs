using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pickup", menuName = "Halloween Hordes/Pickup", order = 5)]
public class PickupScriptableObject : ScriptableObject
{
    public string pickupName;
    public bool healthPickup;
    public bool xpPickup;
    public float amount;
    public Sprite sprite;
    public AudioClip pickupSound;
}