using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public EnemySpawner spawner;
    public Database database;
    public UpgradeScript upgradeScript;
    public LevelManager levelManager;

    public PlayerScript player;
    public Vector2 areaBounds;

    public ObjectPool pickupPool;
    public ObjectPool bloodPool, xpPool, heartPool;

    public GameObject levelUpPanel;
    public GameObject deathPanel;

    private void Awake()
    {
        instance = this;
    }

    public void PlayerLevelUp(int level)
    {
        OpenUIElement(levelUpPanel);
        print("player level: " + level);
        spawner.RefreshSpawnList(level);
        upgradeScript.NewUpgrades();
        upgradeScript.SetUpgradeUI();
        upgradeScript.ChooseUpgrades();
        spawner.SetSpawnCooldown(level);
    }

    public void OpenUIElement(GameObject gameObjectUI)
    {
        RectTransform rectTransform = gameObjectUI.GetComponent<RectTransform>();
        gameObjectUI.gameObject.SetActive(true);
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, 0.25f).SetUpdate(true);
    }
    public void CloseUIElement(GameObject gameObjectUI)
    {
        RectTransform rectTransform = gameObjectUI.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.DOScale(Vector3.zero, 0.25f).SetUpdate(true).OnComplete(() => gameObjectUI.gameObject.SetActive(false));
    }
}


