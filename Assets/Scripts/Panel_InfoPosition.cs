using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Panel_InfoPosition : MonoBehaviour
{
    public GameObject canvasMenu;
    public GameObject referencePoint;
    public Button btn;
    public float canvasOffset = 4f;

    private void Start()
    {
        btn.onClick.AddListener(PositionCanvasInFrontOfHMD);
    }

    public void PositionCanvasInFrontOfHMD()
    {
        Vector3 forward = referencePoint.transform.forward;
        Vector3 nuevaPos = referencePoint.transform.position + forward * canvasOffset;
        Vector3 lookDirection = new Vector3(forward.x, 0, forward.z);
        canvasMenu.transform.position = new Vector3(nuevaPos.x, referencePoint.transform.position.y, nuevaPos.z);
        canvasMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
        canvasMenu.SetActive(true);
    }
}
