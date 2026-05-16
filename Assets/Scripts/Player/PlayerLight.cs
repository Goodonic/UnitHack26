using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 9.7f;
    [SerializeField] private float lightRange = 20f;
    
    
    private Light playerLightComponent;
    
    void Start()
    {
        playerLightComponent = GetComponentInChildren<Light>();
        if (playerLightComponent == null)
        {
            GameObject lightObject = new GameObject("Player Light");
            lightObject.transform.SetParent(transform);

            playerLightComponent = lightObject.AddComponent<Light>();
            playerLightComponent.type = LightType.Point;
            
            playerLightComponent.shadows = LightShadows.Soft;
            playerLightComponent.bounceIntensity = 0;
            playerLightComponent.color = new Color(0.921568632f, 0.717647076f, 0.13333334f);
            playerLightComponent.intensity = lightIntensity;
            playerLightComponent.range = lightRange;
        }
        else
        {
            playerLightComponent.transform.SetParent(transform);
        }

        
    }

    
}
