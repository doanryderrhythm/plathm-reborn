using UnityEngine;
using UnityEngine.UI;

public class InputPlaceholder : MonoBehaviour
{
    [System.Serializable]
    struct KeyInput
    {
        public KeyCode key;
        [Space(5.0f)]
        public bool isFreeKey;
        public string keyString;
        [Space(5.0f)]
        public string action;
    }

    [System.Serializable]
    struct KeyPage
    {
        public KeyInput[] leftInput;
        public KeyInput[] rightInput;
    }

    [SerializeField] HorizontalLayoutGroup leftGroup;
    [SerializeField] HorizontalLayoutGroup rightGroup;

    [Space(10.0f)]
    private int pageIndex = 0;
    [SerializeField] KeyPage[] pages;

    [SerializeField] InputIndicator leftInputPrefab;
    [SerializeField] InputIndicator rightInputPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdatePage(0);
    }

    public void UpdatePage(int pageIndex)
    {
        foreach (Transform obj in leftGroup.transform)
            if (obj) Destroy(obj.gameObject);

        foreach (Transform obj in rightGroup.transform)
            if (obj) Destroy(obj.gameObject);

        if (pageIndex >= 0 && pageIndex < pages.Length)
        {
            this.pageIndex = pageIndex;
            for (int i = 0; i < pages[pageIndex].leftInput.Length; i++)
            {
                GameObject inputObject = Instantiate(leftInputPrefab.gameObject, leftGroup.transform);
                InputIndicator indicator = inputObject.GetComponent<InputIndicator>();

                if (!indicator)
                    continue;

                if (pages[pageIndex].leftInput[i].isFreeKey)
                    indicator.InsertSpecificAction(
                        pages[pageIndex].leftInput[i].keyString,
                        pages[pageIndex].leftInput[i].action);
                else
                    indicator.InsertSpecificAction(
                        pages[pageIndex].leftInput[i].key, 
                        pages[pageIndex].leftInput[i].action);
            }

            for (int i = 0; i < pages[pageIndex].rightInput.Length; i++)
            {
                GameObject inputObject = Instantiate(rightInputPrefab.gameObject, rightGroup.transform);
                InputIndicator indicator = inputObject.GetComponent<InputIndicator>();

                if (!indicator)
                    continue;

                if (pages[pageIndex].rightInput[i].isFreeKey)
                    indicator.InsertSpecificAction(
                        pages[pageIndex].rightInput[i].keyString,
                        pages[pageIndex].rightInput[i].action);
                else
                    indicator.InsertSpecificAction(
                        pages[pageIndex].rightInput[i].key,
                        pages[pageIndex].rightInput[i].action);
            }
        }
    }
}
