using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour, IDataPersistence
{
    private GameCharacter owner;

    [SerializeField]
    [GUIColor(1f, 0.5f, 0.5f)]
    private int health;

    [SerializeField]
    [GUIColor(1f, 0.3f, 0.3f)]
    private int maxHealth;

    [SerializeField]
    [GUIColor(0f, 1f, 0f)]
    private float staggerPoints;

    [SerializeField]
    [GUIColor(0f, 0.7f, 0f)]
    private float maxStaggerPoints;
    private float staggerRegenRate;
    private bool staggerRegenerating = true;
    private WaitForSeconds staggerPauseWFS;
    private Coroutine staggerPauseCoroutine;


    public bool isDead;
    public bool isStaggered;

    [SerializeField]
    private Vector3 textOffset;
    
    private void Awake() 
    {
        owner = GetComponent<GameCharacter>();
    }

    private void Update()
    {
        RegenStagger();
    }


    private void RegenStagger()
    {
        if (!staggerRegenerating) return;
        
        if (isStaggered)
        {
            staggerPoints = Mathf.Clamp(staggerPoints + staggerRegenRate * 8 * Time.deltaTime, 0f, maxStaggerPoints);
        }
        else if (staggerPoints < maxStaggerPoints) staggerPoints = Mathf.Clamp(staggerPoints + staggerRegenRate * Time.deltaTime, 0f, maxStaggerPoints);
    }

    public void SetHealth(int _health, bool updateMaxHealth = true)
    {
        health = _health;
        if (updateMaxHealth) maxHealth = health;
    }

    public void SetStaggerPoints(float _staggerPoints, float _staggerRegenRate, bool updateMaxStagger = true)
    {
        staggerPoints = _staggerPoints;
        staggerRegenRate = _staggerRegenRate;
        if (updateMaxStagger) maxStaggerPoints = staggerPoints;
        staggerPauseWFS = new WaitForSeconds(staggerRegenRate / 8);
    }

    public void SetStaggered(bool _state)
    {
        isStaggered = _state;
    }

    public void TakeDamage(int damageAmount, string text = null)
    {
        //health
        int appliedDamage = (int)(damageAmount * owner.status.DamageModifier());

        health = Mathf.Clamp(health - appliedDamage, 0, maxHealth);
        isDead = health <= 0 ? true : false;

        //blood splatter
        if (damageAmount > 3f)
        {
            GameObject blood = GameManager.instance.bloodSplatterPool.GetPooledObject();
            blood.SetActive(true);
            blood.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);
            if (!owner.useFlip)
            {
                blood.transform.localScale = transform.localScale;
            }
            else
            {
                blood.transform.localScale = new Vector3(owner.currentDirection.x, 1f, 0f);
            }
            blood.GetComponent<ParticleSystem>().Play();

            //master slowdown
            if (owner.character.characterType == CharacterType.Player)
            {
                PlayerGroupController.instance.PlayerWasHit();
            }
        }

        //sfx
        if (isDead)
        {
            SoundManager.instance.PlayRandomSound(owner.character.deathSounds, owner.audioSource, 0.25f);
        }
        else if (appliedDamage > 0 && Random.value > 0.5f) SoundManager.instance.PlayRandomSound(owner.character.damageSounds, owner.audioSource, 0.25f);

        //stagger
        if (!isStaggered)
        {
            staggerPoints = Mathf.Clamp(staggerPoints - appliedDamage, 0, maxStaggerPoints);
            if (staggerPauseCoroutine != null) StopCoroutine(staggerPauseCoroutine);
            staggerPauseCoroutine = StartCoroutine(StaggerRegenPause());
            if (staggerPoints <= 0f)
            {
                float staggerDuration = maxStaggerPoints / (staggerRegenRate * 8);
                owner.CharacterStaggered(staggerDuration);
            }
        }

        //Floating text
        if (appliedDamage > 0) SetCombatText(appliedDamage);
        else if (text != null) SetCombatText(0, text, true);
    }

    public void TakeHealing(int healingAmount, string text = null)
    {
        int startHealth = health;
        health = Mathf.Clamp(health + healingAmount, 0, maxHealth);
        int appliedHealing = healingAmount;
        if (health >= maxHealth)
        {
            appliedHealing = maxHealth - startHealth;
        }

        //Floating text
        if (appliedHealing > 0) SetCombatText(appliedHealing, null, false, true);
        else if (text != null) SetCombatText(0, text, true);
    }

    public void SetCombatText(int damage, string text = null, bool useText = false, bool heal = false)
    {
        GameObject combatText = GameManager.instance.combatTextPool.GetPooledObject();
        combatText.SetActive(true);
        string stringToUse;
        if (useText && text != null) stringToUse = text;
        else stringToUse = damage.ToString();
        combatText.GetComponent<CombatText>().ActivateText(transform.position + textOffset, stringToUse, useText, heal);
    }

    public float GetHealthPercent()
    {
        return (float)health / maxHealth;
    }

    private IEnumerator StaggerRegenPause()
    {
        staggerRegenerating = false;
        yield return staggerPauseWFS;
        staggerRegenerating = true;
    }

}
