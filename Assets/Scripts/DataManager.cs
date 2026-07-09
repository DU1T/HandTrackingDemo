using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Usaremos un int para almacenar un "ID" que indica qué escena/botón nos trajo aquí.
    // 0 = Sin origen específico (ej. cargado desde el editor o inicio por defecto)
    // 1 = Botón de la Escena Principal (Main Menu)
    // 2 = Botón de la Escena Secundaria (Level Select)
    public static int ID_Origen_Carga_Escena = 0;

    public static DataManager Instance; // Patrón Singleton para acceso fácil

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
