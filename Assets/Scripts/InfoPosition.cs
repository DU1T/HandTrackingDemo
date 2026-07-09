using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class InfoPosition : MonoBehaviour
{
    public GameObject canvasMenu;
    public GameObject referencePoint;
    public float canvasOffset = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(AlignRotationRoutine());
    }

    private IEnumerator AlignRotationRoutine() 
    {
        yield return new WaitForSeconds(0.1f);
        PositionCanvasInFrontOfHMD();
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
