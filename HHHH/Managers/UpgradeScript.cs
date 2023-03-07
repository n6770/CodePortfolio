using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UpgradeScript : MonoBehaviour
{
    [SerializeField] private float statUpgradeAmount;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private RectTransform leftPanel, rightPanel;

    public Sprite[] statUpgradeSprites;

    public bool canGetNewWeapon = true;
    public bool canGetWeaponUpgrade = true;

    public PlayerUpgrade leftUpgrade, rightUpgrade;
    private PlayerScript player;

    public TextMeshProUGUI leftText, rightText;
    public Image leftImage, rightImage;

    [SerializeField] private bool choosingUpgrade;
    [SerializeField] private float waitTime;

    private void Start()
    {
        player = GameManager.instance.player;
    }

    private void Update()
    {
        if (choosingUpgrade && waitTime < 0.5f) waitTime += Time.unscaledDeltaTime;

        if (choosingUpgrade && waitTime > 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                RewardUpgrade(false);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                RewardUpgrade(true);
            }
        }
    }

    public void NewUpgrades()
    {
        leftUpgrade = GetRandomUpgrade();
        rightUpgrade = GetRandomUpgrade();
        
    }


    public void SetUpgradeUI()
    {
        leftText.text = leftUpgrade.upgradeText;
        leftImage.sprite = leftUpgrade.upgradeSprite;

        rightText.text = rightUpgrade.upgradeText;
        rightImage.sprite = rightUpgrade.upgradeSprite;
    }

    public void ChooseUpgrades()
    {
        choosingUpgrade = true;
        Time.timeScale = 0f;
        waitTime = 0f;
    }

    private void CheckMaxUpgrades()
    {
        PlayerScript player = GameManager.instance.player;
        bool maxUpgradesReached = true;
        
        for (int i = 0; i < player.weapons.Count; i++)
        {
            if (player.weapons[i].upgradeLevel != player.weapons[i].baseWeapon.maxUpgradeLevel) maxUpgradesReached = false;
        }

        canGetWeaponUpgrade = !maxUpgradesReached;
    }

    public void RewardUpgrade(bool right)
    {
        if (!choosingUpgrade) return;

        player = GameManager.instance.player;
        choosingUpgrade = false;

        PlayerUpgrade rewardUpgrade = right ? rightUpgrade : leftUpgrade;

        if (rewardUpgrade.upgradeType == UpgradeType.NewWeapon) GiveNewWeapon(rewardUpgrade);
        else if (rewardUpgrade.upgradeType == UpgradeType.WeaponUpgrade) GiveWeaponUpgrade(rewardUpgrade);
        else if (rewardUpgrade.upgradeType == UpgradeType.StatUpgrade) GiveStatUpgrade(rewardUpgrade);

        player.PickupHealth(player.maxHealth);
        player.RefreshCooldownList();

        SoundManager.instance.PlaySound(SoundManager.instance.upgradeSound, 1f);

        if (!right) RewardSelectedJuice(leftPanel);
        else RewardSelectedJuice(rightPanel);
    }

    private void RewardSelectedJuice(RectTransform selection)
    {
        selection.DOPunchScale(Vector3.one * 1.2f, 0.25f, 1, 0.2f).SetUpdate(true).OnComplete(() => CloseLevelPanel());
    }

    private void CloseLevelPanel()
    {
        GameManager.instance.CloseUIElement(GameManager.instance.levelUpPanel);
        rightPanel.localScale = Vector3.one;
        leftPanel.localScale = Vector3.one;
        Time.timeScale = 1f;
        player.UpdateStats();
        UpdateStatsText();
    }

    private void GiveNewWeapon(PlayerUpgrade rewardUpgrade)
    {
        player.weapons.Add(GameManager.instance.database.BuildWeaponInst(rewardUpgrade.weaponToGetOrUpgrade));
    }

    private void GiveWeaponUpgrade(PlayerUpgrade rewardUpgrade)
    {
        GameManager.instance.database.RemoveWeaponFromPlayer(rewardUpgrade.weaponToGetOrUpgrade.baseWeapon);
        player.weapons.Add(GameManager.instance.database.BuildWeaponInst(rewardUpgrade.weaponToGetOrUpgrade));
    }
    
    private void GiveStatUpgrade(PlayerUpgrade rewardUpgrade)
    {
        UpgradeStat statToUpgrade = rewardUpgrade.upgradeStat;
        switch (statToUpgrade)
        {
            case UpgradeStat.Health:
                player.healthMultiplier += statUpgradeAmount;
                break;
            case UpgradeStat.Speed:
                player.speedMultiplier += statUpgradeAmount;
                break;
            case UpgradeStat.Damage:
                player.damageMultiplier += statUpgradeAmount;
                break;
        }
        player.UpdateStats();
        UpdateStatsText();
    }

    public void UpdateStatsText()
    {
        statsText.text =
        "Health: " + player.health.ToString("0.0") +
        "\nMax health " + player.maxHealth.ToString("0.0") +
        "\nSpeed " + player.movementSpeed.ToString("0.0") + "\n" +
        "\nMultipliers" +
        "\nHealth X " + player.healthMultiplier.ToString("0.0") +
        "\nSpeed X " + player.speedMultiplier.ToString("0.0") +
        "\nDmg X " + player.damageMultiplier.ToString("0.0");

    }

    public PlayerUpgrade GetRandomUpgrade()
    {
        CheckMaxUpgrades();

        PlayerUpgrade upgrade;
        UpgradeStat upgradeStat = RandomStatUpgrade();
        Weapon weaponToGetOrUpgrade = GameManager.instance.database.allWeapons[0];
        string upgradeText = null;
        Sprite upgradeSprite = null;

        UpgradeType upgradeType = (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);
        while ((upgradeType == UpgradeType.NewWeapon && !canGetNewWeapon) || (upgradeType == UpgradeType.WeaponUpgrade && !canGetWeaponUpgrade))
        {
            upgradeType = (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);
        }
        
        if (upgradeType == UpgradeType.NewWeapon)
        {
            weaponToGetOrUpgrade = GameManager.instance.database.baseWeapons[Random.Range(0, GameManager.instance.database.baseWeapons.Length)];
            while (GameManager.instance.database.PlayerOwnsWeapon(weaponToGetOrUpgrade))
            {
                weaponToGetOrUpgrade = GameManager.instance.database.baseWeapons[Random.Range(0, GameManager.instance.database.baseWeapons.Length)];
            }
            upgradeText = "New weapon!\n" + weaponToGetOrUpgrade.weaponName;
            upgradeSprite = weaponToGetOrUpgrade.sprites[0];
        }

        if (upgradeType == UpgradeType.WeaponUpgrade)
        {
            List<Weapon> possibleWeaponUpgrades = new List<Weapon>();
            for (int i = 0; i < GameManager.instance.player.weapons.Count; i++)
            {
                for (int j = 0; j < GameManager.instance.database.allWeapons.Length; j++)
                {
                    Weapon weapon = GameManager.instance.database.allWeapons[j];
                    if (weapon.baseWeapon == GameManager.instance.player.weapons[i].baseWeapon)
                    {
                        if (weapon.upgradeLevel == GameManager.instance.player.weapons[i].upgradeLevel + 1) possibleWeaponUpgrades.Add(weapon);
                    }
                }
            }
            weaponToGetOrUpgrade = possibleWeaponUpgrades[Random.Range(0, possibleWeaponUpgrades.Count)];
            upgradeText = "Weapon upgrade!\n" + weaponToGetOrUpgrade.weaponName;
            upgradeSprite = weaponToGetOrUpgrade.sprites[0];
        }

        if (upgradeType == UpgradeType.StatUpgrade)
        {
            upgradeText = upgradeStat.ToString() + " up!";
            Sprite sprite = null;
            switch (upgradeStat)
            {
                case UpgradeStat.Health:
                    sprite = statUpgradeSprites[0];
                    break;
                case UpgradeStat.Speed:
                    sprite = statUpgradeSprites[1];
                    break;
                case UpgradeStat.Damage:
                    sprite = statUpgradeSprites[2];
                    break;
            }
            upgradeSprite = sprite;
        }


        upgrade = new PlayerUpgrade(upgradeType, upgradeText, upgradeStat, weaponToGetOrUpgrade, upgradeSprite);

        return upgrade;
        
    }

    private UpgradeStat RandomStatUpgrade()
    {
        UpgradeStat upgradeStat = (UpgradeStat)Random.Range(0, System.Enum.GetValues(typeof(UpgradeStat)).Length);
        return upgradeStat;
    }
}

[System.Serializable]
public class PlayerUpgrade
{
    public UpgradeType upgradeType;
    public string upgradeText;
    public UpgradeStat upgradeStat;
    public Weapon weaponToGetOrUpgrade;
    public Sprite upgradeSprite;

    public PlayerUpgrade(UpgradeType upgradeType, string upgradeText, UpgradeStat upgradeStat, Weapon weaponToGetOrUpgrade, Sprite upgradeSprite)
    {
        this.upgradeType = upgradeType;
        this.upgradeText = upgradeText;
        this.upgradeStat = upgradeStat;
        this.weaponToGetOrUpgrade = weaponToGetOrUpgrade;
        this.upgradeSprite = upgradeSprite;
    }
}

public enum UpgradeType { NewWeapon, WeaponUpgrade, StatUpgrade };
public enum UpgradeStat { Health, Speed, Damage };


