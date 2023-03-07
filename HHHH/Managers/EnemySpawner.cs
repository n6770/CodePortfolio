using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private int maxEnemies;
    [SerializeField] private ObjectPool enemyPool;
    [SerializeField] private EnemyCharacter[] allEnemies;
    private List<EnemyCharacter> enemiesToSpawn;
    private float spawnCooldown = 0.8f;
    private float currentspawnTime = 0f;
    private void Start()
    {
        enemiesToSpawn = new List<EnemyCharacter>();
        RefreshSpawnList(1);
    }

    private void Update()
    {
        if (currentspawnTime < spawnCooldown) currentspawnTime += Time.deltaTime;
        else
        {
            SpawnEnemy(enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)]);
            currentspawnTime = 0f;
        }
    }

    public void SetSpawnCooldown(int level)
    {
        spawnCooldown = 0.8f - ((float)level * 0.05f);
    }

    private void SpawnEnemy(EnemyCharacter enemyCharacter)
    {

        GameObject enemy = enemyPool.GetPooledObject();
        if (enemy != null)
        {
            Vector2 spawnPos;
            if (Random.value > 0.5f)
            {
                //Ver spawn
                if (Random.value > 0.5f) spawnPos.x = GameManager.instance.areaBounds.x + GameManager.instance.player.transform.position.x;
                else spawnPos.x = -GameManager.instance.areaBounds.x + GameManager.instance.player.transform.position.x;
                spawnPos.y = Random.Range(-GameManager.instance.areaBounds.y + GameManager.instance.player.transform.position.y, GameManager.instance.areaBounds.y + GameManager.instance.player.transform.position.y);
            }
            else
            {
                //Hor Spawn
                if (Random.value > 0.5f) spawnPos.y = GameManager.instance.areaBounds.y + GameManager.instance.player.transform.position.y;
                else spawnPos.y = -GameManager.instance.areaBounds.y + GameManager.instance.player.transform.position.y;
                spawnPos.x = Random.Range(-GameManager.instance.areaBounds.x + GameManager.instance.player.transform.position.x, GameManager.instance.areaBounds.x + GameManager.instance.player.transform.position.x);
            }

            enemy.GetComponent<EnemyScript>().SetupEnemy(enemyCharacter);
            enemy.transform.position = spawnPos;
            enemy.SetActive(true);
        }
    }

    public void RefreshSpawnList(int level)
    {
        enemiesToSpawn.Clear();
        foreach(var enemy in allEnemies)
        {
            if(enemy.levelToSpawn <= level && enemy.levelToStopSpawning > level)
            {
                enemiesToSpawn.Add(enemy);
            }
        }
    }

}
