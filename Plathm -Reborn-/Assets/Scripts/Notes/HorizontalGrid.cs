using UnityEngine;

public class HorizontalGrid : MonoBehaviour
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
        if (editorManager.worldPosition.y >= this.transform.position.y - ValueStorer.gridOffset &&
            editorManager.worldPosition.y <= this.transform.position.y + ValueStorer.gridOffset)
        {
            isHovered = true;
            spriteRenderer.color = ValueStorer.gridSelectedColor;
        }
        else
        {
            isHovered = false;
            spriteRenderer.color = ValueStorer.gridDefaultColor;
        }

        if (isHovered && editorManager.draggedNote)
        {
            editorManager.confirmedHorizontalGrid = this;
            editorManager.isTimingGridConfirm = true;
            editorManager.horizontalGridValue = this.transform.position.y;
        }
        else if (!isHovered && editorManager.confirmedHorizontalGrid == this)
        {
            editorManager.isTimingGridConfirm = false;
        }
    }
}
