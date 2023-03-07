using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoulLeech", menuName = "Abilities/SoulLeech")]
public class AbilitySoulLeech : Ability
{
    [SerializeField] private GameObject vfxObject;
    [SerializeField] private GameObject windupVfxObject;
    [SerializeField] private float range;
    [SerializeField] private int initialDamage;
    [SerializeField] private int leechPerHit;

    [SerializeField] private StatusEffect effect;

    [SerializeField] private AudioClip sfx;
    [SerializeField] private AudioClip windupSfx;

    public override void ActivateAbility(GameCharacter _owner)
    {
        Quaternion _rotation;
        if (_owner.currentDirection.x > 0f) _rotation = Quaternion.Euler(0f, 90f, 0f);
        else _rotation = Quaternion.Euler(180f, 90f, 0f);

        Instantiate(vfxObject, _owner.attackPosition.position, _rotation);
        SoundManager.instance.PlaySound(sfx, _owner.audioSource, 0.25f);

        RaycastHit2D[] hits = Physics2D.RaycastAll(_owner.transform.position, _owner.currentDirection, range, _owner.enemyLayer);

        if (hits.Length > 0)
        {
            PlayerGroupController.instance.HealGroup(leechPerHit * hits.Length);

            foreach (var hit in hits)
            {
                hit.collider.GetComponent<GameCharacter>().TakeDamage(initialDamage, null);
                hit.collider.GetComponent<CharacterStatus>().NewStatus(effect.CloneStatus(effect));
            }
        }
    }

    public override void OnStartCasting(GameCharacter owner)
    {
        SoundManager.instance.PlaySound(windupSfx, owner.audioSource, 0.25f);
        GameObject vfx = Instantiate(windupVfxObject, owner.attackPosition.position, Quaternion.identity);
        vfx.transform.SetParent(owner.transform, true);
    }
}
