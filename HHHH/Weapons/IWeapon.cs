using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    public void ActivateWeapon(Vector2 direction, WeaponInst weapon, float addAngle);

    public void Deactivate();
}
