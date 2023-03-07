using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour, IWeapon
{
    private string weaponName;
    private float damage;
    private float speed;
    private float range;
    private float lifeTime;
    private float lifeTimeElapsed = 0f;
    private bool piercing;

    private WeaponMovement weaponMovement;
    private AreaOfEffect areaOfEffect;
    [SerializeField] private WeaponMover weaponMover;


    private void SetUpWeapon(WeaponInst weapon)
    {
        weaponName = weapon.weaponName;
        damage = weapon.damage;
        lifeTime = weapon.lifeTime;
        piercing = weapon.piercing;
        speed = weapon.speed;
        range = weapon.range;

        weaponMovement = weapon.weaponMovement;
        areaOfEffect = weapon.areaOfEffect;
    }

    public void ActivateWeapon(Vector2 direction, WeaponInst weapon, float angle)
    {
        lifeTimeElapsed = 0f;
        SetUpWeapon(weapon);
        weaponMover.StartMoving(weaponMovement, direction, range, speed);
    }

    private void Update()
    {
        lifeTimeElapsed += Time.deltaTime;
        if (lifeTimeElapsed > lifeTime)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyScript>().TakeDamage(damage);
            if (!piercing) Deactivate();
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
