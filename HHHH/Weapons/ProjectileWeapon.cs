using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    private float lifeTimeElapsed = 0f;
    private float damage;
    private Tween moveTween;
    private Tween rotateTween;
    private WeaponInst weaponInst;

    [SerializeField] private Transform gfx;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D circleCollider;

    private void SetUpWeapon(WeaponInst weapon)
    {
        weaponInst = weapon;
        damage = weapon.damage;
        spriteRenderer.sprite = weaponInst.sprites[0];
        circleCollider.radius = weaponInst.colliderRadius;
    }

    public void ActivateWeapon(Vector2 direction, WeaponInst weapon, float addAngle)
    {
        lifeTimeElapsed = 0f;
        SetUpWeapon(weapon);
        StartMoving(direction, weaponInst.range, weaponInst.speed, addAngle);
    }

    private void Update()
    {
        lifeTimeElapsed += Time.deltaTime;
        if (lifeTimeElapsed > weaponInst.lifeTime)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyScript>().TakeDamage(damage);
            if (!weaponInst.piercing) Deactivate();
        }
    }

    public void Deactivate()
    {
        if (moveTween != null && moveTween.IsPlaying()) moveTween.Kill();
        if (rotateTween != null && rotateTween.IsPlaying()) rotateTween.Kill();
        gfx.localRotation = Quaternion.Euler(Vector3.zero);
        gameObject.SetActive(false);
    }

    //MOVE
    public void StartMoving(Vector2 direction, float range, float speed, float addAngle, bool rotates = true)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += addAngle;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        moveTween = transform.DOMove(transform.position + (transform.up * range), speed).SetEase(Ease.Linear);
        if (weaponInst.rotates)
        {
            rotateTween = gfx.DOLocalRotate(new Vector3(gfx.rotation.x, gfx.rotation.y, 360f), 1f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
        }

    }

}
