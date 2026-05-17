using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 189f;
    [SerializeField] private float lightRange = 9f;
    

    private Light playerLightComponent;

    void Start()
    {
        playerLightComponent = GetComponentInChildren<Light>();
        if (playerLightComponent == null)
        {
            GameObject lightObject = new GameObject("Player Light");

            playerLightComponent = lightObject.AddComponent<Light>();
            playerLightComponent.type = LightType.Point;
        }

        playerLightComponent.shadows = LightShadows.Soft;
        playerLightComponent.bounceIntensity = 0;
        playerLightComponent.color = new Color(1f,0,1f, 1f);
        playerLightComponent.intensity = lightIntensity;
        playerLightComponent.range = lightRange;
        
        UpdateLightPosition();
    }

    private void LateUpdate()
    {
        UpdateLightPosition();
    }

    private void UpdateLightPosition()
    {
        if (playerLightComponent == null)
        {
            return;
        }

        playerLightComponent.transform.position = transform.position;
    }
}
