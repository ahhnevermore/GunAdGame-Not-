using UnityEngine;

public class HighwayRings : MonoBehaviour
{
    public Material mat;

    const int MAX_RINGS = 8;
    const float BASE_WIDTH = 2.5f;

    Vector4[] centers = new Vector4[MAX_RINGS];
    float[] radii = new float[MAX_RINGS];
    float[] widths = new float[MAX_RINGS];
    float[] baseWidths = new float[MAX_RINGS];
    Color[] ringColors = new Color[MAX_RINGS];



    public float ringSpeed = 10000f;
    public float maxRadius = 200f;
    public Color[] possibleColors = { Color.cyan, Color.green, Color.magenta };

    int ringIndex = 0;

    void Start()
{
    // Initialize radii and widths
    for (int i = 0; i < MAX_RINGS; i++)
    {
        radii[i] = -1;
        widths[i] = BASE_WIDTH;
    }

    // Spawn 9 test rings at different positions along Z
    float spacing = 5f; // distance between each ring
    for (int i = 0; i < 9; i++)
    {
        float zPos = i * spacing;
        SpawnRingAt(new Vector3(0,0,zPos));
    }
}


    void Update()
    {
        // Expand active rings
        for (int i = 0; i < MAX_RINGS; i++)
        {
            if (radii[i] >= 0)
            {
                radii[i] += ringSpeed * Time.deltaTime;

                if (radii[i] > maxRadius){
                    radii[i] = -1;
                }else{
                    widths[i] = baseWidths[i] * (maxRadius - radii[i]) / maxRadius;
                }
            }
        }

        // Send to shader
        mat.SetFloat("_RingCount", MAX_RINGS);
        mat.SetVectorArray("_RingCenters", centers);
        mat.SetFloatArray("_RingRadii", radii);
        mat.SetFloatArray("_RingWidths", widths);
        mat.SetColorArray("_RingColors", ringColors);
    }

    public void SpawnRingAt(Vector3 enemyPos, float width = 1.5f)
    {
        ringIndex = (ringIndex + 1) % MAX_RINGS;

        centers[ringIndex] = new Vector4(enemyPos.x, 0, enemyPos.z, 0);
        radii[ringIndex] = 0f;
        widths[ringIndex] = width;
        baseWidths[ringIndex]=width;


        // pick color
        ringColors[ringIndex] = possibleColors[ringIndex % possibleColors.Length];
    }

}
