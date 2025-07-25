using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Random = System.Random;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;
    
    public List<Path> paths = new List<Path>();
    public List<Path> flipped_paths = new List<Path>();
    public List<Path> wave_paths = new List<Path>();

    public bool initialized = false;
    public LevelData levelData;
    [NonSerialized] public float levelCountdown;
    private WaveData[] waveDatas;

    [SerializeField] public Transform spawnPoint;

    public int EnemiesAlive = 0;

    private bool flipedPathing = false;

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {
        levelCountdown = levelData.countdown;
        levelData.currentWaveIndex = 0;
        levelData.readyToCountDown = true;
        levelData.spawnNextWave = false;
        waveDatas = levelData.waves;

        foreach (var wave in waveDatas)
        {
            wave.ResetEnemiesLeft();
        }
        initialized = true;
    }

    private bool waveSpawning = false;

    private void Update()
    {
        if (!initialized) return;
        
        levelData.UpdateLevel();

        if (levelData.spawnNextWave && !waveSpawning)
        {
            waveSpawning = true;
            StartCoroutine(SpawnWave());
        }

        // Reset flag once coroutine is done
        if (!levelData.spawnNextWave && waveSpawning)
        {
            waveSpawning = false;
        }
    }

    public void Restart()
    {
        waveSpawning = false;
        Initialize();
    }

    public void FlipPathing()
    {
        flipedPathing = !flipedPathing;
    }

    private IEnumerator SpawnWave()
    {
        levelData.spawnNextWave = false;

        if (levelData.currentWaveIndex >= levelData.waves.Length)
            yield break;

        WaveData wave = levelData.waves[levelData.currentWaveIndex];
        wave.ResetEnemiesLeft();

        for (int i = 0; i < wave.enemies.Length; i++)
        {
            Enemy enemyPrefab = wave.enemies[i];

            if (enemyPrefab is TidalWave wavePrefab) //special case for wave attacks
            {
                Debug.Log("Spawning Wave attack");
                TidalWave waveEnemy = Instantiate(wavePrefab, spawnPoint.position, Quaternion.identity, spawnPoint);

                waveEnemy.path = wave_paths[flipedPathing ? 1 : 0];
                
                // flip wave
                Vector3 scale = waveEnemy.transform.localScale;
                scale.x = flipedPathing ? -1 : 1;
                waveEnemy.transform.localScale = scale;

                waveEnemy.levelData = levelData;
                waveEnemy.currentNodeId = 1;
            }
            else if (enemyPrefab is Enemy pathingEnemyPrefab)
            {
                Enemy pathingEnemy = Instantiate(pathingEnemyPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);

                // Choose path list based on if the paths are flipped
                List<Path> pathList = flipedPathing ? flipped_paths : paths;
                // flip sprite
                Vector3 scale = pathingEnemy.transform.localScale;
                scale.x = flipedPathing ? -1 : 1;
                pathingEnemy.transform.localScale = scale;

                pathingEnemy.path = pathList[UnityEngine.Random.Range(0, pathList.Count)];
                pathingEnemy.levelData = levelData;
                pathingEnemy.currentNodeId = 1;
            }
            else
            {
                Enemy enemy = Instantiate(enemyPrefab, spawnPoint.transform);
                enemy.transform.SetParent(spawnPoint.transform);
            }

            EnemiesAlive++;
            yield return new WaitForSeconds(wave.timeToNextEnemy);
        }

        levelData.waveSpawned = true;
        Debug.Log("Wave " + levelData.currentWaveIndex + " spawned.");
    }
}