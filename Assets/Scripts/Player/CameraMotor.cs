using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float rotationDuration = 0.1f;
    [SerializeField] private float cameraHeight = 1.7f;
    [SerializeField] private float fieldOfView = 80;
    
    private Camera playerCamera;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private float rotationProgress = 0f;
    private Quaternion startRotation;

    public bool IsRotating => isRotating;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = gameObject.AddComponent<Camera>();
        }

        int previewLayer = LayerMask.NameToLayer("ItemPreview");
        if (previewLayer != -1)
        {
            playerCamera.cullingMask &= ~(1 << previewLayer);
        }

        playerCamera.fieldOfView = fieldOfView;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (isRotating)
        {
            rotationProgress += Time.deltaTime / rotationDuration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);
            
            if (rotationProgress >= 1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }
    
    public void RotateLeft()
    {
        if (isRotating)
        {
            return;
        }

        float currentTargetYaw = targetRotation.eulerAngles.y;
        StartRotation(currentTargetYaw - 90f);
    }
    
    public void RotateRight()
    {
        if (isRotating)
        {
            return;
        }

        float currentTargetYaw = targetRotation.eulerAngles.y;
        StartRotation(currentTargetYaw + 90f);
    }
    
    private void StartRotation(float targetYaw)
    {
        if (isRotating) return;
        
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, targetYaw, 0);
        rotationProgress = 0f;
        isRotating = true;
    }
    
    public void SetRotation(Direction direction)
    {
        float yaw = direction switch
        {
            Direction.North => 0,
            Direction.East => 90,
            Direction.South => 180,
            Direction.West => 270,
            _ => 0
        };
        
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        targetRotation = transform.rotation;
        isRotating = false;
    }
}