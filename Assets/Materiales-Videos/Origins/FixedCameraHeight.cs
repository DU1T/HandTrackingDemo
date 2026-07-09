using UnityEngine;

public class FixedCameraHeight : MonoBehaviour
{
    public Transform cameraOffset;
    public float fixedHeight = 1.36f;

    void LateUpdate()
    {
        Vector3 pos = cameraOffset.localPosition;
        pos.y = fixedHeight;
        cameraOffset.localPosition = pos;
    }
}
