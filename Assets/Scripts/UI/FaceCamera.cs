using UnityEngine;
public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
    }
}