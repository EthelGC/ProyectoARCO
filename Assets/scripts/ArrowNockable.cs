
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ArrowNockable : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody arrowRigidbody;

    private Collider[] arrowColliders;

    public bool IsNocked { get; private set; }

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (arrowRigidbody == null)
            arrowRigidbody = GetComponent<Rigidbody>();

        arrowColliders = GetComponentsInChildren<Collider>(true);
    }

    public bool TryNock(Transform nockPoint, Transform bowRoot)
    {
        if (IsNocked ||
            nockPoint == null ||
            bowRoot == null ||
            grabInteractable == null ||
            arrowRigidbody == null ||
            !grabInteractable.isSelected)
        {
            return false;
        }

        var manager = grabInteractable.interactionManager;
        var interactor = grabInteractable.firstInteractorSelecting;

        if (manager == null || interactor == null)
            return false;

        // Soltar la flecha de la mano al engancharla.
        manager.SelectExit(interactor, grabInteractable);

        arrowRigidbody.linearVelocity = Vector3.zero;
        arrowRigidbody.angularVelocity = Vector3.zero;
        arrowRigidbody.useGravity = false;
        arrowRigidbody.isKinematic = true;

        foreach (Collider col in arrowColliders)
            col.enabled = false;

        grabInteractable.enabled = false;

        // Debe ser el mismo objeto que mueve el arco al agarrarlo.
        transform.SetParent(bowRoot, true);

        // Ajustar la posicion y orientacion iniciales.
        transform.SetPositionAndRotation(
            nockPoint.position,
            nockPoint.rotation
        );

        IsNocked = true;
        return true;
    }

    public void SetNockedPose(Vector3 position, Quaternion rotation)
    {
        if (!IsNocked)
            return;

        transform.SetPositionAndRotation(position, rotation);
    }

    public void Shoot(Vector3 velocity)
    {
        if (!IsNocked)
            return;

        // Desprender la flecha antes de aplicar la velocidad.
        IsNocked = false;
        transform.SetParent(null, true);

        foreach (Collider col in arrowColliders)
            col.enabled = true;

        arrowRigidbody.isKinematic = false;
        arrowRigidbody.useGravity = true;

        grabInteractable.enabled = true;
        arrowRigidbody.linearVelocity = velocity;
    }
}
