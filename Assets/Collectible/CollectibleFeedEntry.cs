using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleFeedEntry : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image[] containedImages = new Image[0];
    [Space]
    [SerializeField] private CollectibleRenderer collectibleRenderer;
    [Space]
    private float currentRotation = 0;
    [SerializeField] private float currentTime = 0;
    [Space]
    [SerializeField] private Vector2 rotationRange = new Vector2(180, 120);
    [SerializeField] private Vector2 scaleRange = new Vector2(1f, 0.7f);
    [SerializeField] private Gradient colorRange = new Gradient();

    #region Sequencing
    public void Initialize(float rotation)
    {
        ForceRotation(rotation);
    }
    #endregion

    #region Transform
    /// <summary>
    ///     Sets the rotation, scale, and color of the entry
    /// </summary>
    /// <param name="rotation">Input Rotation</param>
    private void SetRotation(float rotation)
    {
        // Sets the rotation
        currentRotation = rotation;
        transform.eulerAngles = Vector3.forward * currentRotation;
        SetTimeBaseRotation(); // Sets the current time based on rotation

        // Sets scale based off rotation
        TimeScale();
        // Sets color based off rotation
        TimeColor();

        // Sets the sorting based off rotation
        TimeSort();
    }
    
    private float targetRotation = -1;
    private static float targetThreshold = 0.1f;
    private static float overTime_Speed = 15f;
    private Coroutine activeCoroutine = null;
    /// <summary>
    ///     Sets rotation over time
    /// </summary>
    /// <param name="rotation">Input Rotation</param>
    public void SetRotationOverTime(float rotation) 
    {
        // Set the target rotation
        targetRotation = rotation;
        // Stop the active coroutine
        if(activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        // Start the rotate coroutine
        activeCoroutine = StartCoroutine(RotateOverTime());
    }
    public void AddRotationOverTime(float amount) {
        // Check if our target is set
        if (targetRotation != -1)
            SetRotationOverTime(targetRotation + amount);
        else
            SetRotationOverTime(currentRotation + amount);
    }
    /// <summary>
    ///     Rotates object over time
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateOverTime() 
    {
        // Run while statement while the target rotation isn't equal to the current rotation
        while(currentRotation - targetRotation > targetThreshold)
        {
            float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * overTime_Speed);
            SetRotation(newRotation);
            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    ///     Forces the rotation of the entry
    /// </summary>
    /// <param name="rotation">Input Rotation</param>
    public void ForceRotation(float rotation) { SetRotation(rotation); }

    /// <summary>
    ///     Sets the scale based on an amount
    /// </summary>
    /// <param name="amount">Input</param>
    private void SetScale(float amount) { collectibleRenderer.transform.localScale = Vector3.one * amount; }
    /// <summary>
    ///     Sets the scale based on time
    /// </summary>
    private void TimeScale() { SetScale(Mathf.LerpUnclamped(scaleRange.x, scaleRange.y, currentTime)); }


    /// <summary>
    ///     Sets the color based on an amount
    /// </summary>
    /// <param name="amount">Input</param>
    private void SetColor(Color color) 
    {
        foreach (Image image in containedImages)
            image.color = color;
    }
    /// <summary>
    ///     Sets the color based on time
    /// </summary>
    private void TimeColor() { SetColor(colorRange.Evaluate(currentTime)); }
    #endregion
    #region Sorting
    /// <summary>
    ///     Sets the sorting layer on the canvas
    /// </summary>
    /// <param name="value">inut value</param>
    private void SetSorting(int value) { canvas.sortingOrder = value; }
    /// <summary>
    ///     Sort canvas based on time
    /// </summary>
    private void TimeSort() { SetSorting(-Mathf.RoundToInt(currentTime * 100)); }
    #endregion
    #region Collectible Rendering
    public void RenderCollectible(uint binary) { collectibleRenderer.RenderCollectible(binary); }
    #endregion

    #region Time Management
    /// <summary>
    ///     Sets the time based on rotation
    /// </summary>
    private void SetTimeBaseRotation() { currentTime = (rotationRange.x - currentRotation) / (rotationRange.x - rotationRange.y); }
    #endregion
}
