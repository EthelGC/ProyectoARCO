using UnityEngine;
using TMPro; // Para TextMeshPro. Si usas el Text normal, mira la nota al final.

public class ContadorImpactos : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textoContador;          // Arrastra aquí tu texto del Canvas
    public string prefijo = "Aciertos: ";

    [Header("Configuración")]
    public string tagObjetivo = "dianas";   // Tag de los objetos que cuentan

    private int conteo = 0;

    private void Start()
    {
        ActualizarTexto();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagObjetivo)) return;

        DianaMovimiento diana = other.GetComponentInParent<DianaMovimiento>();

        if (diana != null)
        {
            if (diana.EstaMuerta) return; // Ya muerta, no cuenta otra vez
            diana.Morir();                // Reproduce la animación Dead
        }

        conteo++;
        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (textoContador != null)
        {
            textoContador.text = prefijo + conteo;
        }
    }
}

// NOTA: si usas el Text clásico (UI > Legacy > Text):
// 1) quita "using TMPro;"
// 2) cambia "TMP_Text" por "UnityEngine.UI.Text"