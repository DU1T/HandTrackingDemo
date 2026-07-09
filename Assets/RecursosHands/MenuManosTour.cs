using UnityEngine;

public class MenuManosTour : MonoBehaviour
{
    public GameObject canvasMenu; //Canvas que necesitamos mostrar
    public GameObject reference; //Punto de anclaje a "orbitar"
    public float canvasOffset = 0.433f; //Distancia de punto de anclaje
    public bool canvasState = false;

    public void OnGesturePerformed()
    {
        if (!canvasState)
        {
            Debug.Log("Gesto de menu detectado");
            Vector3 forward = reference.transform.forward;
            Vector3 nuevaPos = reference.transform.position + forward * canvasOffset;
            Vector3 lookDirection = new Vector3(forward.x, 0, forward.z);
            canvasMenu.transform.position = new Vector3(nuevaPos.x, reference.transform.position.y, nuevaPos.z);
            canvasMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
            canvasMenu.SetActive(true);
            canvasState = true;
        }
    }

    public void ResetCanvasState() 
    {
        if (canvasState)
        {
            canvasState = false;
        }
    }
    public void OnGestureEnded() 
    {
        Debug.Log("Gesto de menu terminado");
    }
}
