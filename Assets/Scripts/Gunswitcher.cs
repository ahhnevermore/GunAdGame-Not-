using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    public List<GameObject> guns;   // Drag guns into this from Inspector
    private int currentIndex = 0;

    void Start()
    {
        SetActiveGun(currentIndex);
    }
    void Update()
    {

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchTo(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchTo(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchTo(2);
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
}
