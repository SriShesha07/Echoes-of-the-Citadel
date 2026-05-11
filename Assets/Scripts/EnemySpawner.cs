using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform player;

    public float spawnInterval = 4f;
    public int maxEnemies = 10;

    private int spawnedCount;
    private bool isSpawning;

    public void StartSpawning()
    {
        if (isSpawning)
        {
            return;
        }

        isSpawning = true;
        spawnedCount = 0;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (spawnedCount < maxEnemies)
        {
            SpawnEnemy();
            spawnedCount++;

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner is missing enemyPrefab or spawnPoints.");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.player = player;
            enemyAI.enabled = true;
        }

        EnemyAttack enemyAttack = enemyObject.GetComponent<EnemyAttack>();
        if (enemyAttack != null)
        {
            enemyAttack.player = player;
            enemyAttack.enabled = true;
        }
    }
}