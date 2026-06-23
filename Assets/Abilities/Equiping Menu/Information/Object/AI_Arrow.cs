using HFHandyUtils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AI_Arrow : MonoBehaviour
{
    /// <summary>
    ///     Rect transform
    /// </summary>
    public RectTransform rectTransform;
    [Space]
    [SerializeField] private Image graphic = null;
    [SerializeField] private TextMeshProUGUI delayInformation = null;

    private AbilityInformation from = null;
    private AbilityInformation to = null;

    /// <summary>
    ///     Sets up arrow between two ability informations
    /// </summary>
    /// <param name="from">From ability information</param>
    /// <param name="to">To ability information</param>
    public void Connect(AbilityInformation from, AbilityInformation to)
    {
        if(from == null || to == null) return;
        // Set parent information
        this.from = from;
        this.to = to;

        TickTransform();
    }

    /// <summary>
    ///     Updates error transform
    /// </summary>
    public void TickTransform()
    {
        // Error check
        if (graphic == null || from == null || to == null) return;

        Vector2 direction = to.transform.position - from.transform.position;
        // Position the parent
        transform.localPosition = direction / 2;
        // Rotate the arrow
        graphic.transform.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, direction));
        // Scale the arrow
        float length = Vector2.Distance(from.transform.position, to.transform.position) - from.rectTransform.sizeDelta.x;
        length *= 1 / transform.localScale.x;
        graphic.rectTransform.sizeDelta = new Vector2(length, graphic.rectTransform.sizeDelta.y);
    }

    /// <summary>
    ///     Sets color of contained graphics
    /// </summary>
    /// <param name="color">New color</param>
    public void SetColor(TimeSpan delay)
    {
        // -> Evaluate strength and convert to style consistent value
        Color strengthColor = HFColor.errorGradient.Evaluate((1 - (delay.Milliseconds / 1000f)) + delay.Seconds);
        Color.RGBToHSV(strengthColor, out float h, out _, out _);
        strengthColor = Color.HSVToRGB(h, 0.61f, 1);

        if (graphic != null) graphic.color = strengthColor;
        if(delayInformation != null) delayInformation.color = strengthColor;
    }
    /// <summary>
    ///     Sets delay information
    /// </summary>
    /// <param name="delay">Delay timespan</param>
    public void SetDelayInformation(TimeSpan delay)
    {
        delayInformation.text = delay.Seconds.ToString();
        if (delay.Milliseconds > 0.0099f) delayInformation.text += '.' + delay.ToString("fff").Substring(0, 2);
        delayInformation.text += 's';
    }
}
