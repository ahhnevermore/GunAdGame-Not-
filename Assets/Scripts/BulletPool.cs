using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    Pistol,
    Rifle,
    Sniper,
}
public class BulletPool : MonoBehaviour
{
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

    void Awake()
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
        Bullet b = pistolPool[0];
        switch (guntype)
        {
            case GunType.Pistol:
                {
                    b = pistolPool[pistolIdx];
                    pistolIdx++;
                    if (pistolIdx >= poolSize)
                    {
                        pistolIdx = 0;
                    }
                }
                break;
            case GunType.Rifle:
                {
                    b = riflePool[rifleIdx];
                    rifleIdx++;
                    if (rifleIdx >= poolSize)
                    {
                        rifleIdx = 0;
                    }
                }
                break;
            case GunType.Sniper:
                {
                    b = sniperPool[sniperIdx];
                    sniperIdx++;
                    if (sniperIdx >= poolSize)
                    {
                        sniperIdx = 0;
                    }
                }
                break;

        }
        b.gameObject.SetActive(false);
        return b;
    }

}