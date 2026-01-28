using UnityEngine;

public class VerticalGrid : MonoBehaviour
{
    public bool isHovered;
    private EditorManager editorManager;
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isHovered = false;

        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (editorManager.worldPosition.x >= this.transform.position.x - ValueStorer.gridOffset &&
            editorManager.worldPosition.x <= this.transform.position.x + ValueStorer.gridOffset)
        {
            isHovered = true;
            spriteRenderer.color = ValueStorer.gridSelectedColor;
        }
        else
        {
            isHovered = false;
            spriteRenderer.color = ValueStorer.gridDefaultColor;
        }

        if (isHovered && editorManager.draggedNote != null)
        {
            editorManager.verticalGridValue = this.transform.position.x;
        }
    }
}
