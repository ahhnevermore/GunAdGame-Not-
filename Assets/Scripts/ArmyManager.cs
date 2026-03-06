using UnityEngine;
using System;
using UnityEngine.InputSystem;
public class ArmyManager : MonoBehaviour
{
    
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
        GlobalEvents.OnAvatarHit += OnAvatarHitCB;
    }

    void OnDisable()
    {
        GlobalEvents.OnAvatarHit -= OnAvatarHitCB;
    }

    private void OnAvatarHitCB(int p)
    {
        armyStrength -= p;
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
                avatars[avatarIndex].SetPower(power);

                remaining -= power;
                avatarIndex++;
            }
        }
    }
    public void Left()
    {

    }
    public void Right()
    {

    }
    public void Swap() { }
}

public static class GlobalEvents
{
    public static event Action<int> OnAvatarHit;

    public static void AvatarHit(int value)
    {
        OnAvatarHit?.Invoke(value);
    }
}