using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuPrincipal;
    public GameObject panelSelectorNivel;
    public GameObject panelOpciones;

    // Mostrar el selector de nivel
    public void MostrarSelectorNivel()
    {
        panelMenuPrincipal.SetActive(false);
        panelSelectorNivel.SetActive(true);
        panelOpciones.SetActive(false);
    }

    // Mostrar las opciones
    public void MostrarOpciones()
    {
        panelMenuPrincipal.SetActive(false);
        panelSelectorNivel.SetActive(false);
        panelOpciones.SetActive(true);
    }

    // Volver al menú principal
    public void VolverAlMenu()
    {
        panelMenuPrincipal.SetActive(true);
        panelSelectorNivel.SetActive(false);
        panelOpciones.SetActive(false);
    }

    // Salir del juego
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
