using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float maxDistance = 30f;
    public int power = 1;
    public bool pierce = false;


    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;



    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }


    void Update()
    {
        if (transform.position.z < maxDistance)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }
    void OnDisable()
    {
        trailRenderer.Clear();
    }


    public void SetupBullet(Tier mesh, Tier trail, Vector3 startPos, int pow)
    {
        transform.position = startPos;

        if (meshRenderer != null)
            meshRenderer.material.color = Tiers.UTColours[mesh][0];

        if (trailRenderer != null)
        {
            // Set start & end color
            trailRenderer.startColor = Tiers.UTColours[trail][1];
            trailRenderer.endColor = Tiers.UTColours[mesh][2];  // fades out
        }
        gameObject.SetActive(true);
        power = pow;
    }
    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            int leftOver = damageable.TakeDamage(power);
            if (pierce && leftOver > 0)
            {
                power = leftOver;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

public interface IDamageable
{
    int TakeDamage(int amount);
}