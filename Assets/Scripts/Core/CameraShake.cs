using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        // Sahne yeniden yüklendiğin yeni instance geçerli olsun
        Instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // magnitude: sarsıntının gücü 
    public void Shake(float duration, float magnitude)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(magnitude);
        }
        else
        {
            Debug.LogWarning("CameraShake: CinemachineImpulseSource bulunamadı");
        }
    }
}
