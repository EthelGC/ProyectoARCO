
using UnityEngine;

public class Diana : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Verificar que el objeto que impactó sea una flecha
        if (collision.gameObject.CompareTag("flecha"))
        {
            // Sumar al contador
            ContadorDianas contador = FindFirstObjectByType<ContadorDianas>();

            if (contador != null)
            {
                contador.SumarDiana();
            }

            // Destruir la diana
            Destroy(gameObject);

            // Destruir la flecha
            Destroy(collision.gameObject);
        }
    }
}
