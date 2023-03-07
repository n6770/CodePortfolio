using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponMover : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 currentPos;
    private Vector2 startDirection;
    private Tween moveTween;
    private Tween rotateTween;
    private bool rotating;
    private float rotateRange;
    private float rotateSpeed;
    private bool rotate;
    [SerializeField] private Transform gfx;

    public void StartMoving(WeaponMovement movement, Vector2 direction, float range, float speed, bool rotates = true)
    {
        startPos = transform.position;
        startDirection = direction;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        switch (movement)
        {
            case WeaponMovement.Projectile:
                StartPierceMove(speed, range);
                break;
            case WeaponMovement.Swing:
                StartSwingMove(speed, range);
                break;
            case WeaponMovement.Rotator:
                StartRotatorMove(speed, range);
                break;
        }
        rotate = rotates;
    }

    public void StartPierceMove(float speed, float range)
    {
        moveTween = transform.DOMove(transform.position + (transform.up * range), speed).SetEase(Ease.Linear);
    }

    public void StartSwingMove(float speed, float range)
    {
        //not in use
    }

    public void StartRotatorMove(float speed, float range)
    {
        rotateRange = range;
        rotateSpeed = speed;
        rotating = true;
        rotateTween = transform.DORotate(new Vector3(transform.rotation.x, transform.rotation.y, -360f), speed, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
    }

    private void Update()
    {
        if (rotating)
        {
            if (gfx.localPosition.y != rotateRange)
            {
                float gfxYtemp = Mathf.Lerp(gfx.localPosition.y, rotateRange, 1f * Time.deltaTime);
                gfx.localPosition = new Vector3(0f, gfxYtemp, 0f);
            }
            transform.position = GameManager.instance.player.transform.position;
        }
    }

    private void OnDisable()
    {
        moveTween.Kill();
        rotateTween.Kill();
        rotating = false;
        gfx.localPosition = Vector3.zero;
    }
}
