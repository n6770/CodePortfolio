using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class HealthOrbScript : MonoBehaviour
{
    [SerializeField] private int healingAmount;
    [SerializeField] private float moveSpeed;
    private bool active = false;
    private Tween moveTween;
    private Vector2 moveDirection;
    private Light2D orbLight;

    [SerializeField] private AudioClip collectSound;

    void Start()
    {
        orbLight = GetComponent<Light2D>();
        Vector3 startScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(startScale, 1f).OnComplete(() => Activate());
        transform.DOMoveY(transform.position.y + 0.7f, 1f).SetEase(Ease.InOutSine);

        moveDirection = PlayerGroupController.instance.master.position.x > transform.position.x ? Vector2.right : Vector2.left;
        DOTween.To(x => orbLight.intensity = x, 0f, 1.5f, 1f);
    }

    private void Update()
    {
        if (!active) return;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void Activate()
    {
        active = true;
        moveTween = transform.DOMoveY(transform.position.y + 0.25f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!active) return;

        moveTween.Kill();
        PlayerGroupController.instance.HealGroup(healingAmount);
        SoundManager.instance.PlaySoundUI(collectSound, 0.25f);

        Destroy(gameObject);
    }
}
