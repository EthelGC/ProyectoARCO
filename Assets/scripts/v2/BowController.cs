using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;


/// <summary>
/// Va en la raíz del prefab "Wooden Bow".
/// - El arco se sostiene con la mano izquierda (se configura con Interaction Layers).
/// - Con la mano derecha: mantén el gatillo, jala hacia atrás y suelta para disparar.
/// - La animación avanza según qué tanto jalas (parámetro float "Pull" 0..1).
/// - El control derecho vibra cada vez más fuerte conforme jalas, y al máximo en el límite.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class BowController : MonoBehaviour
{
    [Header("Input (gatillo derecho)")]
    [Tooltip("XRI RightHand Interaction / Activate")]
    [SerializeField] private InputActionReference drawAction;

    [Header("Mano derecha (la que jala la cuerda)")]
    [Tooltip("Transform del control derecho. Si lo dejas vacío, se busca por nombre.")]
    [SerializeField] private Transform rightHand;
    [SerializeField] private string rightHandName = "Right Controller";

    [Header("Jalón")]
    [Tooltip("Distancia en metros que hay que mover la mano para el jalón máximo (controla la animación de la cuerda). Más allá de esto ya no cuenta.")]
    [SerializeField] private float maxPullDistance = 0.45f;
    [Tooltip("Jalón mínimo (0 a 1) para que el disparo cuente")]
    [SerializeField] private float minPullToFire = 0.1f;
    [Tooltip("Resistencia al empezar a jalar. 1 = normal (sigue a tu mano). 2 = pesado al inicio y más ligero al final")]
    [SerializeField, Range(1f, 3f)] private float pullResistance = 1f;

    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Empty hijo del arco. Su eje Z azul apunta hacia donde sale el proyectil")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float minSpeed = 8f;
    [SerializeField] private float maxSpeed = 45f;
    [SerializeField] private float projectileLifetime = 15f;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [Tooltip("Parámetro Float del Animator (Motion Time)")]
    [SerializeField] private string pullParameter = "Pull";
    [Tooltip("Hasta qué punto de la animación llega al jalar al máximo (0 a 1). Si la cuerda se regresa al final del clip, bájalo hasta el punto donde se ve más tensada")]
    [SerializeField, Range(0.1f, 0.999f)] private float animMaxTime = 0.999f;
    [Tooltip("Qué tan rápido regresa la cuerda al soltar")]
    [SerializeField] private float returnSpeed = 8f;

    [Header("Prueba de animación (solo para encontrar el punto máximo)")]
    [Tooltip("Actívalo en Play y mueve Test Value para ver la animación a mano")]
    [SerializeField] private bool testAnimation = false;
    [SerializeField, Range(0f, 1f)] private float testValue = 0f;

    [Header("Flecha puesta en el arco (solo visual)")]
    [Tooltip("Copia decorativa de la flecha, hija del FirePoint. Sin Rigidbody ni Collider.")]
    [SerializeField] private Transform nockedArrow;
    [Tooltip("Punto de referencia: la COLA de la flecha en reposo (un Empty fijo, hijo del arco, NO de la flecha). Desde aquí se mide el jalón. Si lo dejas vacío usa el FirePoint")]
    [SerializeField] private Transform nockPoint;
    [Tooltip("Qué tan cerca debe estar tu mano derecha de la cola de la flecha para poder agarrarla (metros). 0 = sin restricción")]
    [SerializeField] private float nockGrabRadius = 0.35f;
    [Tooltip("Metros que retrocede SOLO la flecha del arco al jalar al máximo. Ajústalo hasta que coincida con la cuerda")]
    [SerializeField] private float nockedTravel = 0.45f;

    [Header("Vibración (control derecho)")]
    [Tooltip("Fuerza de la vibración al empezar a jalar")]
    [SerializeField] private float pullHapticMin = 0.05f;
    [Tooltip("Fuerza de la vibración casi en el límite")]
    [SerializeField] private float pullHapticMax = 0.7f;
    [Tooltip("Fuerza cuando ya llegaste al límite")]
    [SerializeField] private float limitHapticAmplitude = 1f;
    [SerializeField] private float hapticInterval = 0.06f;
    [SerializeField] private float releaseHapticAmplitude = 0.3f;
    [SerializeField] private float releaseHapticDuration = 0.1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Collider[] bowColliders;
    private bool drawing;
    private bool wasPressed;
    private float startOffset;
    private float nextHapticTime;
    private float pull;      // jalón actual 0..1
    private float animPull;  // valor que ve el Animator
    private Vector3 nockedRestLocal;
    private bool nockedHidden;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        bowColliders = GetComponentsInChildren<Collider>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (nockedArrow != null) nockedRestLocal = nockedArrow.localPosition;

        if (rightHand == null)
        {
            GameObject go = GameObject.Find(rightHandName);
            if (go != null) rightHand = go.transform;
            else Debug.LogWarning("BowController: no encontré la mano derecha. Asígnala en el Inspector o revisa 'Right Hand Name'.");
        }
    }

    private void OnEnable()
    {
        if (drawAction != null) drawAction.action.Enable();
    }

    // LateUpdate: corre DESPUÉS de que el arco y los controles ya se movieron en este frame
    private void LateUpdate()
    {
        bool held = grab.isSelected;
        bool pressed = drawAction != null && drawAction.action.ReadValue<float>() > 0.5f;

        if (!held)
        {
            // Si se suelta el arco a mitad del jalón, se cancela
            drawing = false;
        }
        else
        {
            if (pressed && !wasPressed) StartDraw();

            if (drawing)
            {
                if (pressed) UpdateDraw();
                else ReleaseDraw();
            }
        }

        wasPressed = pressed;

        // Animación: sigue el jalón mientras jalas, regresa suave cuando no
        if (drawing) animPull = pull;
        else animPull = Mathf.MoveTowards(animPull, 0f, returnSpeed * Time.deltaTime);

        if (animator != null)
        {
            float value = testAnimation ? testValue : Mathf.Clamp01(animPull) * animMaxTime;
            animator.SetFloat(pullParameter, value);
            animator.Update(0f); // aplica la pose de inmediato para que cuerda y flecha vayan sincronizadas
        }

        UpdateNockedArrow();
    }

    // La flecha del arco sigue a la cuerda; al disparar desaparece y reaparece cuando la cuerda regresa
    private void UpdateNockedArrow()
    {
        if (nockedArrow == null) return;

        if (nockedHidden && !drawing && animPull <= 0.02f) nockedHidden = false;

        if (nockedArrow.gameObject.activeSelf == nockedHidden)
            nockedArrow.gameObject.SetActive(!nockedHidden);
        if (nockedHidden) return;

        Transform p = nockedArrow.parent;
        // Desplazamiento en metros reales, convertido a la escala del padre
        Vector3 worldOffset = -firePoint.forward * (animPull * nockedTravel);
        Vector3 localOffset = p != null ? p.InverseTransformVector(worldOffset) : worldOffset;
        nockedArrow.localPosition = nockedRestLocal + localOffset;
    }

    // Distancia (metros) de la mano hacia atrás del punto de disparo
    private float HandBackDistance()
    {
        Transform reference = nockPoint != null ? nockPoint : firePoint;
        Vector3 offset = rightHand.position - reference.position;
        return Vector3.Dot(offset, -firePoint.forward);
    }

    private void StartDraw()
    {
        if (rightHand == null || firePoint == null) return;

        // La mano tiene que estar cerca de la cola de la flecha para agarrarla
        Transform reference = nockPoint != null ? nockPoint : firePoint;
        if (nockGrabRadius > 0f && Vector3.Distance(rightHand.position, reference.position) > nockGrabRadius) return;

        drawing = true;
        pull = 0f;
        startOffset = HandBackDistance();
        nextHapticTime = 0f;
    }

    private void UpdateDraw()
    {
        float dist = HandBackDistance() - startOffset;
        pull = Mathf.Pow(Mathf.Clamp01(dist / maxPullDistance), pullResistance); // nunca pasa de 1: ya no cuenta más allá del límite

        // Vibración progresiva: más fuerte entre más jalas, máxima en el límite
        if (pull > 0.05f && Time.time >= nextHapticTime)
        {
            float amp = pull >= 0.98f
                ? limitHapticAmplitude
                : Mathf.Lerp(pullHapticMin, pullHapticMax, pull);
            Haptic(amp, hapticInterval * 1.5f);
            nextHapticTime = Time.time + hapticInterval;
        }
    }

    private void ReleaseDraw()
    {
        drawing = false;

        if (pull >= minPullToFire)
        {
            Shoot(Mathf.Lerp(minSpeed, maxSpeed, pull));
            nockedHidden = true;
            Haptic(releaseHapticAmplitude, releaseHapticDuration);
        }
        pull = 0f;
    }

    private void Shoot(float speed)
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Sale un poquito adelante del FirePoint para no nacer encima de la mano/arco
        GameObject proj = Instantiate(projectilePrefab, firePoint.position + firePoint.forward * 0.1f, firePoint.rotation);

        // Que la flecha no choque con el arco al salir
        foreach (Collider a in proj.GetComponentsInChildren<Collider>())
            foreach (Collider b in bowColliders)
                if (a != null && b != null) Physics.IgnoreCollision(a, b);

        if (proj.TryGetComponent(out ArrowFlight flight))
        {
            flight.Launch(firePoint.forward, speed);
        }
        else if (proj.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(firePoint.forward * speed, ForceMode.VelocityChange);
        }

        Destroy(proj, projectileLifetime);
    }

    private void Haptic(float amplitude, float duration)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            device.SendHapticImpulse(0, amplitude, duration);
    }
}