using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class ArmyUI : MonoBehaviour
{
    public Manager manager; // drag reference in Inspector

    private Button left;
    private Button right;
    private Button swap;

    void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            manager.Left();

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            manager.Right();

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            manager.Swap();
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        left = root.Q<Button>("Left");
        right = root.Q<Button>("Right");
        swap = root.Q<Button>("Swap");

        left.clicked += OnLeftClicked;
        right.clicked += OnRightClicked;
        swap.clicked += OnSwapClicked;
    }

    void OnDisable()
    {
        // Check if the UIDocument's root is still available before trying to unsubscribe
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        if (left != null) left.clicked -= OnLeftClicked;
        if (right != null) right.clicked -= OnRightClicked;
        if (swap != null) swap.clicked -= OnSwapClicked;
    }

    private void OnLeftClicked() => manager.Left();
    private void OnRightClicked() => manager.Right();
    private void OnSwapClicked() => manager.Swap();
}