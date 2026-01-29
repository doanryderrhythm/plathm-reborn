using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Animator infoStorer;
    [SerializeField] bool isInfoStorerToggled = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleInformation()
    {
        if (!isInfoStorerToggled)
        {
            infoStorer.SetTrigger(ValueStorer.triggerInfoOpen);
            isInfoStorerToggled = true;
        }
        else
        {
            infoStorer.SetTrigger(ValueStorer.triggerInfoClose);
            isInfoStorerToggled = false;
        }
    }
}
