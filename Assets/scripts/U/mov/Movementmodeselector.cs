using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Botones del menu para elegir el tipo de movimiento y de giro.
/// Autonomo: busca solo los componentes de locomocion del XR Origin, no hay que asignar providers.
///
/// Movimiento: Smooth Move / Teleport
/// Giro:       Smooth Turn / Snap Turn
/// </summary>
public class MovementModeSelector : MonoBehaviour
{
    [Header("Botones - Movimiento")]
    [SerializeField] private Button smoothMoveButton;
    [SerializeField] private Button teleportButton;

    [Header("Botones - Giro")]
    [SerializeField] private Button smoothTurnButton;
    [SerializeField] private Button snapTurnButton;

    [Header("Opciones")]
    [SerializeField] private bool defaultSmoothMove = true;
    [SerializeField] private bool defaultSmoothTurn = false;
    [SerializeField] private bool rememberChoice = true;

    [Header("Depuracion")]
    [Tooltip("Muestra en la Console los componentes de locomocion encontrados.")]
    [SerializeField] private bool listarComponentesAlIniciar = false;

    private const string MovePrefKey = "ajuste_movimiento_suave";
    private const string TurnPrefKey = "ajuste_giro_suave";

    public bool SmoothMove { get; private set; }
    public bool SmoothTurn { get; private set; }

    /// <summary>true = movimiento suave, false = teletransporte.</summary>
    public static event System.Action<bool> OnMoveChanged;
    /// <summary>true = giro suave, false = giro por pasos.</summary>
    public static event System.Action<bool> OnTurnChanged;

    private void Awake()
    {
        if (smoothMoveButton != null) smoothMoveButton.onClick.AddListener(() => SetMove(true));
        if (teleportButton != null)   teleportButton.onClick.AddListener(() => SetMove(false));
        if (smoothTurnButton != null) smoothTurnButton.onClick.AddListener(() => SetTurn(true));
        if (snapTurnButton != null)   snapTurnButton.onClick.AddListener(() => SetTurn(false));
    }

    private IEnumerator Start()
    {
        SmoothMove = defaultSmoothMove;
        SmoothTurn = defaultSmoothTurn;

        if (rememberChoice)
        {
            SmoothMove = PlayerPrefs.GetInt(MovePrefKey, defaultSmoothMove ? 1 : 0) == 1;
            SmoothTurn = PlayerPrefs.GetInt(TurnPrefKey, defaultSmoothTurn ? 1 : 0) == 1;
        }

        // Esperamos un frame para que el XR Origin termine de inicializarse
        yield return null;

        if (listarComponentesAlIniciar) Listar();

        Aplicar();
        UpdateButtonVisuals();
    }

    public void SetMove(bool suave)
    {
        SmoothMove = suave;
        if (rememberChoice) { PlayerPrefs.SetInt(MovePrefKey, suave ? 1 : 0); PlayerPrefs.Save(); }
        Aplicar();
        UpdateButtonVisuals();
        OnMoveChanged?.Invoke(suave);
    }

    public void SetTurn(bool suave)
    {
        SmoothTurn = suave;
        if (rememberChoice) { PlayerPrefs.SetInt(TurnPrefKey, suave ? 1 : 0); PlayerPrefs.Save(); }
        Aplicar();
        UpdateButtonVisuals();
        OnTurnChanged?.Invoke(suave);
    }

    // Busca los componentes de locomocion por nombre de tipo y los enciende/apaga.
    private void Aplicar()
    {
        int managers = 0, snap = 0, giroCont = 0, movimiento = 0, teleport = 0;

        MonoBehaviour[] todos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour m in todos)
        {
            if (m == null || m == this) continue;
            string tipo = m.GetType().Name;

            // 1) Starter Assets: el controlador de cada mano trae las opciones ya hechas
            //    (tambien maneja el rayo de teletransporte)
            if (tipo == "ControllerInputActionManager" || tipo == "ActionBasedControllerManager")
            {
                bool a = PonerBool(m, "smoothMotionEnabled", SmoothMove);
                bool b = PonerBool(m, "smoothTurnEnabled", SmoothTurn);
                if (a || b) managers++;
            }
            // 2) Ademas se encienden o apagan directamente los providers
            else if (tipo == "SnapTurnProvider" || tipo == "ActionBasedSnapTurnProvider")
            { m.enabled = !SmoothTurn; snap++; }
            else if (tipo == "ContinuousTurnProvider" || tipo == "ActionBasedContinuousTurnProvider")
            { m.enabled = SmoothTurn; giroCont++; }
            else if (tipo == "TeleportationProvider")
            { m.enabled = !SmoothMove; teleport++; }
            else if (tipo.EndsWith("MoveProvider") && !tipo.StartsWith("Grab"))
            { m.enabled = SmoothMove; movimiento++; }
        }

        Debug.Log("[MovementModeSelector] " + (SmoothMove ? "Movimiento suave" : "Teletransporte")
                  + " + " + (SmoothTurn ? "giro suave" : "giro por pasos")
                  + " | encontrados -> controladores: " + managers + ", giro snap: " + snap
                  + ", giro suave: " + giroCont + ", mover: " + movimiento + ", teletransporte: " + teleport);

        if (!SmoothMove && teleport == 0)
            Debug.LogWarning("[MovementModeSelector] Pediste teletransporte, pero no hay ningun TeleportationProvider en la escena.");
        if (snap == 0 && giroCont == 0 && managers == 0)
            Debug.LogWarning("[MovementModeSelector] No encontre ningun componente de giro. Revisa la locomocion del XR Origin.");
    }

    // Para ver en la Console que componentes de locomocion tiene tu rig
    private void Listar()
    {
        foreach (MonoBehaviour m in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            string tipo = m.GetType().Name;
            if (tipo.Contains("Provider") || tipo.Contains("Manager") || tipo.Contains("Teleport") || tipo.Contains("Locomotion"))
                Debug.Log("[LOCOMOCION] " + tipo + " en '" + m.gameObject.name + "' (activo: " + m.enabled + ")");
        }
    }

    // Cambia una propiedad bool por nombre (sin depender de la version del paquete XRI)
    private static bool PonerBool(object objeto, string nombre, bool valor)
    {
        PropertyInfo prop = objeto.GetType().GetProperty(
            nombre, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop != null && prop.CanWrite && prop.PropertyType == typeof(bool))
        {
            prop.SetValue(objeto, valor);
            return true;
        }
        return false;
    }

    // El boton elegido se deshabilita para que se vea distinto (color "Disabled" del Button).
    private void UpdateButtonVisuals()
    {
        if (smoothMoveButton != null) smoothMoveButton.interactable = !SmoothMove;
        if (teleportButton != null)   teleportButton.interactable   = SmoothMove;
        if (smoothTurnButton != null) smoothTurnButton.interactable = !SmoothTurn;
        if (snapTurnButton != null)   snapTurnButton.interactable   = SmoothTurn;
    }
}