using Unity.XR.CoreUtils;
using UnityEngine;
using System.Collections;

public class OrientacionInicialXR : MonoBehaviour

{

    [Header("Configuración XR")]

    public XROrigin xrOrigin;



    [Header("Objetivo")]

    [Tooltip("El usuario mirará hacia la dirección 'Forward' (flecha azul) de este transform.")]

    public Transform targetDirection;



    void Start()

    {

        if (xrOrigin == null)

            xrOrigin = GetComponent<XROrigin>();



        if (xrOrigin != null && targetDirection != null)

        {

            StartCoroutine(AlignRotationRoutine());

        }

        else

        {

            Debug.LogWarning("Faltan referencias en OrientacionInicialXR.");

        }

    }



    private IEnumerator AlignRotationRoutine()

    {

        // Esperar un breve momento para que el tracking del HMD se estabilice

        // 0.2 segundos suelen ser suficientes para evitar que el hardware sobrescriba el cambio.

        yield return null;



        // Este método rota el XR Origin para que la cámara mire hacia el targetDirection.forward

        // manteniendo el eje vertical (Vector3.up).

        xrOrigin.MatchOriginUpCameraForward(Vector3.up, targetDirection.forward);



        Debug.Log("Orientación de cámara sincronizada con: " + targetDirection.name);

    }

}