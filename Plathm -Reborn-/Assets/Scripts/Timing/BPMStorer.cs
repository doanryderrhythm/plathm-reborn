using TMPro;
using UnityEngine;

public class BPMStorer : MonoBehaviour
{
    private EditorManager editorManager;
    private UIManager uiManager;

    public int index;
    public float timing = 0f;
    public float BPM = 0f;

    [SerializeField] TMP_Text numberLabel;
    [SerializeField] TMP_InputField timingInputField;
    [SerializeField] TMP_InputField BPMInputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        uiManager = GameObject.FindFirstObjectByType<UIManager>();

        ConvertFromValueToText();
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

    public void ConvertFromValueToText()
    {
        timingInputField.text = timing.ToString();
        BPMInputField.text = BPM.ToString();
    }

    public void ConvertFromTextToValue()
    {
        bool isTimingParsed = float.TryParse(timingInputField.text, out float timing);
        bool isBPMParsed = float.TryParse(BPMInputField.text, out float BPM);
        
        if (!isTimingParsed || !isBPMParsed)
        {
            timingInputField.text = this.timing.ToString();
            BPMInputField.text = this.BPM.ToString();
            return;
        }

        this.timing = timing;
        this.BPM = BPM;
    }

    public void MoveUp()
    {
        uiManager.SwitchTimingItems(editorManager.timingGroupIndex, this.index, this.index - 1);
    }

    public void MoveDown()
    {
        uiManager.SwitchTimingItems(editorManager.timingGroupIndex, this.index, this.index + 1);
    }

    public void DeleteItem()
    {
        uiManager.RemoveTimingItem(editorManager.timingGroupIndex, this.index);
    }
}
