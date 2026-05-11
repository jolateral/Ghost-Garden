using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// Attach this to each Button GameObject on the title screen.
// Assign the button's TextMeshPro child text object in the Inspector.
// The text will become bold when the mouse hovers over the button
// and return to normal when the mouse leaves.

public class ButtonHoverBold : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Reference")]
    // Drag the TMP text child of this button into this field
    public TextMeshProUGUI buttonText;

    [Header("Optional")]
    // If you also want to play a sound on hover, enable this
    public bool playHoverSound = true;

    void Start()
    {
        // Auto-find the TMP text child if not assigned in Inspector
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText == null)
            Debug.LogWarning($"[ButtonHoverBold] No TextMeshProUGUI found on {gameObject.name}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText == null) return;
        buttonText.fontStyle = FontStyles.Bold;

        if (playHoverSound)
            AudioManager.Instance?.PlayUIPop();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText == null) return;
        buttonText.fontStyle = FontStyles.Normal;
    }
}