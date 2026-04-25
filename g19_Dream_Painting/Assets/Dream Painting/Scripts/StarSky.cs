using UnityEngine;

public class StarField : MonoBehaviour
{
    public int count = 5000;
    public float radius = 50f;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Random.onUnitSphere * radius;

            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.transform.position = pos;
            star.transform.localScale = Vector3.one * 0.05f;
            star.GetComponent<Renderer>().material.color = Color.white;
            star.transform.parent = transform;
        }
    }
}