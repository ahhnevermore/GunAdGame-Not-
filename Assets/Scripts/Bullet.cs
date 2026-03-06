using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float maxDistance;
    public int power;
    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;



    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void OnDisable()
    {
        trailRenderer.Clear();
    }


    public void SetupBullet(Tier mesh, Tier trail, Vector3 startPos, int pow)
    {
        transform.position = startPos;
        power = pow;

        if (meshRenderer != null)
            meshRenderer.material.color = Tiers.UTColours[mesh][0];
        if (trailRenderer != null)
        {
            trailRenderer.startColor = Tiers.UTColours[trail][1];
            trailRenderer.endColor = Tiers.UTColours[mesh][2];
        }

        gameObject.SetActive(true);
    }
}