using UnityEngine;
using UnityEngine.SceneManagement;

public class CambarLvl : MonoBehaviour
{
    // Cambiar de nivel usando el nombre de la escena
    public void CargarNivel(string nombreNivel)
    {
        Debug.Log("[DEBUG] ¡Se hizo clic en el botón de nivel!");
        Debug.Log("[DEBUG] Nivel solicitado: " + nombreNivel);

        // Comprobar si la escena existe en la compilación
        int indice = SceneUtility.GetBuildIndexByScenePath(nombreNivel);

        if (indice == -1)
        {
            Debug.LogWarning(
                "[DEBUG] No se encontró la escena por ruta: " + nombreNivel +
                ". Comprueba el nombre y que esté incluida en Build Profiles."
            );
        }

        SceneManager.LoadScene(nombreNivel);
    }

    // Volver al menú principal
    public void VolverAlMenu()
    {
        Debug.Log("[DEBUG] Clic en Volver al menú principal");
        SceneManager.LoadScene("Menu");
    }

    // Cerrar el juego
    public void SalirDelJuego()
    {
        Debug.Log("[DEBUG] Clic en Salir del juego");
        Application.Quit();
    }
}