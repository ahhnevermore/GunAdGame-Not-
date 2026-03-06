using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseScoreValue = 10;
    private int currentHealth;

    [Header("Fragment Drops")]
    public FragmentType fragmentDropType;
    public int baseFragmentDropAmount = 1;

    [Header("Movement")]
    public float speed = 2f;
    public bool canSwitchLanes = false;
    public float laneSwitchInterval = 3f;
    private float nextLaneSwitchTime = 0f;

    [Header("State")]
    public Tier tier = Tier.Normal;
    public int laneIndex = 0;
    private Manager manager;
    public bool hasAttemptedDamage = false;

    // We only need to store the target X, the Manager will handle the movement
    private float targetXPosition;

    public float TargetXPosition => targetXPosition;


    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            manager.OnEnemyKilled(this);
            gameObject.SetActive(false);
        }
    }

    public void Initialize(Manager manager, Vector3 position, int lane, Tier enemyTier)
    {
        this.manager = manager;
        transform.position = position;
        targetXPosition = position.x;
        laneIndex = lane;
        tier = enemyTier;
        hasAttemptedDamage = false;

        // Calculate health based on tier
        float healthMultiplier = Mathf.Pow(10, (int)tier); // 1, 10, 100, 1000
        currentHealth = (int)(baseHealth * healthMultiplier);
        
        // Slightly increase scale based on tier
        transform.localScale = Vector3.one * (1f + ((int)tier * 0.2f));
        
        nextLaneSwitchTime = Time.time + laneSwitchInterval;
        gameObject.SetActive(true);
    }

    public void UpdateBehavior()
    {
        if (!canSwitchLanes || Time.time < nextLaneSwitchTime)
        {
            return;
        }

        // Time to switch lanes
        int direction = (Random.value < 0.5f) ? -1 : 1;
        int newLane = Mathf.Clamp(laneIndex + direction, 0, manager.laneCount - 1);

        if (newLane != laneIndex)
        {
            laneIndex = newLane;
            targetXPosition = manager.GetXForLane(laneIndex);
        }

        nextLaneSwitchTime = Time.time + laneSwitchInterval;
    }
}