using UnityEngine;
using UnityEngine.SceneManagement;

public class CargadorDeEscenaConID : MonoBehaviour
{
    // Define en el Inspector qué ID debe enviar este botón (ej: 1, 2, 3...)
    public int ID_Unico_De_Este_Boton;
    // Define a qué escena debe ir este botón
    public string NombreDeLaEscenaDestino;

    // Asigna esta función al evento OnClick() del botón en el Inspector
    public void CargarEscenaDestino()
    {
        // 1. Marcar el origen en el DataManager
        DataManager.ID_Origen_Carga_Escena = ID_Unico_De_Este_Boton;

        // 2. Cargar la siguiente escena
        SceneManager.LoadScene(NombreDeLaEscenaDestino);
    }
}
