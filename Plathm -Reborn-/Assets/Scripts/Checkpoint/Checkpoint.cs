using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    [SerializeField] ParticleSystem risingParticles;
    [SerializeField] ParticleSystem particles;
    [SerializeField] GameObject cpLight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleCheckpoint(bool isToggled)
    {
        if (isToggled)
        {
            spriteRenderer.color = ValueStorer.checkpointToggled;
            boxCollider.enabled = false;

            var particleMain = risingParticles.main;

            var startColor = particleMain.startColor;
            startColor.colorMin = ValueStorer.toggledMinParticles;
            startColor.colorMax = ValueStorer.toggledMaxParticles;
            particleMain.startColor = startColor;

            particles.gameObject.SetActive(true);
            cpLight.SetActive(true);
        }
        else
        {
            spriteRenderer.color = ValueStorer.checkpointUntoggled;
            boxCollider.enabled = true;
            var particleMain = risingParticles.main;

            particles.gameObject.SetActive(false);
            cpLight.SetActive(false);
        }
    }
}
