using UnityEngine;

/// <summary>
/// Va en el prefab de la flecha (junto a su Rigidbody).
/// - La flecha siempre apunta hacia donde va (nunca sale chueca).
/// - Gravedad reducida para que llegue más lejos y vuele casi horizontal.
/// - Se clava (se detiene) al chocar con algo, pero NO justo al salir.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ArrowFlight : MonoBehaviour
{
    [Header("Orientación del modelo")]
    [Tooltip("Corrige hacia dónde apunta la punta de TU modelo.")]
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;

    [Header("Vuelo")]
    [Tooltip("0 = vuela recto sin caer. 1 = gravedad normal. Menos = llega más lejos")]
    [SerializeField, Range(0f, 1f)] private float gravityScale = 0.2f;

    [Header("Impacto")]
    [SerializeField] private bool stickOnImpact = true;
    [Tooltip("Segundos después de salir en los que ignora choques (evita clavarse en la mano/jugador)")]
    [SerializeField] private float armTime = 0.15f;

    private Rigidbody rb;
    private Vector3 lastPosition;
    private float spawnTime;
    private bool stuck;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        lastPosition = transform.position;
    }

    /// <summary>Lo llama el arco al disparar.</summary>
    public void Launch(Vector3 direction, float speed)
    {
        direction.Normalize();

        // Asegurar que la física esté activa (por si el prefab venía en Kinematic o con restricciones)
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false;                 // la gravedad la aplicamos nosotros
        rb.freezeRotation = true;              // la rotación la controla este script
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(modelRotationOffset);
        lastPosition = transform.position;
        rb.AddForce(direction * speed, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (stuck || rb.isKinematic) return;

        // Gravedad suave
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        // Apuntar hacia donde se está moviendo
        Vector3 moved = transform.position - lastPosition;
        if (moved.sqrMagnitude > 0.000001f)
            transform.rotation = Quaternion.LookRotation(moved.normalized) * Quaternion.Euler(modelRotationOffset);

        lastPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!stickOnImpact || stuck) return;
        if (Time.time - spawnTime < armTime) return; // ignorar choques al salir

        stuck = true;
        rb.isKinematic = true;
        Debug.Log("Flecha clavada en: " + collision.gameObject.name);
    }
}