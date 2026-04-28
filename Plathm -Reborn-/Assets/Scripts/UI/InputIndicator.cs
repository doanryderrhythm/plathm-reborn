using TMPro;
using UnityEngine;

public class InputIndicator : MonoBehaviour
{
    public KeyCode key;
    public string actionName;

    [SerializeField] TMP_Text keyText;
    [SerializeField] TMP_Text actionText;

    void Start()
    {
        
    }

    public void InsertSpecificAction(string keyString, string actionName)
    {
        keyText.text = keyString;
        actionText.text = actionName;
    }

    public void InsertSpecificAction(KeyCode key, string actionName)
    {
        this.key = key;
        this.actionName = actionName;

        InsertAction();
    }

    void InsertAction()
    {
        keyText.text = key switch
        {
            KeyCode.Return => "enter",
            KeyCode.Escape => "esc",
            KeyCode.Space => "space",
            _ => key.ToString()
        };
        actionText.text = actionName.ToString();
    }
}
