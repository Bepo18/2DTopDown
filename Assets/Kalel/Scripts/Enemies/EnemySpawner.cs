using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [Tooltip("Enemy prefabs for rounds 1-2 (normal enemies)")]
    public GameObject[] normalEnemies; // e.g., Slime, Goblin, Orc
    [Tooltip("Boss prefab for the final round")]
    public GameObject bossPrefab;
    public Transform[] spawnPoints;

    [Header("Round Settings")]
    public int[] enemiesPerRound = { 3, 5, 8 }; // Round 3 includes the boss + random enemies
    public float timeBetweenRounds = 5f;

    private int currentRound = 0;
    private bool spawning = false;

    void Start()
    {
        StartCoroutine(SpawnRounds());
    }

    IEnumerator SpawnRounds()
    {
        while (currentRound < enemiesPerRound.Length)
        {
            if (!spawning)
            {
                spawning = true;
                yield return StartCoroutine(SpawnRound(currentRound));
                spawning = false;

                if (currentRound < enemiesPerRound.Length - 1)
                {
                    Debug.Log($"Round {currentRound + 1} complete! Next round in {timeBetweenRounds} seconds.");
                    yield return new WaitForSeconds(timeBetweenRounds);
                }
            }
            currentRound++;
        }

        Debug.Log("All rounds completed!");
    }

    IEnumerator SpawnRound(int roundIndex)
    {
        int enemiesToSpawn = enemiesPerRound[roundIndex];
        Debug.Log($"Starting Round {roundIndex + 1} - Spawning {enemiesToSpawn} enemies.");

        if (roundIndex < 2)
        {
            // Rounds 1 & 2: single enemy type per round
            GameObject prefab = normalEnemies[Mathf.Clamp(roundIndex, 0, normalEnemies.Length - 1)];
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Instantiate(prefab, point.position, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            // Round 3: one boss + random normal enemies
            Transform bossPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(bossPrefab, bossPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(1f);

            for (int i = 1; i < enemiesToSpawn; i++) // 1 slot taken by boss
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject randomEnemy = normalEnemies[Random.Range(0, normalEnemies.Length)];
                Instantiate(randomEnemy, point.position, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Wait until all enemies destroyed before next round
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
            yield return null;
    }


}
