using UnityEngine;
using System;
using UnityEngine.InputSystem;
public class ArmyManager : MonoBehaviour
{
    public int armyStrength = 1;
    public int maxVisibleAvatars = 11;
    private int[] powerTiers = new int[] { 1000, 100, 10, 1 };

    public Avatar[] avatars;  // 10 max

    void Start()
    {
        UpdateAvatarStrength();
    }
    void Update()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            armyStrength++;
            UpdateAvatarStrength();
        }
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            armyStrength += 10;
            UpdateAvatarStrength();
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            armyStrength += 100;
            UpdateAvatarStrength();
        }
    }
    void OnEnable()
    {
        GlobalEvents.OnArmyChange += RefreshVisuals;
    }

    void OnDisable()
    {
        GlobalEvents.OnArmyChange -= RefreshVisuals;
    }

    private void RefreshVisuals()
    {
        Debug.Log("Army visuals updated!");
        UpdateAvatarStrength();
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
                avatars[avatarIndex].SetTier(GetTierForPower(power));

                remaining -= power;
                avatarIndex++;
            }
        }
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

public static class GlobalEvents
{
    public static event Action OnArmyChange;

    public static void ArmyChange()
    {
        OnArmyChange?.Invoke();
    }
}