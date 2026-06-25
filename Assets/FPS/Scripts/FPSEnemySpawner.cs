using UnityEngine;
using System.Collections;

namespace FPSGame
{
    public class FPSEnemySpawner : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public Transform[] spawnPoints;
        public Transform[] patrolPoints;
        public int maxEnemies = 3;
        public float respawnDelay = 5f;

        int _aliveCount;

        void Start()
        {
            for (int i = 0; i < maxEnemies; i++)
                SpawnEnemy();
        }

        void SpawnEnemy()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);

            FPSEnemy enemyScript = enemy.GetComponent<FPSEnemy>();
            if (enemyScript != null)
                enemyScript.patrolPoints = patrolPoints;

            FPSHealth health = enemy.GetComponent<FPSHealth>();
            if (health != null)
                health.OnDied += OnEnemyDied;

            _aliveCount++;
        }

        void OnEnemyDied(FPSHealth health)
        {
            health.OnDied -= OnEnemyDied;
            _aliveCount--;
            StartCoroutine(RespawnAfterDelay());
        }

        IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            SpawnEnemy();
        }
    }
}
