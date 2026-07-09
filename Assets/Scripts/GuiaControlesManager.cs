using UnityEngine;
using System.Collections;

public class GuiaControlesManager : MonoBehaviour
{
    public GameObject canvasMenu;
    public GameObject referencePoint;
    public float canvasOffset = 4f;

    // Define qué ID específico debe tener la escena de origen para activar este canvas
    public int ID_Requerido_Para_Mostrar_Canvas = 1; // DEBE COINCIDIR CON EL BOTÓN DEL PASO 2

    void OnEnable()
    {
        // Verificamos si el ID actual del DataManager coincide con el ID que requerimos (ej: 1)
        if (DataManager.ID_Origen_Carga_Escena == ID_Requerido_Para_Mostrar_Canvas)
        {
            Debug.Log("Condición cumplida. El canvas se mostrará.");
            StartCoroutine(PositionCanvasAfterDelay());
        }
        else
        {
            // Ocultar canvas si no cumple la condición
            if (canvasMenu != null)
            {
                canvasMenu.SetActive(false); // Asegúrate de que está inactivo
            }
            Debug.LogWarning("Canvas oculto. ID de origen no coincide con el requerido.");
        }

        // MUY IMPORTANTE: Reiniciar el ID a 0 después de usarlo.
        DataManager.ID_Origen_Carga_Escena = 0;
    }

    // ... Tus métodos para posicionar el Canvas ...

    private IEnumerator PositionCanvasAfterDelay()
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
