using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Avatar : MonoBehaviour
{
    public List<GameObject> guns;   // Drag guns into this from Inspector
    public BulletPool bulletPool;
    private int currentIndex = 0;
    private GunType activeGun = GunType.Pistol;
    private Tier gunTier = Tier.Normal;
    private Tier selfTier = Tier.Normal;


    void Start()
    {
        SetActiveGun(currentIndex);
    }
    void Update()
    {

    }

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= guns.Count) return;
        currentIndex = index;
        SetActiveGun(index);
    }

    void SetActiveGun(int index)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].SetActive(i == index);
        }
    }

    public void FireBullet()
    {
        Bullet b = bulletPool.GetNext(activeGun);
        b.SetupBullet(gunTier, selfTier, new Vector3(transform.position.x, 4f, transform.position.z + 2), (int)(Math.Pow(10f, (double)selfTier) * (double)(1 + gunTier)));
    }
}
