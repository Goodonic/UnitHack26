using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    private bool isOpen = false;

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("Клавиша TAB нажата!");
            ToggleInventory();
        }
    }
    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
    }
}
