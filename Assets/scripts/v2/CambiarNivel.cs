using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarNivel : MonoBehaviour
{
    // Cambiar de nivel usando el nombre de la escena
    public void CargarNivel(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    // Volver al menú principal
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Cerrar el juego
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
