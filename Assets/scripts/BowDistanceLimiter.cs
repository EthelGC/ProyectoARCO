
using UnityEngine;


public class BowDistanceLimiter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable bowGrab;
    [SerializeField] private Transform playerHead;

    [Header("Distancia maxima desde la cabeza")]
    [SerializeField, Min(0.2f)]
    private float maxDistanceFromHead = 0.85f;

    private void Awake()
    {
        if (bowGrab == null)
            bowGrab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void LateUpdate()
    {
        if (bowGrab == null ||
            playerHead == null ||
            !bowGrab.isSelected)
            return;

        Vector3 offset = transform.position - playerHead.position;

        if (offset.sqrMagnitude >
            maxDistanceFromHead * maxDistanceFromHead)
        {
            transform.position =
                playerHead.position +
                offset.normalized * maxDistanceFromHead;
        }
    }
}
