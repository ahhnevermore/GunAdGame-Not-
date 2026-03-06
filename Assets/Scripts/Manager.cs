using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public enum GunType
{
    Pistol,
    Rifle,
    Sniper,
}

public enum FragmentType
{
    Pistol,
    Rifle,
    Sniper,
    Army
}

public enum ArmyLane
{
    Left,
    Center,
    Right
}
public enum MovementType
{
    Lerp,
    ConstantSpeed,
    SmoothDamp
}

public class Manager : MonoBehaviour
{
    [Header("UI")]
    public UIManager uiManager;

    [Header("Army Configuration")]
    public Avatar[] avatars;
    public int armyStrength = 100;
    public float armyXPosition = 0f;

    [Header("Movement")]
    public MovementType armyMovementType = MovementType.ConstantSpeed;
    public float pistolMoveSpeed = 15f;
    public float rifleMoveSpeed = 8f;
    public float sniperMoveSpeed = 10f;
    [Tooltip("The approximate time it will take to reach the target. A smaller value will reach the target faster.")]
    public float smoothTime = 0.3f;
    [Tooltip("How close an enemy needs to be to an avatar on the X-axis to collide.")]
    public float avatarCollisionWidth = 1.0f;

    [Header("Lane Configuration")]
    public float roadWidth = 30f;
    public int laneCount = 10;
    [HideInInspector]
    public float laneWidth; // Calculated at runtime
    public float maxArmyX = 9f;
    private ArmyLane currentArmyLane = ArmyLane.Center;
    private ArmyLane targetArmyLane = ArmyLane.Center;
    private float armyXVelocity = 0f;

    [Header("Fragment System")]
    [Tooltip("The low, starting cost for the first army fragment upgrade.")]
    public int initialArmyFragmentThreshold = 10;
    [Tooltip("Cost for each tier upgrade: Normal->Ice, Ice->Arcane, Arcane->Legendary")]
    public int[] pistolFragmentThresholds = { 500, 2000, 5000 };
    [Tooltip("Cost for each tier upgrade: Normal->Ice, Ice->Arcane, Arcane->Legendary")]
    public int[] rifleFragmentThresholds = { 1000, 4000, 10000 };
    [Tooltip("Cost for each tier upgrade: Normal->Ice, Ice->Arcane, Arcane->Legendary")]
    public int[] sniperFragmentThresholds = { 2500, 8000, 20000 };
    public float fragmentDropMultiplier = 5f;

    private float currentArmyFragmentThreshold;
    private Dictionary<FragmentType, int> currentFragments;
    private Dictionary<GunType, Tier> gunTiers;

    [Header("Gun Management")]
    public float swapCooldown = 2f;
    private GunType mainGun = GunType.Pistol;
    private List<GunType> unlockedGuns = new List<GunType>();
    private int currentGunIndex = 0;
    private float fireRate;
    private float nextSwapTime = 0f;
    private GunType? gunToKeepUpdating = null;
    private float stopUpdatingTime = 0f;

    [Header("Difficulty and Scoring")]
    public long score = 0;
    public float timeToMaxDifficulty = 180f; // 3 minutes to reach max difficulty
    public float minTimeBetweenWaves = 1f; // Fastest spawn rate
    public int maxAdditionalEnemies = 10; // How many extra enemies spawn at max difficulty
    private float gameTime = 0f;

    [Header("Spawning")]
    public Enemy[] enemyPrefabs;
    public float initialTimeBetweenWaves = 5f;
    public int minEnemiesPerWave = 3;
    public int maxEnemiesPerWave = 8;

    private float nextWaveTime = 0f;

    [Header("Enemy Management")]
    public int enemiesPerLane = 20;
    public float sortLanesInterval = 5f;
    private float nextSortTime = 0f;

    [Header("Bullet Pools")]
    public Bullet pistolPrefab;
    public Bullet riflePrefab;
    public Bullet sniperPrefab;
    public int poolSize = 50;
    private Bullet[] pistolPool;
    private Bullet[] riflePool;
    private Bullet[] sniperPool;
    private int pistolIdx = 0;
    private int rifleIdx = 0;
    private int sniperIdx = 0;

    private Lane[] lanes;
    private Vector3[] avatarInitialOffsets;

    private readonly int[] powerTiers = { 1000, 100, 10, 1 };


    void Awake()
    {
        // Calculate lane width based on fixed road width
        laneWidth = roadWidth / laneCount;

        InitializeFragments();
        InitializeBulletPools();
        InitializeLanes();
        InitializeAvatarOffsets();

        mainGun = GunType.Pistol;
        unlockedGuns.Add(GunType.Pistol);
        currentGunIndex = 0;

        // Initial setup for avatars
        foreach (var avatar in avatars)
        {
            fireRate = avatar.SwitchTo(mainGun);
        }
        UpdateAvatarStrength();
        if (uiManager != null) uiManager.UpdateScore(score);
    }

    void Update()
    {
        if (Time.timeScale == 0) return; // Don't update if game is over

        gameTime += Time.deltaTime;
        UpdateArmyMovement(Time.deltaTime);
        UpdateAvatarsFiring(Time.deltaTime);
        UpdateBullets(Time.deltaTime);
        UpdateEnemies(Time.deltaTime);
        UpdateSpawning();

#if UNITY_EDITOR
        HandleDebugInput();
#endif

        if (Time.time >= nextSortTime)
        {
            SortAllLanes();
            nextSortTime = Time.time + sortLanesInterval;
        }
    }

#if UNITY_EDITOR
    void HandleDebugInput()
    {
        // Cycle through guns
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            mainGun = (GunType)(((int)mainGun + 1) % Enum.GetValues(typeof(GunType)).Length);
            foreach (var avatar in avatars)
            {
                fireRate = avatar.SwitchTo(mainGun);
            }
        }

        // Adjust army power
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            armyStrength += 1;
            UpdateAvatarStrength();
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            armyStrength += 10;
            UpdateAvatarStrength();
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            armyStrength += 100;
            UpdateAvatarStrength();
        }
    }
#endif

    #region Input Methods
    public void Left()
    {
        if (targetArmyLane == ArmyLane.Right)
            targetArmyLane = ArmyLane.Center;
        else if (targetArmyLane == ArmyLane.Center)
            targetArmyLane = ArmyLane.Left;
    }
    public void Right()
    {
        if (targetArmyLane == ArmyLane.Left)
            targetArmyLane = ArmyLane.Center;
        else if (targetArmyLane == ArmyLane.Center)
            targetArmyLane = ArmyLane.Right;
    }
    public void Swap()
    {
        // Only allow swapping if more than one gun is unlocked and cooldown is over
        if (unlockedGuns.Count > 1 && Time.time >= nextSwapTime)
        {
            nextSwapTime = Time.time + swapCooldown;

            // Keep updating the old gun's bullets for the cooldown duration
            gunToKeepUpdating = mainGun;
            stopUpdatingTime = Time.time + swapCooldown;

            // Cycle to the next gun in the list
            currentGunIndex = (currentGunIndex + 1) % unlockedGuns.Count;
            mainGun = unlockedGuns[currentGunIndex];

            foreach (var avatar in avatars)
            {
                fireRate = avatar.SwitchTo(mainGun);
            }
        }
    }
    #endregion

    #region Spawning and Rewards
    void InitializeFragments()
    {
        currentArmyFragmentThreshold = initialArmyFragmentThreshold;

        currentFragments = new Dictionary<FragmentType, int>
        {
            { FragmentType.Army, 0 },
            { FragmentType.Pistol, 0 },
            { FragmentType.Rifle, 0 },
            { FragmentType.Sniper, 0 }
        };

        gunTiers = new Dictionary<GunType, Tier>
        {
            { GunType.Pistol, Tier.Normal },
            { GunType.Rifle, Tier.Normal },
            { GunType.Sniper, Tier.Normal }
        };
    }

    void UpdateSpawning()
    {
        if (Time.time >= nextWaveTime)
        {
            float difficultyPercent = Mathf.Clamp01(gameTime / timeToMaxDifficulty);
            SpawnEnemyWave(difficultyPercent);

            // Adjust time for next wave based on difficulty
            float currentTimeBetweenWaves = Mathf.Lerp(initialTimeBetweenWaves, minTimeBetweenWaves, difficultyPercent);
            nextWaveTime = Time.time + currentTimeBetweenWaves;
        }
    }

    void SpawnEnemyWave(float difficulty)
    {
        // Generate a contiguous "weak block" (gap) to allow dodging.
        // The gap width scales with lane count (approx 1/3 of the road).
        int gapWidth = Mathf.Max(2, laneCount / 3);
        // Vary the width slightly to add variety
        gapWidth += UnityEngine.Random.Range(-1, 2);
        gapWidth = Mathf.Clamp(gapWidth, 2, laneCount - 1);

        // Pick a random start position for the weak block
        int gapStart = UnityEngine.Random.Range(0, laneCount - gapWidth + 1);

        for (int i = 0; i < laneCount; i++)
        {
            // Determine if this lane is inside the "Weak" block
            bool isWeakLane = (i >= gapStart && i < gapStart + gapWidth);

            bool shouldSpawn = false;
            Tier tier = Tier.Normal;

            if (isWeakLane)
            {
                // Weak Lane: High chance of being a gap (empty).
                // If it does spawn, it's always a Normal (weak) enemy.
                // 20% chance to spawn, meaning 80% chance of a gap.
                if (UnityEngine.Random.value < 0.2f)
                {
                    shouldSpawn = true;
                    tier = Tier.Normal;
                }
            }
            else
            {
                // Strong Lane: High chance to spawn.
                // Tier increases with difficulty.
                // Spawn chance scales from 40% to 90% based on difficulty.
                float spawnChance = Mathf.Lerp(0.4f, 0.9f, difficulty);
                if (UnityEngine.Random.value < spawnChance)
                {
                    shouldSpawn = true;
                    tier = GetRandomTier(difficulty);
                }
            }

            if (shouldSpawn)
            {
                Lane lane = lanes[i];
                // Find an inactive enemy in the selected lane
                for (int j = 0; j < lane.enemies.Length; j++)
                {
                    // Use a circular search to find next available spot
                    int idx = (lane.spawnIndex + j) % lane.enemies.Length;
                    if (!lane.enemies[idx].gameObject.activeSelf)
                    {
                        float xPos = GetXForLane(i);
                        float zPos = 50f; // Spawn way ahead
                        Vector3 spawnPos = new Vector3(xPos, 0, zPos);

                        lane.enemies[idx].Initialize(this, spawnPos, i, tier);
                        break; // Found a spot, exit loop
                    }
                }
            }
        }
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        // Award points based on base score and tier
        long scoreMultiplier = (long)Mathf.Pow(10, (int)enemy.tier);
        score += enemy.baseScoreValue * scoreMultiplier;
        if (uiManager != null) uiManager.UpdateScore(score);

        // Grant fragments
        int fragmentsDropped = enemy.baseFragmentDropAmount * (int)Mathf.Pow(fragmentDropMultiplier, (int)enemy.tier);
        currentFragments[enemy.fragmentDropType] += fragmentsDropped;

        CheckForUpgrades(enemy.fragmentDropType);
    }

    void CheckForUpgrades(FragmentType type)
    {
        switch (type)
        {
            case FragmentType.Army:
                if (currentFragments[type] >= currentArmyFragmentThreshold)
                {
                    currentFragments[type] -= (int)currentArmyFragmentThreshold;
                    armyStrength += 10; // Flat power boost
                    UpdateAvatarStrength();
                    currentArmyFragmentThreshold *= 1.5f; // Increase cost for the next upgrade
                }
                break;
            case FragmentType.Pistol:
                HandleGunUpgrade(GunType.Pistol);
                break;
            case FragmentType.Rifle:
                HandleGunUpgrade(GunType.Rifle);
                break;
            case FragmentType.Sniper:
                HandleGunUpgrade(GunType.Sniper);
                break;
        }
    }

    void HandleGunUpgrade(GunType gunType)
    {
        Tier currentTier = gunTiers[gunType];
        if (currentTier >= Tier.Legendary) return; // Already max tier

        FragmentType fragType = (FragmentType)Enum.Parse(typeof(FragmentType), gunType.ToString());
        int[] thresholds = GetThresholdsForGun(gunType);

        // Ensure there is a threshold defined for the next tier
        if ((int)currentTier >= thresholds.Length) return;

        int requiredFragments = thresholds[(int)currentTier];

        if (currentFragments[fragType] >= requiredFragments)
        {
            currentFragments[fragType] -= requiredFragments;

            // If this is the first time collecting enough, unlock the gun
            if (!unlockedGuns.Contains(gunType))
            {
                unlockedGuns.Add(gunType);
            }

            // Upgrade the tier
            gunTiers[gunType]++;
        }
    }

    int[] GetThresholdsForGun(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.Pistol: return pistolFragmentThresholds;
            case GunType.Rifle: return rifleFragmentThresholds;
            case GunType.Sniper: return sniperFragmentThresholds;
            default: return new int[0];
        }
    }

    private Tier GetRandomTier(float difficulty)
    {
        // This method determines which enemy tier to spawn based on the current difficulty (a value from 0 to 1).
        // You can adjust these thresholds and probabilities to fine-tune the game's difficulty curve.

        float rand = UnityEngine.Random.value;

        // Phase 1: At low difficulty, only spawn Normal enemies to give the player a chance to start.
        if (difficulty < 0.1f)
        {
            return Tier.Normal;
        }

        // Phase 2: Gradually introduce tougher enemies as difficulty increases.
        // The probability of Normal enemies decreases as the game gets harder.
        if (rand < 0.7f - (difficulty * 0.5f))
        {
            return Tier.Normal;
        }

        // Ice enemies can only start appearing after 30% difficulty.
        if (difficulty > 0.3f && rand < 0.9f - (difficulty * 0.2f))
        {
            return Tier.Ice;
        }

        // Arcane enemies are reserved for later in the game (after 60% difficulty).
        if (difficulty > 0.6f && rand < 0.98f)
        {
            return Tier.Arcane;
        }

        // Legendary enemies are very rare and only appear in the end-game (after 80% difficulty).
        if (difficulty > 0.8f)
        {
            return Tier.Legendary;
        }

        return Tier.Normal; // Fallback to ensure something always spawns.
    }
    #endregion

    #region Update Logic
    void UpdateArmyMovement(float dt)
    {
        float currentMoveSpeed;
        switch (mainGun)
        {
            case GunType.Pistol:
                currentMoveSpeed = pistolMoveSpeed;
                break;
            case GunType.Rifle:
                currentMoveSpeed = rifleMoveSpeed;
                break;
            case GunType.Sniper:
                currentMoveSpeed = sniperMoveSpeed;
                break;
            default:
                currentMoveSpeed = 5f; // Fallback
                break;
        }

        float targetX;
        switch (targetArmyLane)
        {
            case ArmyLane.Left:
                targetX = -maxArmyX;
                break;
            case ArmyLane.Right:
                targetX = maxArmyX;
                break;
            default:
                targetX = 0;
                break;
        }

        // If we are already at the target, snap to the exact position and do nothing.
        if (Mathf.Abs(armyXPosition - targetX) < 0.01f)
        {
            armyXPosition = targetX;
            armyXVelocity = 0f; // Reset velocity for SmoothDamp
            return;
        }

        float previousX = armyXPosition;

        switch (armyMovementType)
        {
            case MovementType.Lerp:
                // Eases out of the movement
                armyXPosition = Mathf.Lerp(armyXPosition, targetX, currentMoveSpeed * dt);
                break;
            case MovementType.ConstantSpeed:
                // Moves at a constant speed
                armyXPosition = Mathf.MoveTowards(armyXPosition, targetX, currentMoveSpeed * dt);
                break;
            case MovementType.SmoothDamp:
                // Eases in and out of the movement
                armyXPosition = Mathf.SmoothDamp(armyXPosition, targetX, ref armyXVelocity, smoothTime, currentMoveSpeed, dt);
                break;
        }

        float deltaX = armyXPosition - previousX;

        foreach (var avatar in avatars)
        {
            if (avatar.gameObject.activeSelf)
            {
                avatar.transform.position += new Vector3(deltaX, 0, 0);
            }
        }

        // Update current lane if we're close enough to the target
        if (Mathf.Abs(armyXPosition - targetX) < 0.1f)
        {
            currentArmyLane = targetArmyLane;
        }
    }

    void UpdateAvatarsFiring(float dt)
    {
        foreach (var avatar in avatars)
        {
            if (avatar.gameObject.activeSelf && Time.time >= avatar.nextFireTime)
            {
                Bullet b = GetNext(avatar.activeGun);
                // Use avatar position but raise it up to prevent floor clipping (approx gun height)
                Vector3 spawnPos = avatar.transform.position + Vector3.up * 1.5f;

                // Get the current global tier for this weapon
                Tier currentGunTier = gunTiers[avatar.activeGun];

                // Calculate bullet power
                int bulletPower = (int)(Math.Pow(10f, (double)avatar.selfTier) * (double)(1 + currentGunTier) * Tiers.GunPower[avatar.activeGun]);

                b.SetupBullet(currentGunTier, avatar.selfTier, spawnPos, bulletPower);

                avatar.nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }

    void UpdateBullets(float dt)
    {
        // 1. Always update the main gun's bullet pool
        UpdateBulletPool(GetPoolForType(mainGun), dt);

        // 2. If we recently swapped, keep updating the old gun's pool for a bit
        if (gunToKeepUpdating.HasValue)
        {
            if (Time.time < stopUpdatingTime)
            {
                // Ensure we don't update the same pool twice
                if (gunToKeepUpdating.Value != mainGun)
                {
                    UpdateBulletPool(GetPoolForType(gunToKeepUpdating.Value), dt);
                }
            }
            else
            {
                // Cooldown finished, stop updating the secondary pool
                gunToKeepUpdating = null;
            }
        }
    }

    void UpdateBulletPool(Bullet[] pool, float dt)
    {
        foreach (Bullet bullet in pool)
        {
            if (bullet.gameObject.activeSelf)
            {
                // 1. Move bullet
                bullet.transform.position += Vector3.forward * bullet.speed * dt;

                // 2. Check for out of bounds
                if (bullet.transform.position.z > bullet.maxDistance)
                {
                    bullet.gameObject.SetActive(false);
                    continue; // Go to next bullet
                }

                // 3. Check for collision
                int laneIndex = GetLaneForX(bullet.transform.position.x);
                Lane lane = lanes[laneIndex];
                for (int i = lane.frontEnemyIndex; i < lane.spawnIndex; i++)
                {
                    Enemy enemy = lane.enemies[i];
                    if (enemy.gameObject.activeSelf)
                    {
                        // Simple distance check on Z axis.
                        if (Mathf.Abs(bullet.transform.position.z - enemy.transform.position.z) < 0.5f)
                        {
                            enemy.TakeDamage(bullet.power);
                            bullet.gameObject.SetActive(false);
                            break; // Bullet is used up, exit enemy loop
                        }
                    }
                }
            }
        }
    }

    void UpdateEnemies(float dt)
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            for (int j = lanes[i].frontEnemyIndex; j < lanes[i].spawnIndex; j++)
            {
                Enemy enemy = lanes[i].enemies[j];
                if (enemy.gameObject.activeSelf)
                {
                    // Let the enemy decide if it wants to switch lanes
                    enemy.UpdateBehavior();

                    // Move the enemy on both axes
                    Vector3 currentPos = enemy.transform.position;
                    float newX = Mathf.MoveTowards(currentPos.x, enemy.TargetXPosition, 3f * dt); // 3f is lane switch speed
                    float newZ = currentPos.z - enemy.speed * dt;
                    enemy.transform.position = new Vector3(newX, 0, newZ);

                    // Only check for collision ONCE when crossing the line
                    if (!enemy.hasAttemptedDamage && enemy.transform.position.z <= 0f)
                    {
                        enemy.hasAttemptedDamage = true; // This enemy has now made its attack run

                        // Check for precise collision with any active avatar in the same lane
                        foreach (var avatar in avatars)
                        {
                            if (avatar.gameObject.activeSelf && GetLaneForX(avatar.transform.position.x) == i)
                            {
                                // Check if the enemy is close enough on the X-axis
                                if (Mathf.Abs(avatar.transform.position.x - enemy.transform.position.x) < avatarCollisionWidth)
                                {
                                    // Deal damage to army
                                    armyStrength -= 10; // example damage
                                    UpdateAvatarStrength();
                                    enemy.gameObject.SetActive(false);

                                    break; // Enemy is destroyed, stop checking against other avatars
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Army Management
    void InitializeAvatarOffsets()
    {
        avatarInitialOffsets = new Vector3[avatars.Length];
        for (int i = 0; i < avatars.Length; i++)
        {
            // Store initial offset from the army's center (which is armyXPosition, initially 0)
            avatarInitialOffsets[i] = avatars[i].transform.position;
        }
    }

    void UpdateAvatarStrength()
    {
        if (armyStrength <= 0)
        {
            armyStrength = 0;
            if (uiManager != null) uiManager.ShowGameOver();
            Debug.Log("Game Over! Army strength is 0.");
            Time.timeScale = 0f; // Pause the game
            // Optionally, disable player input here
            return; // Stop processing further
        }

        int remaining = armyStrength;
        int avatarIndex = 0;

        // hide all first
        for (int i = 0; i < avatars.Length; i++)
            avatars[i].gameObject.SetActive(false);

        // assign power using greedy place values
        foreach (int power in powerTiers)
        {
            while (remaining >= power && avatarIndex < avatars.Length)
            {
                avatars[avatarIndex].gameObject.SetActive(true);

                // Set position based on current army X and initial offset
                Vector3 initialOffset = avatarInitialOffsets[avatarIndex];
                avatars[avatarIndex].transform.position = new Vector3(armyXPosition + initialOffset.x, initialOffset.y, initialOffset.z);

                avatars[avatarIndex].SetPower(power);

                remaining -= power;
                avatarIndex++;
            }
        }
    }
    #endregion

    #region Lane & Enemy Management
    void InitializeLanes()
    {
        lanes = new Lane[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            lanes[i] = new Lane();
            lanes[i].enemies = new Enemy[enemiesPerLane];
            for (int j = 0; j < enemiesPerLane; j++)
            {
                // Pick a random enemy prefab from the provided list
                if (enemyPrefabs.Length > 0)
                {
                    Enemy prefabToSpawn = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
                    lanes[i].enemies[j] = Instantiate(prefabToSpawn, transform);
                    lanes[i].enemies[j].gameObject.SetActive(false);
                }
            }
        }
    }

    void SortAllLanes()
    {
        foreach (var lane in lanes)
        {
            // Sort by active status (active first), then by Z position (descending)
            var sortedEnemies = lane.enemies.OrderByDescending(e => e.gameObject.activeSelf).ThenByDescending(e => e.transform.position.z).ToArray();
            lane.enemies = sortedEnemies;

            lane.frontEnemyIndex = 0; // The first active one is now at the front
            lane.spawnIndex = lane.enemies.Count(e => e.gameObject.activeSelf);

            // if no active enemies, front index should be where spawn index is.
            if (lane.spawnIndex == 0)
            {
                lane.frontEnemyIndex = 0;
            }
        }
    }

    public float GetXForLane(int laneIndex)
    {
        float startX = -(roadWidth) / 2f;
        return startX + laneIndex * laneWidth + laneWidth * 0.5f;
    }

    int GetLaneForX(float xPos)
    {
        float halfWidth = roadWidth / 2f;
        int lane = Mathf.FloorToInt((xPos + halfWidth) / laneWidth);
        return Mathf.Clamp(lane, 0, laneCount - 1);
    }
    #endregion

    #region Bullet Pooling
    Bullet[] GetPoolForType(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.Pistol:
                return pistolPool;
            case GunType.Rifle:
                return riflePool;
            case GunType.Sniper:
                return sniperPool;
            default:
                return null;
        }
    }

    void InitializeBulletPools()
    {
        pistolPool = new Bullet[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pistolPool[i] = Instantiate(pistolPrefab, transform);
            pistolPool[i].gameObject.SetActive(false);
        }

        riflePool = new Bullet[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            riflePool[i] = Instantiate(riflePrefab, transform);
            riflePool[i].gameObject.SetActive(false);
        }

        sniperPool = new Bullet[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            sniperPool[i] = Instantiate(sniperPrefab, transform);
            sniperPool[i].gameObject.SetActive(false);
        }
    }

    public Bullet GetNext(GunType guntype)
    {
        Bullet b = null;
        switch (guntype)
        {
            case GunType.Pistol:
                b = pistolPool[pistolIdx];
                pistolIdx = (pistolIdx + 1) % poolSize;
                break;
            case GunType.Rifle:
                b = riflePool[rifleIdx];
                rifleIdx = (rifleIdx + 1) % poolSize;
                break;
            case GunType.Sniper:
                b = sniperPool[sniperIdx];
                sniperIdx = (sniperIdx + 1) % poolSize;
                break;
        }
        return b;
    }
    #endregion
}

public class Lane
{
    public Enemy[] enemies;
    public int spawnIndex = 0;
    public int frontEnemyIndex = 0;
}