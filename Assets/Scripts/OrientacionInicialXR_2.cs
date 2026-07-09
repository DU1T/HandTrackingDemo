using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;


public class OrientacionInicialXR_2 : MonoBehaviour
{
    [System.Serializable]
    public struct ConfiguracionDireccion
    {
        public int ID_Origen; // El ID que viene del DataManager (6, 8, etc.)
        public Transform transformObjetivo; // El objeto al que debe mirar
    }

    [Header("Configuración XR")]
    public XROrigin xrOrigin;

    [Header("Lista de Direcciones por ID")]
    public List<ConfiguracionDireccion> listaDeDirecciones;

    [Header("Ajuste por Defecto")]
    public Transform direccionPorDefecto;

    void Start()
    {
        if (xrOrigin == null) xrOrigin = GetComponent<XROrigin>();

        StartCoroutine(AlignRotationRoutine());
    }

    private IEnumerator AlignRotationRoutine()
    {
        // 0.2s es ideal para que el hardware de VR no ignore el cambio
        yield return null;

        // 1. Obtenemos el ID que guardó el CargadorDeEscena
        int idActual = DataManager.ID_Origen_Carga_Escena;

        // 2. Buscamos en nuestra lista si ese ID tiene un objeto asignado
        Transform target = direccionPorDefecto;

        foreach (var config in listaDeDirecciones)
        {
            if (config.ID_Origen == idActual)
            {
                target = config.transformObjetivo;
                break;
            }
        }

        // 3. Aplicamos la rotación
        if (xrOrigin != null && target != null)
        {
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, target.forward);
            Debug.Log($"Orientación exitosa. ID Origen: {idActual}, Mirando a: {target.name}");
        }
    }
}
