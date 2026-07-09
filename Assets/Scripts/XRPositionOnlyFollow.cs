using UnityEngine;

public class XRPositionOnlyFollow : MonoBehaviour
{
    [Header("Referencias")]
    public Transform headTarget;

    [Header("Configuracion de Ilusion Optica")]
    public float originalCameraY = 1.2f;

    [Header("Suavizado")]
    //Entre  mayor smoothness, la posicion se corrige de manera mas rapida
    public float smoothness = 15f;

    private Vector3 _initialWorldOffset;

    void Start()
    {
        if (headTarget == null) headTarget = Camera.main.transform;

        if (headTarget != null)
        {
            //Calculo del offset ajustado:

            //Tomamos la posicion actual del objeto.
            Vector3 virtualCameraPos = headTarget.position;
            virtualCameraPos.y = originalCameraY; // Forzamos la Y al valor antiguo

            //En lugar de restar la posicion real de la camara (que varia),
            //restamos la posicion con la que se configuro en su origen.
            _initialWorldOffset = transform.position - virtualCameraPos;
        }
    }

    void LateUpdate()
    {
        if (headTarget == null) return;

        //La posicion objetivo ahora compensa la diferencia de altura actual del HMD
        //con respecto a la altura original de 1.2m
        Vector3 targetPosition = headTarget.position + _initialWorldOffset;

        //Aplicamos el movimiento
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothness);
    }
}
