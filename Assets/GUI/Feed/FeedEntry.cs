using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FeedEntry : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image[] containedImages = new Image[0];
    [Space]
    [SerializeField] private float currentTime = 0;
    [Space]
    [SerializeField] private Vector2[] positionRange = new Vector2[2];
    private float positionDistance = 0;
    [SerializeField] private Vector2 rotationRange = new Vector2(180, 120);
    [Space]
    [SerializeField] private Vector2 scaleRange = new Vector2(1f, 0.7f);
    [SerializeField] private Gradient colorRange = new Gradient();
    [Space]
    [SerializeField] private Transform targetTransform;

    #region Sequencing
    public void Initialize(Vector3 position, float rotation)
    {
        ForcePosition(position);
        ForceRotation(rotation);
    }
    #endregion

    #region Bump Handling
    /// <summary>
    ///     Handle the bump logic
    /// </summary>
    /// <param name="amount">Bump Value</param>
    public virtual void Bump(float amount) { }
    #endregion

    #region Transform
    private static float targetThreshold = 0.1f;
    private static float overTime_Speed = 15f;

    #region Position
    private Vector3 targetPosition = new Vector3(0, 0, -1);
    private Coroutine activePosition = null;
    private Vector3 currentPosition = Vector3.zero;

    /// <summary>
    ///     Sets the position, scale, and color of the entry
    /// </summary>
    /// <param name="rotation">Input Position</param>
    private void SetPosition(Vector3 offset)
    {
        // Sets the position
        currentPosition = offset;
        transform.localPosition = currentPosition;
        SetTimeBasePosition(); // Sets the current time based on position

        // Sets scale based off position
        TimeScale();
        // Sets color based off position
        TimeColor();

        // Sets the sorting based off position
        TimeSort();
    }

    /// <summary>
    ///     Sets position over time
    /// </summary>
    /// <param name="rotation">Input position</param>
    public void SetPositionOverTime(Vector3 offset)
    {
        // Set the target position
        targetPosition = new Vector3(offset.x, offset.y, 0);
        // Stop the active coroutine
        if (activePosition != null)
            StopCoroutine(activePosition);
        // Start the position coroutine
        activePosition = StartCoroutine(PositionOverTime());
    }
    public void AddPositionOverTime(Vector3 offset)
    {
        // Check if our target is set
        if (targetPosition.z != -1)
            SetPositionOverTime(targetPosition + offset);
        else
            SetPositionOverTime(targetPosition + offset);
    }
    /// <summary>
    ///     Sets position object over time
    /// </summary>
    /// <returns>Wait</returns>
    private IEnumerator PositionOverTime()
    {
        // Run while statement while the target position isn't equal to the current position
        while (Vector3.Distance(currentPosition, targetPosition) > targetThreshold)
        {
            Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * overTime_Speed);
            SetPosition(newPosition);
            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    ///     Forces the position of the entry
    /// </summary>
    /// <param name="rotation">Input position</param>
    public void ForcePosition(Vector3 offset) { SetPosition(offset); }
    #endregion
    #region Rotation
    private float targetRotation = -1;
    private Coroutine activeRotation = null;
    private float currentRotation = 0;

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
    
    /// <summary>
    ///     Sets rotation over time
    /// </summary>
    /// <param name="rotation">Input Rotation</param>
    public void SetRotationOverTime(float rotation) 
    {
        // Set the target rotation
        targetRotation = rotation;
        // Stop the active coroutine
        if(activeRotation != null)
            StopCoroutine(activeRotation);
        // Start the rotate coroutine
        activeRotation = StartCoroutine(RotateOverTime());
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
    #endregion
    #region Scale
    /// <summary>
    ///     Sets the scale based on an amount
    /// </summary>
    /// <param name="amount">Input</param>
    private void SetScale(float amount) { targetTransform.localScale = Vector3.one * amount; }
    /// <summary>
    ///     Sets the scale based on time
    /// </summary>
    private void TimeScale() { SetScale(Mathf.LerpUnclamped(scaleRange.x, scaleRange.y, currentTime)); }
    #endregion

    #region Color
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

    #endregion
    #region Sorting
    /// <summary>
    ///     Sets the sorting layer on the canvas
    /// </summary>
    /// <param name="value">inut value</param>
    private void SetSorting(int value) 
    { 
        if(canvas != null)
            canvas.sortingOrder = value; 
    }
    /// <summary>
    ///     Sort canvas based on time
    /// </summary>
    private void TimeSort() { SetSorting(-Mathf.RoundToInt(currentTime * 100)); }
    #endregion
    #region Rendering Container
    /// <summary>
    ///     Updates rendering components with generic input
    /// </summary>
    /// <param name="value">Input value</param>
    public virtual void TickRenderer<T>(T value) { }
    #endregion

    #region Time Management
    /// <summary>
    ///     Sets the time based on position
    /// </summary>
    private void SetTimeBasePosition()
    {
        // Check if position range is set properly
        if (positionRange.Length != 2)
        {
            Debug.LogError("Position range length does not equal 2");
            return;
        }
        // Check position distance
        if (positionDistance == 0)
            positionDistance = Vector2.Distance(positionRange[0], positionRange[1]);

        // Get the current time
        currentTime = Vector2.Distance(currentPosition, positionRange[0]) / positionDistance; 
    }
    /// <summary>
    ///     Sets the time based on rotation
    /// </summary>
    private void SetTimeBaseRotation() { currentTime = (rotationRange.x - currentRotation) / (rotationRange.x - rotationRange.y); }
    #endregion
}
