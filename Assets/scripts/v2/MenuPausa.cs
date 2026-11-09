using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPausa;
    [Tooltip("Objeto del selector de movimiento (movSelect). Opcional.")]
    public GameObject panelMovimiento;

    [Header("Botones del panel de pausa")]
    public Button botonReanudar;     // "Button"
    public Button botonMovimiento;   // "mov"
    public Button botonSalir;        // "Exit_game"
    public Button botonReiniciar;    // opcional
    [Tooltip("Botón para volver desde el panel de movimiento. Opcional.")]
    public Button botonVolverDeMovimiento;

    [Header("VR")]
    [Tooltip("Acción del botón Menu del control (XRI Left/Menu o similar).")]
    public InputActionReference accionPausa;
    [Tooltip("Normalmente la Main Camera del XR Origin.")]
    public Transform camaraJugador;
    public float distanciaAlJugador = 1.5f;

    [Header("Salir")]
    public string escenaMenu = "Menu";
    [Tooltip("Si está activo, el botón Salir cierra la aplicación en vez de ir al menú.")]
    public bool salirCierraApp = false;

    private bool juegoPausado = false;

    void Awake()
    {
        if (botonReanudar != null) botonReanudar.onClick.AddListener(ReanudarJuego);
        if (botonMovimiento != null) botonMovimiento.onClick.AddListener(AbrirPanelMovimiento);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
        if (botonReiniciar != null) botonReiniciar.onClick.AddListener(ReiniciarNivel);
        if (botonVolverDeMovimiento != null) botonVolverDeMovimiento.onClick.AddListener(VolverDePanelMovimiento);
    }

    void OnEnable()
    {
        if (accionPausa != null)
        {
            accionPausa.action.Enable();
            accionPausa.action.performed += OnPausaPresionada;
        }
    }

    void OnDisable()
    {
        if (accionPausa != null)
            accionPausa.action.performed -= OnPausaPresionada;
    }

    void Start()
    {
        panelPausa.SetActive(false);
        if (panelMovimiento != null) panelMovimiento.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPausaPresionada(InputAction.CallbackContext ctx)
    {
        if (juegoPausado) ReanudarJuego();
        else PausarJuego();
    }

    public void PausarJuego()
    {
        ColocarPanelFrenteAlJugador(panelPausa.transform);
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        panelPausa.SetActive(false);
        if (panelMovimiento != null) panelMovimiento.SetActive(false);
        Time.timeScale = 1f;
        juegoPausado = false;
    }

    public void AbrirPanelMovimiento()
    {
        if (panelMovimiento == null) return;
        ColocarPanelFrenteAlJugador(panelMovimiento.transform);
        panelPausa.SetActive(false);
        panelMovimiento.SetActive(true);
    }

    public void VolverDePanelMovimiento()
    {
        if (panelMovimiento != null) panelMovimiento.SetActive(false);
        panelPausa.SetActive(true);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Salir()
    {
        Time.timeScale = 1f;

        if (salirCierraApp)
        {
            Application.Quit();
            return;
        }

        SceneManager.LoadScene(escenaMenu);
    }

    public bool EstaPausado()
    {
        return juegoPausado;
    }

    // Coloca el panel (Canvas en World Space) delante de la mirada, mirando al jugador.
    private void ColocarPanelFrenteAlJugador(Transform panel)
    {
        if (camaraJugador == null) return;

        Vector3 adelante = camaraJugador.forward;
        adelante.y = 0f;
        if (adelante.sqrMagnitude < 0.001f) adelante = Vector3.forward;
        adelante.Normalize();

        panel.position = camaraJugador.position + adelante * distanciaAlJugador;
        panel.rotation = Quaternion.LookRotation(panel.position - camaraJugador.position);
    }
}