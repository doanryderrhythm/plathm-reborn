using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
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
        }
        else
        {
            spriteRenderer.color = ValueStorer.checkpointUntoggled;
            boxCollider.enabled = true;
        }
    }
}
