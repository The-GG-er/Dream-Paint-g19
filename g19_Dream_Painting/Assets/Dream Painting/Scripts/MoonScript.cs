using UnityEngine;

public class MoonOrbit : MonoBehaviour
{
    public Transform planet;

    [Header("Orbit Settings")]
    public float orbitRadius = 8f;
    public float orbitSpeed = 20f;
    public float enterSpeed = 2f;

    private bool isActivated = false;
    private bool isOrbiting = false;

    private float angle;

    private Vector3 orbitAxis;

    void Start()
    {
        // Random orbit plane for nicer visuals
        orbitAxis = Random.onUnitSphere;
    }

    public void ActivateMoon()
    {
        isActivated = true;
    }

    void Update()
    {
        if (!isActivated) return;

        if (!isOrbiting)
        {
            // Move toward orbit radius
            Vector3 dir = (transform.position - planet.position).normalized;
            Vector3 targetPos = planet.position + dir * orbitRadius;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * enterSpeed);

            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist < 0.1f)
            {
                isOrbiting = true;
            }
        }
        else
        {
            // Orbit motion
            angle += orbitSpeed * Time.deltaTime;

            Quaternion rot = Quaternion.AngleAxis(angle, orbitAxis);
            Vector3 offset = rot * Vector3.forward * orbitRadius;

            transform.position = planet.position + offset;
        }
    }
}