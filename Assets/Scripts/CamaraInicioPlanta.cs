using Unity.XR.CoreUtils;
using UnityEngine;
using System.Collections;
public class CamaraInicioPlanta : MonoBehaviour
{
    public GameObject canvasMenu;
    public Transform referencePoint; // Asignado a Main Camera en el Inspector
    public float canvasOffset = 4f;

    void OnEnable()
    {
        // En lugar de llamar a la función directamente, iniciamos una Coroutine
        StartCoroutine(PositionCanvasAfterDelay());
    }

    private IEnumerator PositionCanvasAfterDelay()
    {
        // Espera un fotograma. Esto es crucial para que Unity/XR actualice la cámara.
        yield return null;
        // Si un frame no es suficiente, puedes intentar yield return new WaitForSeconds(0.1f);

        PositionCanvasInFrontOfHMD();
    }

    public void PositionCanvasInFrontOfHMD()
    {
        if (referencePoint == null || canvasMenu == null)
        {
            Debug.LogError("Referencias nulas. Asegúrese de asignar Main Camera a Reference Point.");
            return;
        }

        // Leerá la posición y rotación actualizadas del HMD
        Vector3 forward = referencePoint.forward;
        Vector3 nuevaPos = referencePoint.position + forward * canvasOffset;

        // Mantiene la altura, pero rota solo horizontalmente
        Vector3 lookDirection = new Vector3(forward.x, 0, forward.z).normalized;

        canvasMenu.transform.position = new Vector3(nuevaPos.x, referencePoint.position.y, nuevaPos.z);

        if (lookDirection != Vector3.zero)
        {
            canvasMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        canvasMenu.SetActive(true);

        Debug.Log("Canvas posicionado con éxito. Dirección forward HMD: " + referencePoint.forward);
    }
}
