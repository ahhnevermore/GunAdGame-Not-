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

    [Header("Lane Configuration")]
    public float laneWidth = 3f;
    public float maxArmyX = 9f;
    private ArmyLane currentArmyLane = ArmyLane.Center;
    private ArmyLane targetArmyLane = ArmyLane.Center;
    private float armyXVelocity = 0f;

    [Header("Gun Management")]
    public float swapCooldown = 2f;
    private GunType mainGun = GunType.Pistol;
    private GunType? sideGun = null;
    private float fireRate;
    private float nextSwapTime = 0f;
    private GunType? gunToKeepUpdating = null;
    private float stopUpdatingTime = 0f;

    [Header("Enemy Management")]
    public Enemy enemyPrefab; // For now, one type
    public int enemiesPerLane = 20;
    public int laneCount = 10;
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
        InitializeBulletPools();
        InitializeLanes();
        InitializeAvatarOffsets();

        mainGun = GunType.Pistol;
        sideGun = null;

        // Initial setup for avatars
        foreach (var avatar in avatars)
        {
            fireRate = avatar.SwitchTo(mainGun);
        }
        UpdateAvatarStrength();
    }

    void Update()
    {
        UpdateArmyMovement(Time.deltaTime);
        UpdateAvatarsFiring(Time.deltaTime);
        UpdateBullets(Time.deltaTime);
        UpdateEnemies(Time.deltaTime);

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
        if (sideGun.HasValue && Time.time >= nextSwapTime)
        {
            nextSwapTime = Time.time + swapCooldown;

            // Keep updating the old gun's bullets for the cooldown duration
            gunToKeepUpdating = mainGun;
            stopUpdatingTime = Time.time + swapCooldown;

            // Swap the guns
            GunType temp = mainGun;
            mainGun = sideGun.Value;
            sideGun = temp;
            foreach (var avatar in avatars)
            {
                fireRate = avatar.SwitchTo(mainGun);
            }
        }
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
                Vector3 spawnPos = avatar.guns[(int)avatar.activeGun].transform.position;

                // Calculate bullet power
                int bulletPower = (int)(Math.Pow(10f, (double)avatar.selfTier) * (double)(1 + avatar.gunTier) * Tiers.GunPower[avatar.activeGun]);

                b.SetupBullet(avatar.selfTier, avatar.gunTier, spawnPos, bulletPower);

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
                    enemy.transform.position += Vector3.back * enemy.speed * dt;
                    if (enemy.transform.position.z <= 0.5f)
                    {
                        // Check if an avatar is in this lane
                        bool avatarInLane = false;
                        foreach (var avatar in avatars)
                        {
                            if (avatar.gameObject.activeSelf && GetLaneForX(avatar.transform.position.x) == i)
                            {
                                avatarInLane = true;
                                break;
                            }
                        }

                        if (avatarInLane)
                        {
                            // Deal damage to army
                            armyStrength -= 10; // example damage
                            UpdateAvatarStrength();
                            enemy.gameObject.SetActive(false);
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
                lanes[i].enemies[j] = Instantiate(enemyPrefab, transform);
                lanes[i].enemies[j].gameObject.SetActive(false);
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

    int GetLaneForX(float xPos)
    {
        // road is from -15 to 15, total width 30.
        // 10 lanes, each 3 units wide.
        // lane 0: -15 to -12, lane 1: -12 to -9, ..., lane 9: 12 to 15
        int lane = Mathf.FloorToInt((xPos + 15f) / laneWidth);
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