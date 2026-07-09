using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Necesario para acceder al componente Image/Graphic

public class SceneTransitionManager : MonoBehaviour
{
    public Image transitionImage; // Asigna tu panel UI aquí desde el Inspector
    public float transitionTime = 1.5f; // Duración de la transición
    private Material transitionMaterial;

    void Start()
    {
        // Obtiene una instancia del material para poder modificar sus propiedades
        transitionMaterial = transitionImage.material;

        // Inicia la transición de entrada (fade in al cargar la escena)
        StartCoroutine(RippleIn());
    }

    public void LoadNextScene(string sceneName)
    {
        StartCoroutine(RippleOutAndLoad(sceneName));
    }

    IEnumerator RippleOutAndLoad(string sceneName)
    {
        float timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            // Anima el radio de 0 a 1.1 (asegurando que cubra toda la pantalla)
            transitionMaterial.SetFloat("_Radius", Mathf.Lerp(0f, 1.1f, timer / transitionTime));
            yield return null;
        }

        // Asegura que el radio sea completo antes de cargar
        transitionMaterial.SetFloat("_Radius", 1.1f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator RippleIn()
    {
        float timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            // Anima el radio de 1.1 a 0 (revelando la escena)
            transitionMaterial.SetFloat("_Radius", Mathf.Lerp(1.1f, 0f, timer / transitionTime));
            yield return null;
        }
        transitionMaterial.SetFloat("_Radius", 0f);
        // Opcional: desactiva el panel después de la transición de entrada
         transitionImage.gameObject.SetActive(false); 
    }
}