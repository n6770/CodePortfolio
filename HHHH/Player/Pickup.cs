using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Pickup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PickupScriptableObject scriptableObject;
    private Tween scaleTween;
    private Tween startMoveTween;
    [SerializeField] private float lifeTime;
    private float timeElapsed;
    [SerializeField] private LayerMask collectLayer;
    private bool active = false;

    public void ActivateObject(PickupScriptableObject pickupScriptableObject)
    {
        active = true;
        transform.localScale = Vector3.zero;
        timeElapsed = 0f;
        scriptableObject = pickupScriptableObject;
        spriteRenderer.sprite = pickupScriptableObject.sprite;
        transform.localScale = Vector3.zero;
        Appear();
        StartMover();
    }

    private void StartMover()
    {
        startMoveTween = transform.DOMove(transform.position + ((Vector3)Random.insideUnitCircle), 0.5f);
    }

    private void Update()
    {
        if (timeElapsed < lifeTime) timeElapsed += Time.deltaTime;
        else if (timeElapsed > lifeTime) TimeUp();
    }

    private void TimeUp()
    {
        if (active)
        {
            scaleTween.Kill();
            scaleTween = transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => DeactivateObject());
            active = false;
        }
    }

    private void Appear()
    {
        scaleTween = transform.DOScale(Vector3.one, 0.2f).OnComplete(() => ScaleUp());
    }

    public void ScaleUp()
    {
        scaleTween = transform.DOScale(Vector3.one * 1.2f, 1f).OnComplete(() => ScaleDown());
    }

    public void ScaleDown()
    {
        scaleTween = transform.DOScale(Vector3.one, 1f).OnComplete(() => ScaleUp());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            SoundManager.instance.PlaySound(scriptableObject.pickupSound, 0.5f);
            if (scriptableObject.healthPickup)
            {
                GameManager.instance.player.PickupHealth(scriptableObject.amount);
                SoundManager.instance.PlaySound(SoundManager.instance.healSound, 0.5f);
                SpawnParticles(true);
            }
            else if (scriptableObject.xpPickup)
            {
                GameManager.instance.player.PickupXP(scriptableObject.amount);
                SoundManager.instance.PlaySound(SoundManager.instance.xpSounds[Random.Range(0, SoundManager.instance.xpSounds.Length)], 0.5f);
                SpawnParticles(false);
            }
        DeactivateObject();
        }
    }

    private void SpawnParticles(bool health)
    {
        ObjectPool poolToUse = health ? GameManager.instance.heartPool : GameManager.instance.xpPool;

        GameObject particleObj = poolToUse.GetPooledObject();
        if (particleObj != null)
        {
            particleObj.SetActive(true);
            particleObj.transform.position = transform.position;
            particleObj.GetComponent<ParticleSystem>().Play();
        }
    }

    public void DeactivateObject()
    {
        scaleTween.Kill();
        startMoveTween.Kill();
        gameObject.SetActive(false);
    }
}
