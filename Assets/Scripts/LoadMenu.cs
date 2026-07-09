using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadMenu : MonoBehaviour
{
    public InputActionReference menuLoaderAction; //Referencia de input action (crear nuevo input action)
    public GameObject canvasMenu; //Canvas que necesitamos mostrar
    public GameObject reference; //Punto de anclaje a "orbitar"
    public float canvasOffset = 0.433f; //Distancia de punto de anclaje

    //Funcion que habilita el metodo OnMenuPressed (listener)
    private void OnEnable()
    {
        menuLoaderAction.action.performed += OnMenuPressed;
        menuLoaderAction.action.Enable();
    }
    //Fucion que termina el listener
    private void OnDisable()
    {
        menuLoaderAction.action.performed -= OnMenuPressed;
        menuLoaderAction.action.Disable();
    }
    //Funcion que habilita el canvas, modificando su posicion y rotacion siguiendo el "ancla"
    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        Vector3 forward = reference.transform.forward;
        Vector3 nuevaPos = reference.transform.position + forward * canvasOffset;
        Vector3 lookDirection = new Vector3(forward.x, 0, forward.z);
        canvasMenu.transform.position = new Vector3(nuevaPos.x, reference.transform.position.y, nuevaPos.z);
        canvasMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
        canvasMenu.SetActive(true);

    }
}
