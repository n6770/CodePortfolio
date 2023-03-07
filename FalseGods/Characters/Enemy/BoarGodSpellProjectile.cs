using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarGodSpellProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    public int damage;
    [SerializeField] private float velocityMin;
    [SerializeField] private float velocityMax;
    [SerializeField] private float zRotateOffset;
    [SerializeField] private float impactRadius;
    [SerializeField] private GameObject impactParticles;
    [SerializeField] private GameObject anticipation;

    [SerializeField] private LayerMask impactLayer;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] AudioClip[] impactSounds;
    [SerializeField] AudioClip[] spawnSounds;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator Start()
    {
        yield return null;
        transform.rotation = Quaternion.Euler(0f, 0f, 180f + Random.Range(-zRotateOffset, zRotateOffset));
        Ray2D ray = new Ray2D(transform.position, transform.up);
        RaycastHit2D hitPoint = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, impactLayer);

        Debug.DrawRay(transform.position, transform.up * 20f, Color.cyan, 2f);

        GameObject impactAnt = Instantiate(anticipation);
        impactAnt.transform.position = hitPoint.point;
        rb.velocity = transform.up * Random.Range(velocityMin, velocityMax);

        SoundManager.instance.PlayRandomSound(spawnSounds, audioSource, 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ImpactTriggered();
    }

    private void ImpactTriggered()
    {
        rb.velocity = Vector2.zero;

        SoundManager.instance.PlayRandomSound(impactSounds, audioSource, 0.15f);

        Collider2D[] playersHit = Physics2D.OverlapCircleAll(transform.position, impactRadius, playerLayer);

        if (playersHit.Length > 0)
        {
            foreach (var player in playersHit)
            {
                player.GetComponent<IDamageable>().TakeDamage(damage);
            }
        }

        GameObject impactPart = Instantiate(impactParticles);
        impactPart.transform.position = transform.position;

        Destroy(gameObject, 2f);
    }
}
