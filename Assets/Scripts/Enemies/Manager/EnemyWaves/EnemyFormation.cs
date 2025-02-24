using UnityEngine;

[System.Serializable]
public class EnemyFormation
{
    // Enemies in formation
    public EnemySpawnPool[] enemiesToSpawn;             // Enemies in formation
    public float formationDelay = 1f;                   // Determines the time delay before this formation
    public SpawnOrder spawnOrder = SpawnOrder.Random;   // Determines how are enemies picked spawnpool

    public int LastUsedIndex { get; private set; } = 0; // Last index used. Used to keep track of enemy unit spawn times

    // Varibles
    private int _currentIndex = 0;                      // Used both in AllFromOne and Rotating spawn order
                                                        // AllFromOne = +1 when everything is spawned from the current formation
                                                        // Rotating = +1 after every spawn and loops formations
    private bool firstRandomIndexUsed;

    /// <summary>
    /// Random spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetRandomEnemyType()
    {
        // Set a default value
        GameEntity e = GameEntity.Null;

        // Ensure that formation contains enemies
        if (ContainsEnemies())
        {
            // Keep selecting a valid GameEntity type randomly
            while (e == GameEntity.Null)
            {
                // Use LastUsedIndex for the first iteration, then randomize
                int randomIndex = firstRandomIndexUsed ? Random.Range(0, enemiesToSpawn.Length) : LastUsedIndex;
                LastUsedIndex = randomIndex;
                firstRandomIndexUsed = true;

                // Get a GameEntity from the selected index
                e = enemiesToSpawn[randomIndex].GetEnemy();
            }
        }
        return e;
    }

    /// <summary>
    /// Get all from one spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetAllFromOneEnemyType()
    {
        GameEntity enemyType = GameEntity.Null;
        // While no enemy type has been found or the index has reached the end
        while (enemyType == GameEntity.Null && _currentIndex < enemiesToSpawn.Length)
        {
            // Get enemy type
            enemyType = enemiesToSpawn[_currentIndex].GetEnemy();
            LastUsedIndex = _currentIndex;

            // If the type is null, then no more of that type can be spawned
            // Move to next element in array
            if (enemyType == GameEntity.Null)
            {
                _currentIndex++;
            }
        }

        return enemyType;
    }

    /// <summary>
    /// Rotating spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetRotatingEnemyType()
    {
        GameEntity enemyType = GameEntity.Null;
        // If enemies remain
        if (ContainsEnemies())
        {
            while (enemyType == GameEntity.Null)
            {
                // Get enemy type (returns null if no more enemies remain
                enemyType = enemiesToSpawn[_currentIndex].GetEnemy();
                LastUsedIndex = _currentIndex;

                // Increase index if enemy type is null
                if (enemyType == GameEntity.Null)
                {
                    IncreaseRotatingIndex();
                }
            }
        }

        // Increase index but ensure it stays within bounds
        IncreaseRotatingIndex();

        return enemyType;
    }

    /// <summary>
    /// Ensures that the index loops
    /// </summary>
    private void IncreaseRotatingIndex()
    {
        _currentIndex++;
        if (_currentIndex >= enemiesToSpawn.Length)
        {
            _currentIndex = 0;
        }
    }

    /// <summary>
    /// Does the formation still have enemies to spawn
    /// </summary>
    /// <returns></returns>
    public bool ContainsEnemies()
    {
        foreach (EnemySpawnPool enemy in enemiesToSpawn)
        {
            if (enemy.EnemiesSpawned < enemy.count)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ensure that if spawing is random, the spawn delays are correctly used 
    /// </summary>
    public void InitializeFirstRandomIndex()
    {
        if (spawnOrder != SpawnOrder.Random) { return; }
        firstRandomIndexUsed = false;
        LastUsedIndex = Random.Range(0, enemiesToSpawn.Length);
    }

    /// <summary>
    /// Returns the GameEntity of the next enemy to be sapwned
    /// </summary>
    /// <returns></returns>
    public GameEntity GetEnemyTypeFromFormation()
    {
        return spawnOrder switch
        {
            SpawnOrder.AllFromOne => GetAllFromOneEnemyType(),
            SpawnOrder.Random => GetRandomEnemyType(),
            SpawnOrder.Rotating => GetRotatingEnemyType(),
            _ => GameEntity.Null,
        };
    }
}