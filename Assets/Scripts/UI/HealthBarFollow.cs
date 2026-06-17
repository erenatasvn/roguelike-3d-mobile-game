using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform target; // takip edilecek karakter
    public Vector3 offset = new Vector3(0f, -0.8f, 0f); // ayak hizasi
    public float pushTowardsCamera = 1.5f; // govdeyle cakismasin diye kameraya dogru kaydir

    void LateUpdate()
    {
        if (target != null && Camera.main != null)
        {
            // Karakterin ayaklarinin konumunu bul
            Vector3 basePosition = target.position + offset;

            // Kameraya olan yon hesaplama
            Vector3 directionToCamera = (Camera.main.transform.position - basePosition).normalized;

            // Bari karakterin govdesiyle cakismamasi icin kameraya dogru yaklastir
            transform.position = basePosition + (directionToCamera * pushTowardsCamera);

            // Barin her zaman kameraya bakmasini sagla
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}