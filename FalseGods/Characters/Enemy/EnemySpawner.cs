using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPosLeft, spawnPosRight;

    private float spawnDelay = 1f;
    private int spawnerCount;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && GameManager.instance.devMode) SpawnEnemy(false, true);
    }


    public void SpawnEnemyPack(int enemyCount, bool left, bool right)
    {
        if (!left && !right) 
        {
            print("No spawndirection set at " + gameObject.name + "!!!");
            return;
        }

        spawnerCount = enemyCount;
        StartCoroutine(SpawnEnemyPackCR(left, right));
    }

    private IEnumerator SpawnEnemyPackCR(bool left, bool right)
    {
        SpawnEnemy(left, right);
        yield return new WaitForSeconds(spawnDelay + Random.Range(-0.2f, 0.2f));
        spawnerCount--;
        if (spawnerCount > 0)
        {
            StartCoroutine(SpawnEnemyPackCR(left, right));
        }
    }

    public void SpawnEnemy(bool left, bool right)
    {
        Transform spawnPosToUse;

        if (left && right)
        {
            spawnPosToUse = Random.value < 0.5f ? spawnPosLeft : spawnPosRight;
        }
        else if (right) spawnPosToUse = spawnPosRight;
        else spawnPosToUse = spawnPosLeft;

        GameObject newEnemy = GameObject.Instantiate(enemyPrefab, spawnPosToUse.position, Quaternion.identity);
        newEnemy.GetComponent<EnemyCharacter>().AddEnemyToCombat();
    }

}
