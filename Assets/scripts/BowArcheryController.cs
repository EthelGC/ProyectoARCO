
using UnityEngine;
using UnityEngine.InputSystem;

public class BowArcheryController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform nockPoint;
    [SerializeField] private Transform drawHand;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Transform bowRoot;

    [Header("Entrada para tensar y disparar")]
    [SerializeField] private InputActionReference drawAction;

    [Header("Tension y velocidad")]
    [SerializeField] private float maxDrawDistance = 0.45f;
    [SerializeField] private float minimumDrawToShoot = 0.06f;
    [SerializeField] private float minArrowSpeed = 5f;
    [SerializeField] private float maxArrowSpeed = 25f;

    [Header("Trayectoria")]
    [SerializeField] private int trajectoryPoints = 60;
    [SerializeField] private float timeStep = 0.05f;
    [SerializeField] private float arrowLength = 0.7f;
    [SerializeField] private LayerMask trajectoryCollisionMask = ~0;

    private ArrowNockable currentArrow;

    private Vector3 drawStartLocalPosition;
    private float currentDrawDistance;
    private bool wasDrawing;

    private void Awake()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }
    }

    public void SetNockedArrow(ArrowNockable arrow)
    {
        if (arrow == null || nockPoint == null || drawHand == null)
            return;

        currentArrow = arrow;
        currentDrawDistance = 0f;

        drawStartLocalPosition =
            nockPoint.InverseTransformPoint(drawHand.position);

        // La accion sigue pulsada fisicamente aunque se haya
        // soltado el objeto de la mano mediante SelectExit.
        wasDrawing = IsDrawPressed();

        currentArrow.SetNockedPose(
            nockPoint.position,
            nockPoint.rotation
        );

        HideTrajectory();
    }

    private void Update()
    {
        if (nockPoint == null || drawHand == null)
        {
            HideTrajectory();
            return;
        }

        if (currentArrow == null || !currentArrow.IsNocked)
        {
            currentArrow = null;
            HideTrajectory();
            return;
        }

        bool drawing = IsDrawPressed();

        if (drawing)
        {
            // Cuando empieza un nuevo intento, registrar la posicion
            // actual de la mano como punto de partida.
            if (!wasDrawing)
            {
                drawStartLocalPosition =
                    nockPoint.InverseTransformPoint(drawHand.position);

                currentDrawDistance = 0f;
            }

            Vector3 handLocalPosition =
                nockPoint.InverseTransformPoint(drawHand.position);

            // Retroceder a lo largo del eje de la cuerda.
            currentDrawDistance = Mathf.Clamp(
                drawStartLocalPosition.z - handLocalPosition.z,
                0f,
                maxDrawDistance
            );

            Vector3 arrowPosition =
                nockPoint.position -
                nockPoint.forward * currentDrawDistance;

            currentArrow.SetNockedPose(
                arrowPosition,
                nockPoint.rotation
            );

            if (currentDrawDistance >= minimumDrawToShoot)
                UpdateTrajectory();
            else
                HideTrajectory();
        }
        else
        {
            // Soltar el control despues de tensar dispara la flecha.
            if (wasDrawing)
            {
                if (currentDrawDistance >= minimumDrawToShoot)
                {
                    FireArrow();
                }
                else
                {
                    // Si la tension fue insuficiente, la flecha
                    // permanece en el arco y se puede intentar otra vez.
                    currentDrawDistance = 0f;

                    currentArrow.SetNockedPose(
                        nockPoint.position,
                        nockPoint.rotation
                    );
                }
            }

            HideTrajectory();
        }

        wasDrawing = drawing;
    }

    private bool IsDrawPressed()
    {
        return drawAction != null &&
               drawAction.action != null &&
               drawAction.action.IsPressed();
    }

    private float GetArrowSpeed()
    {
        float tension = Mathf.Clamp01(
            currentDrawDistance / maxDrawDistance
        );

        return Mathf.Lerp(
            minArrowSpeed,
            maxArrowSpeed,
            tension
        );
    }

    private void FireArrow()
    {
        if (currentArrow == null || nockPoint == null)
            return;

        float speed = GetArrowSpeed();
        Vector3 launchVelocity = nockPoint.forward.normalized * speed;

        ArrowNockable arrowToFire = currentArrow;
        currentArrow = null;
        currentDrawDistance = 0f;

        arrowToFire.Shoot(launchVelocity);
        HideTrajectory();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryLine == null ||
            currentArrow == null ||
            nockPoint == null)
        {
            HideTrajectory();
            return;
        }

        float speed = GetArrowSpeed();
        Vector3 forward = nockPoint.forward.normalized;
        Vector3 velocity = forward * speed;

        Transform arrowTip = currentArrow.transform.Find("ArrowTip");

        Vector3 origin = arrowTip != null
            ? arrowTip.position
            : currentArrow.transform.position + forward * arrowLength;

        Vector3 gravity = Physics.gravity;
        Vector3 previousPoint = origin;

        int pointCount = Mathf.Max(2, trajectoryPoints);

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = pointCount;
        trajectoryLine.SetPosition(0, origin);

        for (int i = 1; i < pointCount; i++)
        {
            float time = i * timeStep;

            Vector3 point =
                origin +
                velocity * time +
                0.5f * gravity * time * time;

            if (Physics.Linecast(
                previousPoint,
                point,
                out RaycastHit hit,
                trajectoryCollisionMask,
                QueryTriggerInteraction.Ignore))
            {
                bool hitBow =
                    bowRoot != null &&
                    (hit.transform == bowRoot ||
                     hit.transform.IsChildOf(bowRoot));

                bool hitArrow =
                    hit.transform == currentArrow.transform ||
                    hit.transform.IsChildOf(currentArrow.transform);

                if (!hitBow && !hitArrow)
                {
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPosition(i, hit.point);
                    return;
                }
            }

            trajectoryLine.SetPosition(i, point);
            previousPoint = point;
        }
    }

    private void HideTrajectory()
    {
        if (trajectoryLine == null)
            return;

        trajectoryLine.enabled = false;
        trajectoryLine.positionCount = 0;
    }
}
