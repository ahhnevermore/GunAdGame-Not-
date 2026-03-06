using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class ArmyUI : MonoBehaviour
{
    public ArmyManager armyManager; // drag reference in Inspector

    private Button left;
    private Button right;
    private Button swap;

    void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            armyManager.Left();

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            armyManager.Right();

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            armyManager.Swap();
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        left = root.Q<Button>("Left");
        right = root.Q<Button>("Right");
        swap = root.Q<Button>("Swap");

        left.clicked += () => armyManager.Left();
        right.clicked += () => armyManager.Right();
        swap.clicked += () => armyManager.Swap();
    }
}