using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerGroupController : MonoBehaviour
{
    public static PlayerGroupController instance;

    public List<PlayerCharacter> activeMembers = new();
    public List<PlayerCharacter> allMembers = new();
    public Transform[] groupTargets = new Transform[4];
    public PlayerCharacter firstMemberInLine;
    public PlayerCharacter lastMemberInLine;
    public int membersCount = 0;

    [SerializeField]
    private float masterMoveSpeed;
    private float moveMultiplier = 1f;
    public float characterMoveSpeed;
    public Transform master;
    private Rigidbody2D masterRb;
    public Transform enemyTargetRight;
    public Transform enemyTargetLeft;

    public float membersOffset;

    private FalseGodsInputActions actions;
    public GroupUIScript groupUI;

    public Vector2 inputVector;
    public Vector2 lastDirection;
    private float dodgeCooldown;

    [SerializeField] private Transform blockerRight, blockerLeft;
    private bool canMoveLeft = true;
    private bool canMoveRight = true;

    private bool dodgePressed;
    private bool parryPressed;
    private bool[] attackBools = new bool[4];
    private bool[] abilityBools = new bool[4];

    public float groupStamina = 100f;
    private float maxStamina;
    [SerializeField]
    private float staminaFillRate = 10f;

    private void Awake()
    {
        instance = this;

        actions = new();
        actions.Enable();
        masterRb = master.GetComponent<Rigidbody2D>();
        groupUI = GetComponent<GroupUIScript>();
    }

    private void Start()
    {
        GetFirstAndLast();
        maxStamina = groupStamina;

        //Get group members count
        for(int i = 0; i < activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                membersCount++;
            }
        }

        //Set groupTargets local positions
        for(int i = 0; i < groupTargets.Length; i++)
        {
            Vector3 newLocalPos = new Vector3(-membersOffset * i, 0, 0);
            groupTargets[i].localPosition = newLocalPos;
        }

        SetBlockers();
    }

    public void SetBlockers()
    {
        blockerLeft.position = enemyTargetLeft.position + new Vector3(1f, 0f, 0f);
        blockerRight.position = enemyTargetRight.position + new Vector3(-1f, 0f, 0f);
    }

    public void SetBlockedDirection(bool left, bool state)
    {
        if (left)
        {
            canMoveLeft = state;
        }
        else
        {
            canMoveRight = state;
        }
    }

    private void Update()
    {
        if (GameManager.instance.playerHasControl) GetInput();
        HandleInput();

        if (dodgeCooldown > 0f) dodgeCooldown -= Time.deltaTime;
        if (moveMultiplier < 1f) moveMultiplier += Time.deltaTime * 0.25f;
    }
    private void GetInput()
    {
        inputVector = actions.Player.Move.ReadValue<Vector2>();
        inputVector.y = 0f;

        if (Mathf.Abs(inputVector.x) > 0.1f) lastDirection.x = inputVector.x;

        if (actions.Player.Parry.WasPressedThisFrame()) parryPressed = true;

        if (actions.Player.Dodge.WasPressedThisFrame() && dodgeCooldown <= 0f)
        {
            if (lastDirection.x > 0f && ScanForBlocker(firstMemberInLine.transform.position, Vector2.right)) dodgePressed = false;
            else if (lastDirection.x < 0f && ScanForBlocker(lastMemberInLine.transform.position, Vector2.left)) dodgePressed = false;
            else dodgePressed = true;

            if (dodgePressed)
            {
                MasterDodge();
                dodgeCooldown = 2f;
            }

        }
        attackBools = new bool[] { actions.Player.Attack1.WasPressedThisFrame(), actions.Player.Attack2.WasPressedThisFrame(), 
                      actions.Player.Attack3.WasPressedThisFrame(), actions.Player.Attack4.WasPressedThisFrame() };
        abilityBools = new bool[] { actions.Player.Ability1.WasPressedThisFrame(), actions.Player.Ability2.WasPressedThisFrame(), 
                       actions.Player.Ability3.WasPressedThisFrame(), actions.Player.Ability4.WasPressedThisFrame() };
    }

    private void HandleInput()
    {
        Vector2 velocity = new Vector2(inputVector.x, 0f);

        if (!canMoveLeft && velocity.x < 0f) velocity.x = 0f;
        if (!canMoveRight && velocity.x > 0f) velocity.x = 0f;

        if (GameManager.instance.playerHasControl && !SomeoneIsBusy() && activeMembers.Count > 0) masterRb.velocity = velocity * masterMoveSpeed * moveMultiplier;
        else masterRb.velocity = Vector2.zero;


        for (int i = 0; i < allMembers.Count; i++)
        {
            if (allMembers[i] != null)
            {
                allMembers[i].GetCommand(dodgePressed, parryPressed, attackBools[i], abilityBools[i]);
            }
        }

        parryPressed = false;
        dodgePressed = false;
    }


    private void MasterDodge()
    {
        master.transform.Translate((Vector3)lastDirection.normalized * 6);
        SoundManager.instance.PlayRandomSound(SoundManager.instance.dodgeClips, firstMemberInLine.audioSource, 0.5f);
    }


    public void MemberStunned(PlayerCharacter character)
    {
        CheckGroupDeathStatus();
        RemoveFromGroup(character);
        GetFirstAndLast();
        EnemyController.instance.SetMoveTargets();
    }

    public void GetFirstAndLast()
    {
        int playersAliveCount = 0;
        for (int i = 0; i < activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                if (!activeMembers[i].health.isDead)
                {
                    playersAliveCount++;
                }
            }
        }

        for (int i = 0; i < playersAliveCount; i++)
        {
            if (activeMembers[i] != null)
            {
                if (i == 0) firstMemberInLine = activeMembers[i];
                if (i == playersAliveCount - 1) lastMemberInLine = activeMembers[i];
            }
        }
    }

    private void CheckGroupDeathStatus()
    {
        bool allDead = true;

        for(int i = 0; i < activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                if (!activeMembers[i].health.isDead)
                {
                    allDead = false;
                }
            }
        }

        if (allDead) //TÄSSÄ GROUPIN KUOLEMA
        {
            print("Group was wiped");
            StartCoroutine(RestartGame());
        }
    }

    public void NewTargets()
    {
        foreach(PlayerCharacter character in activeMembers)
        {
            if (character != null)
            {
                character.moveTarget = GiveMoveTarget(character);
            }
        }
    }

    public Transform GiveMoveTarget(PlayerCharacter member)
    {
        int index = activeMembers.IndexOf(member);
        return groupTargets[index];
    }

    private void RefreshMoveTargets()
    {
        int moveTargetIndex = 0;
        for(int i = 0; i < activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                if (!activeMembers[i].health.isDead)
                {
                    activeMembers[i].moveTarget = groupTargets[moveTargetIndex];
                    moveTargetIndex++;
                }
            }
        }
    }

    public void AddToGroup(PlayerCharacter character)
    {
        activeMembers.Add(character);
        RefreshGroupList();
    }

    public void RemoveFromGroup(PlayerCharacter character)
    {
        int index = activeMembers.IndexOf(character);
        activeMembers.RemoveAt(index);
        RefreshGroupList();
    }

    public void SwapGroupPositions(PlayerCharacter character1, PlayerCharacter character2)
    {
        PlayerCharacter tmp = character2;
        
        int index1 = activeMembers.IndexOf(character1);
        int index2 = activeMembers.IndexOf(character2);

        activeMembers[index2] = character1;
        activeMembers[index1] = character2;
    }

    private void RefreshGroupList()
    {
        List<PlayerCharacter> newList = new List<PlayerCharacter>();

        for(int i = 0; i <activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                newList.Add(activeMembers[i]);
            }
        }

        activeMembers = newList;
        RefreshMoveTargets();
    }

    public void GroupInstantIdle()
    {
        for (int i = 0; i < activeMembers.Count; i++)
        {
            if(activeMembers[i] != null) activeMembers[i].SetIdle();
        }
    }

    public void HealGroup(int amount)
    {
        foreach(PlayerCharacter member in activeMembers)
        {
            if (member != null)
            {
                member.TakeHealing(amount);
            }
        }
    }

    private bool SomeoneIsBusy()
    {
        bool busy = false;

        for (int i = 0; i < activeMembers.Count; i++)
        {
            if (activeMembers[i] != null)
            {
                if (activeMembers[i].isBusy)
                {
                    busy = true;
                }
            }
        }

        return busy;
    }

    private bool ScanForBlocker(Vector2 startPos, Vector2 direction)
    {
        bool returnBool = Physics2D.Raycast(startPos + Vector2.up, direction, 6f, 1 << 15);
        Debug.DrawLine(startPos + Vector2.up, startPos + Vector2.up + direction * 6f, Color.cyan, 2f);
        return returnBool;
    }

    public void PlayerWasHit()
    {
        moveMultiplier = 0.5f;
    }

    public float GetVelocityX()
    {
        return masterRb.velocity.x;
    }

    public bool Slowed()
    {
        return moveMultiplier < 1f;
    }

}
