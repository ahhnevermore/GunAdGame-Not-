using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int health = 100;
    public float speed = 2f;
    public Tier tier = Tier.Normal;
    public int laneIndex = 0;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            gameObject.SetActive(false);
            // a particle effect or something can be spawned here
        }
    }

    public void Initialize(Vector3 position, int lane, Tier enemyTier)
    {
        transform.position = position;
        laneIndex = lane;
        tier = enemyTier;
        // set health, speed, scale based on tier
        
        gameObject.SetActive(true);
    }
}