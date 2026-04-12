using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NumericalItem : MonoBehaviour
{
    enum NumericalType
    {
        NUMERIC,
        BOOLEAN,
    }

    bool isSelected = false;

    [SerializeField] NumericalType numericalType;

    [Header("Numeric")]
    [SerializeField] float currentValue;
    [SerializeField] float minValue;
    [SerializeField] float maxValue;
    [SerializeField] float step;

    [Header("Boolean")]
    [SerializeField] bool isToggled;

    [Header("UI")]
    [SerializeField] Sprite arrowAvailable;
    [SerializeField] Sprite arrowNotAvailable;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [Space(10.0f)]
    [SerializeField] Sprite nonHighlightedSprite;
    [SerializeField] Sprite highlightedSprite;
    [SerializeField] Image background;
    [Space(10.0f)]
    [SerializeField] TMP_Text valueText;

    [Header("Event")]
    [SerializeField] UnityEvent onUp;
    [SerializeField] UnityEvent onDown;

    void Start()
    {
        ChangeHighlight();
        ShowValueUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                OnUp();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                OnDown();
        }
    }

    public void Toggle(bool isSelected)
    {
        this.isSelected = isSelected;
        ChangeHighlight();
    }

    void ChangeHighlight()
    {
        if (isSelected)
            background.sprite = highlightedSprite;
        else
            background.sprite = nonHighlightedSprite;
    }

    void ShowValueUI()
    {
        if (numericalType == NumericalType.NUMERIC)
        {
            valueText.text = currentValue.ToString("0.0");

            if (currentValue + step > maxValue)
                upArrow.sprite = arrowNotAvailable;
            else upArrow.sprite = arrowAvailable;

            if (currentValue - step < minValue)
                downArrow.sprite = arrowNotAvailable;
            else downArrow.sprite = arrowAvailable;
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            valueText.text = this.isToggled ? "ON" : "OFF";

            if (isToggled)
            {
                upArrow.sprite = arrowNotAvailable;
                downArrow.sprite = arrowAvailable;
            }
            else
            {
                upArrow.sprite = arrowAvailable;
                downArrow.sprite = arrowNotAvailable;
            }
        }

        upArrow.SetNativeSize();
        downArrow.SetNativeSize();
    }

    void OnUp()
    {
        if (!isSelected)
            return;

        if (numericalType == NumericalType.NUMERIC)
        {
            if (currentValue + step > maxValue)
                return;

            currentValue += step;
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            if (isToggled)
                return;

            isToggled = true;
        }
        ShowValueUI();

        onUp.Invoke();
    }

    void OnDown()
    {
        if (!isSelected)
            return;

        if (numericalType == NumericalType.NUMERIC)
        {
            if (currentValue - step < minValue)
                return;

            currentValue -= step;
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            if (!isToggled)
                return;

            isToggled = false;
        }
        ShowValueUI();

        onDown.Invoke();
    }
}
