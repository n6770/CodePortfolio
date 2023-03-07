using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlasher : MonoBehaviour
{
    [SerializeField] private Material flashMaterial, spriteMaterial;
    [SerializeField] private float flashTime, normalTime;
    [SerializeField] private int flashes;
    [SerializeField] private WaitForSeconds flashWFS, normalWFS;
    

    private void Awake()
    {
        flashWFS = new WaitForSeconds(flashTime);
        normalWFS = new WaitForSeconds(normalTime);
    }

    public void StartFlashing(SpriteRenderer spriteRenderer)
    {
        StartCoroutine(Flashing(spriteRenderer));
    }
    public void SetNormal(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.material = spriteMaterial;
    }

    private IEnumerator Flashing(SpriteRenderer spriteRenderer)
    {
        for (int i = flashes; i > 0; i--)
        {
            spriteRenderer.material = flashMaterial;
            yield return flashWFS;
            spriteRenderer.material = spriteMaterial;
            yield return normalWFS;
        }
    }
}
