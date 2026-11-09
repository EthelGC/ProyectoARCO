using UnityEngine;

public class DianaMovimiento : MonoBehaviour
{
    public enum TipoMovimiento
    {
        Lados,      // izquierda <-> derecha (eje X)
        Frente,     // adelante <-> atrás (eje Z)
        Vertical    // arriba <-> abajo (eje Y)
    }

    [Header("Movimiento")]
    public TipoMovimiento tipoMovimiento = TipoMovimiento.Lados;
    public float distancia = 3f;
    public float velocidad = 2f;
    [Tooltip("Si está activo, los ejes se toman según la rotación de la diana. Si no, son los ejes del mundo.")]
    public bool usarEjesLocales = false;

    [Header("Animación de muerte")]
    public Animator animator;                  // Si lo dejas vacío, lo busca solo en el modelo hijo
    public string nombreEstadoMuerte = "Dead"; // Nombre EXACTO del estado en el Animator
    public float segundosAntesDeDestruir = 3f; // 0 = no destruir el objeto
    public bool EstaMuerta { get; private set; }

    private Vector3 posicionInicial;
    private Vector3 direccion;

    void Start()
    {
        posicionInicial = transform.position;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        direccion = ObtenerDireccion();
    }

    void Update()
    {
        if (EstaMuerta) return; // Si ya murió, deja de moverse

        float desplazamiento = Mathf.PingPong(
            Time.time * velocidad,
            distancia * 2
        ) - distancia;

        transform.position = posicionInicial + direccion * desplazamiento;
    }

    private Vector3 ObtenerDireccion()
    {
        switch (tipoMovimiento)
        {
            case TipoMovimiento.Frente:
                return usarEjesLocales ? transform.forward : Vector3.forward;
            case TipoMovimiento.Vertical:
                return usarEjesLocales ? transform.up : Vector3.up;
            default: // Lados
                return usarEjesLocales ? transform.right : Vector3.right;
        }
    }

    public void Morir()
    {
        if (EstaMuerta) return;
        EstaMuerta = true;

        // Desactiva los colliders para que no vuelva a contar otro golpe
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        if (animator != null)
        {
            animator.Play(nombreEstadoMuerte);
        }

        if (segundosAntesDeDestruir > 0f)
        {
            Destroy(gameObject, segundosAntesDeDestruir);
        }
    }
}