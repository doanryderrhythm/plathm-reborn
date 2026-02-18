using TMPro;
using UnityEngine;

public class HorizontalGrid : MonoBehaviour
{
    public bool isHovered;
    private EditorManager editorManager;
    private SpriteRenderer spriteRenderer;

    [SerializeField] float timing;
    [SerializeField] TextMeshPro timingText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isHovered = false;

        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();

        timing = (int)(transform.localPosition.y / editorManager.chartSpeed * 1000f);
        timingText.text = timing.ToString();
        timingText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (editorManager.worldPosition.y >= this.transform.position.y - ValueStorer.gridOffset &&
            editorManager.worldPosition.y <= this.transform.position.y + ValueStorer.gridOffset)
        {
            isHovered = true;
            spriteRenderer.color = ValueStorer.gridSelectedColor;
            timingText.gameObject.SetActive(true);
        }
        else
        {
            isHovered = false;
            spriteRenderer.color = ValueStorer.gridDefaultColor;
            timingText.gameObject.SetActive(false);
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
