using UnityEngine;

public class Moon : MonoBehaviour
{
    public Transform planet;
    public float speed = 5f;
    public int type;

    private Vector3 velocity;
    private bool locked = false;

    void Update()
    {
        Vector3 dir = planet.position - transform.position;
        float dist = dir.magnitude;

        if (!locked)
        {
            // gravity pull
            velocity += dir.normalized * Time.deltaTime * 2f;
            transform.position += velocity * Time.deltaTime;

            // capture orbit
            if (dist < 2.5f)
            {
                locked = true;
                velocity = Vector3.Cross(dir.normalized, Vector3.up) * speed;
            }
        }
        else
        {
            // stable orbit
            Vector3 orbitDir = Vector3.Cross(dir.normalized, Vector3.up);
            transform.position = planet.position + dir.normalized * 2.5f;
            transform.position += orbitDir * speed * Time.deltaTime;
        }
    }
}