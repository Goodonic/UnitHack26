using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoPanel : MonoBehaviour
{
    public static ItemInfoPanel Instance;

    [Header("UI Ссылки")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public RawImage previewImage;

    [Header("Настройки превью")]
    public Transform previewAnchor; // Точка перед камерой, где будет спавниться предмет
    private GameObject currentPreviewModel;

    void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentPreviewModel != null)
        {
            currentPreviewModel.transform.Rotate(Vector3.up, 20f * Time.deltaTime);
        }
    }

    public void ShowInfo(ItemData data)
    {
        gameObject.SetActive(true);
        titleText.text = data.itemName;
        descriptionText.text = data.description;

        UpdatePreview(data.itemPrefab);
    }
    void UpdatePreview(GameObject prefab)
    {
        if (currentPreviewModel != null) Destroy(currentPreviewModel);
        if (prefab == null) return;

        currentPreviewModel = Instantiate(prefab, previewAnchor.position, previewAnchor.rotation, previewAnchor);

        // --- НОВЫЙ КОД: Отключаем физику для превью ---
        // Отключаем Rigidbody, чтобы он не падал и не выдавал ошибок
        if (currentPreviewModel.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // Отключаем коллайдер, чтобы он не мешал основной сцене
        if (currentPreviewModel.TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }
        // ----------------------------------------------

        int layer = LayerMask.NameToLayer("ItemPreview");
        SetLayerRecursive(currentPreviewModel, layer);
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }
}
