using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContextMenu : MonoBehaviour
{
    public static ContextMenu Instance;
    private ItemData currentItem;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(ItemData item, Vector2 position)
    {
        currentItem = item;
        gameObject.SetActive(true);
        // Перемещаем меню к курсору
        transform.position = position;
    }

    public void OnInspectClick()
    {
        Debug.Log("Описание: " + currentItem.description);
        // Здесь можно вывести текст в отдельное UI окно описания
        Close();
    }

    public void OnDropClick()
    {
        Debug.Log("Выбросили: " + currentItem.itemName);
        // Тут будет логика удаления из InventoryGrid и спавна 3D объекта
        Close();
    }

    public void Close() => gameObject.SetActive(false);
}
