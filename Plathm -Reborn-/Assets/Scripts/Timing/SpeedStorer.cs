using TMPro;
using UnityEngine;

public class SpeedStorer : MonoBehaviour
{
    private EditorManager editorManager;
    private UIManager uiManager;

    public int index;
    public float timing = 0f;
    public float speedMulti = 0f;

    [SerializeField] TMP_Text numberLabel;
    [SerializeField] TMP_InputField timingInputField;
    [SerializeField] TMP_InputField speedMultiInputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        uiManager = GameObject.FindFirstObjectByType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeNumberLabel(int index)
    {
        this.index = index;
        numberLabel.text = this.index.ToString();
    }

    public void ConvertFromTextToValue()
    {
        bool isTimingParsed = float.TryParse(timingInputField.text, out float timing);
        bool isSpeedMultiParsed = float.TryParse(speedMultiInputField.text, out float speedMulti);

        if (!isTimingParsed || !isSpeedMultiParsed)
        {
            timingInputField.text = this.timing.ToString();
            speedMultiInputField.text = this.speedMulti.ToString();
            return;
        }

        this.timing = timing;
        this.speedMulti = speedMulti;
    }

    public void MoveUp()
    {
        uiManager.SwitchSpeedItems(this.index, this.index - 1);
    }

    public void MoveDown()
    {
        uiManager.SwitchSpeedItems(this.index, this.index + 1);
    }

    public void DeleteItem()
    {
        uiManager.RemoveSpeedItem(this.index);
    }
}
