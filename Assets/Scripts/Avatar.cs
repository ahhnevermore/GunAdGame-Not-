using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Avatar : MonoBehaviour
{
    public List<GameObject> guns;   // Drag guns into this from Inspector

    [HideInInspector]
    public GunType activeGun = GunType.Pistol;
    public Tier gunTier = Tier.Normal;
    public Tier selfTier = Tier.Normal;
    public int power = 0;
    public float nextFireTime = 0f;

    void Start()
    {
        SwitchTo(GunType.Pistol);
    }

    public float SwitchTo(GunType gunType)
    {
        activeGun = gunType;
        float fireRate = 0f;
        switch (gunType)
        {
            case GunType.Pistol:
                {
                    SetActiveGun(0);
                    fireRate = 2.5f;
                }
                break;
            case GunType.Rifle:
                {
                    SetActiveGun(1);
                    fireRate = 5f;

                }
                break;

            case GunType.Sniper:
                {
                    SetActiveGun(2);
                    fireRate = 1.4f;
                }
                break;
        }
        return fireRate;
    }

    void SetActiveGun(int index)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].SetActive(i == index);
        }
    }


    public void SetPower(int p)
    {
        power = p;
        selfTier = GetTierForPower(p);
    }
    
    public Tier GetTierForPower(int power)
    {
        if (power < 10)
            return Tier.Normal;
        if (power < 100)
            return Tier.Ice;
        if (power < 1000)
            return Tier.Arcane;

        return Tier.Legendary;
    }
}
