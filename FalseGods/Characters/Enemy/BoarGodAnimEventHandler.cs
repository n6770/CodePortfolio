using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarGodAnimEventHandler : MonoBehaviour
{
    private BoarGodScript boarGodScript;

    private void Awake()
    {
        boarGodScript = GetComponentInParent<BoarGodScript>();
    }

    public void AnimEventAttack0()
    {
        boarGodScript.Attack0();
    }
    public void AnimEventAttack1()
    {
        boarGodScript.Attack1();
    }
    public void AnimEventAttack2()
    {
        boarGodScript.Attack2();
    }
    public void AnimEventAttack3()
    {
        boarGodScript.Attack3();
    }

    public void AnimEventSpellStart()
    {
        boarGodScript.SpellStart();
    }

    public void AnimEventSpellProjectilesSpawn()
    {
        boarGodScript.SpellProjectiles();
    }

    public void AnimEventAttackFinished()
    {
        boarGodScript.AttackFinished();
    }

    public void AnimEventDissole(float endValue)
    {
        boarGodScript.DissolveSprite(endValue, 1f);
    }

    public void AnimEventJumpUp()
    {
        boarGodScript.JumpUp();
    }
    public void AnimEventJumpDown()
    {
        boarGodScript.JumpDown();
    }

    public void Footstep()
    {
        boarGodScript.PlayFootstep();
    }
}
