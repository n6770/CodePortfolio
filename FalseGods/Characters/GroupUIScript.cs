using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GroupUIScript : MonoBehaviour
{
    private PlayerGroupController gameController;
    
    [SerializeField]
    private Transform[] groupMembersUI;
    [SerializeField]
    private AbilityUIHandler[] abilityUIHandlers;
    private Image[] portraits;
    private Image[] healthBars;
    private Image[] healthBarBacks;
    [SerializeField]
    private Image staminaBar;



    private void Start()
    {
        gameController = GetComponent<PlayerGroupController>();
        Initialize();
    }

    private void Initialize()
    {
        portraits = new Image[groupMembersUI.Length];
        healthBars = new Image[groupMembersUI.Length];
        healthBarBacks = new Image[groupMembersUI.Length];

        for(int i = 0; i < groupMembersUI.Length; i++)
        {
            portraits[i] = groupMembersUI[i].Find("Portrait").GetComponent<Image>();
            healthBars[i] = groupMembersUI[i].Find("Health").Find("HealthBackFill").Find("HealthFill").GetComponent<Image>();
            healthBarBacks[i] = groupMembersUI[i].Find("Health").Find("HealthBackFill").GetComponent<Image>();
        }
        SetPortraits(gameController.activeMembers.ToArray());
    }

    public void SetPortraits(PlayerCharacter[] _members)
    {
        for(int i = 0; i < groupMembersUI.Length; i++)
        {
            if (i < _members.Length)
            {
                portraits[i].sprite = _members[i].character.portraitImage;
                groupMembersUI[i].gameObject.SetActive(true);

                abilityUIHandlers[i] = groupMembersUI[i].gameObject.GetComponent<AbilityUIHandler>();
                abilityUIHandlers[i].SetVariables(gameController.activeMembers[i].actionManager.currentAbility.abilityIcon, gameController.allMembers[i].actionManager.currentAbility.cooldown);
            }
            else
            {
                groupMembersUI[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetHealthBar(float fill, int member, float time = 0.2f)
    {
        DOTween.To(() => healthBars[member].fillAmount, x => healthBars[member].fillAmount = x, fill, time);
        DOTween.To(() => healthBarBacks[member].fillAmount, x => healthBarBacks[member].fillAmount = x, fill, time * 4).SetDelay(1f);
    }

    public void SetStaminaBar(float fill)
    {
        staminaBar.fillAmount = fill;
    }

    public void StartCooldown(int memberIndex)
    {
        abilityUIHandlers[memberIndex].StartCooldown();
    }
}
