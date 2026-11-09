using System.Collections.Generic;
using UnityEngine;

public class WallTargetManager : MonoBehaviour
{
    [Header("Configuración de Dianas")]
    [Tooltip("Prefab de la diana a instanciar")]
    public GameObject targetPrefab;
    
    [Tooltip("Cantidad de dianas que se mantendrán activas")]
    public int maxTargets = 6;
    
    [Tooltip("Distancia desde la pared para evitar Z-Fighting")]
    public float wallOffset = 0.02f;

    [Header("Configuración de Raycast y Búsqueda")]
    [Tooltip("Capa de las paredes / entorno")]
    public LayerMask environmentLayerMask = -1;
    
    [Tooltip("Radio máximo de búsqueda alrededor del usuario")]
    public float searchRadius = 10f;

    [Tooltip("Transform del visor/jugador (ej. CenterEyeAnchor). Si es nulo, usa la MainCamera.")]
    public Transform centerEye;

    private List<GameObject> activeTargets = new List<GameObject>();

    private void Start()
    {
        if (centerEye == null && Camera.main != null)
        {
            centerEye = Camera.main.transform;
        }

        // Genera el lote inicial de 6 dianas
        MaintainTargetCount();
    }

    private void Update()
    {
        // Limpia la lista eliminando referencias a dianas destruidas
        activeTargets.RemoveAll(target => target == null);

        // Si hay menos de 6, intenta instanciar más hasta completar la cantidad requerida
        if (activeTargets.Count < maxTargets)
        {
            MaintainTargetCount();
        }
    }

    private void MaintainTargetCount()
    {
        int safetyLoop = 0;
        while (activeTargets.Count < maxTargets && safetyLoop < 30)
        {
            safetyLoop++;
            if (TrySpawnSingleTargetOnWall())
            {
                // Diana creada exitosamente
            }
        }
    }

    private bool TrySpawnSingleTargetOnWall()
    {
        if (centerEye == null) return false;

        // Dirección aleatoria en 360° en el plano horizontal (XZ)
        Vector3 randomDirection = Quaternion.Euler(
            Random.Range(-15f, 15f), // Ligera variación en inclinación vertical
            Random.Range(0f, 360f),  // Ángulo en horizontal
            0f
        ) * Vector3.forward;

        Vector3 origin = centerEye.position;

        if (Physics.Raycast(origin, randomDirection, out RaycastHit hit, searchRadius, environmentLayerMask))
        {
            // Comprobación de que la superficie sea una pared (Normal casi horizontal respecto al suelo)
            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f)
            {
                Vector3 spawnPos = hit.point + (hit.normal * wallOffset);
                Quaternion spawnRot = Quaternion.LookRotation(hit.normal);

                GameObject newTarget = Instantiate(targetPrefab, spawnPos, spawnRot);
                activeTargets.Add(newTarget);
                return true;
            }
        }

        return false;
    }
}