using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MRTargetSpawner : MonoBehaviour
{
    [Header("Referencias XR")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject targetPrefab;

    [Header("Configuración")]
    [SerializeField] private int totalTargets = 5;
    [SerializeField] private float surfaceOffset = 0.02f; // Separación para evitar z-fighting

    private List<GameObject> spawnedTargets = new List<GameObject>();
    private bool hasSpawned = false;

    private void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (hasSpawned) return;

        // Una vez que el visor detecta superficies, generamos las 5 dianas
        if (planeManager.trackables.count > 0)
        {
            SpawnTargetsOnPlanes();
            hasSpawned = true;
        }
    }

    private void SpawnTargetsOnPlanes()
    {
        List<ARPlane> availablePlanes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            availablePlanes.Add(plane);
        }

        for (int i = 0; i < totalTargets; i++)
        {
            // Selecciona un plano al azar (pared, mesa, piso, etc.)
            ARPlane selectedPlane = availablePlanes[Random.Range(0, availablePlanes.Count)];

            // Calcula una posición aleatoria dentro de los límites del plano
            Vector2 planeSize = selectedPlane.size;
            Vector3 localRandomPos = new Vector3(
                Random.Range(-planeSize.x / 2f, planeSize.x / 2f),
                0,
                Random.Range(-planeSize.y / 2f, planeSize.y / 2f)
            );

            // Convierte a posición y orientación globales
            Vector3 worldPos = selectedPlane.transform.TransformPoint(localRandomPos) + (selectedPlane.normal * surfaceOffset);
            Quaternion rotation = Quaternion.LookRotation(selectedPlane.normal);

            GameObject newTarget = Instantiate(targetPrefab, worldPos, rotation);
            spawnedTargets.Add(newTarget);
        }
    }
}