
using UnityEngine;

public class BowNockZone : MonoBehaviour
{
    [SerializeField] private Transform bowRoot;
    [SerializeField] private BowArcheryController archeryController;

    private void OnTriggerEnter(Collider other)
    {
        ArrowNockable arrow =
            other.GetComponentInParent<ArrowNockable>();

        if (arrow == null)
            return;

        if (bowRoot == null || archeryController == null)
        {
            Debug.LogError(
                "BowNockZone: asigna Bow Root y Bow Archery Controller."
            );
            return;
        }

        if (arrow.TryNock(transform, bowRoot))
        {
            archeryController.SetNockedArrow(arrow);
            Debug.Log("Flecha enganchada y lista para tensar.");
        }
    }
}
