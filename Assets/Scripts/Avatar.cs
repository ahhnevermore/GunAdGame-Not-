using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Avatar : MonoBehaviour
{
    public List<GameObject> guns;   // Drag guns into this from Inspector
    public BulletPool bulletPool;

    private GunType activeGun = GunType.Pistol;
    public Tier gunTier = Tier.Normal;
    public Tier selfTier = Tier.Normal;

    [SerializeField] Animator anim;



    void Start()
    {
        SwitchTo(GunType.Pistol);
    }
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            activeGun++;
            if ((int)activeGun > 2)
            {
                activeGun = (GunType)0;
            }
            SwitchTo(activeGun);
        }
    }

    public void SwitchTo(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.Pistol:
                {
                    SetActiveGun(0);
                    anim.SetFloat("shootSpeed", 0.7f);
                }
                break;
            case GunType.Rifle:
                {
                    SetActiveGun(1);
                    anim.SetFloat("shootSpeed", 1.2f);
                }
                break;

            case GunType.Sniper:
                {
                    SetActiveGun(2);
                    anim.SetFloat("shootSpeed", 0.4f);
                }
                break;
        }
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
        b.SetupBullet(gunTier, selfTier, new Vector3(transform.position.x, 4f, transform.position.z + 2),
        (int)(Math.Pow(10f, (double)selfTier) * (double)(1 + gunTier) * Tiers.GunPower[activeGun]));
    }

    public void SetTier(Tier tier)
    {
        selfTier = tier;
        Color c = Tiers.UTColours[tier][0];
    }
}
