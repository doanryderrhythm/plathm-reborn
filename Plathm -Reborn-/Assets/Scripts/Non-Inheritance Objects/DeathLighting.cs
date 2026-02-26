using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DeathLighting : MonoBehaviour
{
    [SerializeField] float timer = 1.5f;
    private float currentTime = 0f;
    [SerializeField] Light2D lighting;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, timer);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime <= timer)
        {
            lighting.color = new Color(lighting.color.r, lighting.color.g, lighting.color.b, (timer - currentTime) / timer);
        }
        else
        {
            lighting.color = new Color(lighting.color.r, lighting.color.g, lighting.color.b, 0f);
        }

        currentTime += Time.deltaTime;
    }
}
