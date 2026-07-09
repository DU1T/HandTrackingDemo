using UnityEngine;
using Unity.XR.CoreUtils;

public class ResetXRCamera : MonoBehaviour
{
    private XROrigin xrOrigin;

    private void Awake()
    {
        xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.MatchOriginUp(Vector3.up);
           // xrOrigin.MatchOriginForward(Vector3.forward);
            xrOrigin.MoveCameraToWorldLocation(Vector3.zero);
        }
    }
}
