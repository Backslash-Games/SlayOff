using UnityEngine;
using TMPro;

public class MonoDebug : MonoBehaviour
{
    [SerializeField] private bool suppressAllMonoDebug = false; // Controls all mono debug suppression. If one is enabled then it will notify the user where the suppression source is
    [SerializeField] private bool suppressLocalMonoDebug = false; // Controls suppression for only this mono debug script
    private static bool suppressDebug = false;
    [Space]
    [SerializeField] private Vector3 offset;
    private static Vector3 defaultOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private Color textColor = Color.white;
    [Space]
    [SerializeField] private DebugStyle debugStyle = DebugStyle.World;
    private enum DebugStyle { Canvas, World }
    [SerializeField] private TextAlignmentOptions canvas_allignment = TextAlignmentOptions.TopLeft;
    [SerializeField] private MonoBehaviour source = null;

    private Canvas canvas;
    private static float canvasScale_world = 2.5f;

    private bool billboardingActive = false;

    private TextMeshProUGUI text;
    private static float textPadding_canvas = 5;

    #region Unity Methods
    private void Awake()
    {
        // Right out of the gates check our suppression
        // -> This needs to stay as the first call in awake
        CheckSuppression();

        // Build the inital components
        Initialize();
    }

    private void Update()
    {
        Tick();
    }
    #endregion
    #region Suppression Handling
    /// <summary>
    ///     Checks to see if our source is suppressing functionality
    /// </summary>
    private void CheckSuppression()
    {
        // Check for a suppression
        if(suppressAllMonoDebug && !suppressDebug)
        {
            suppressDebug = true;
            Debug.LogError($"{name} has suppressed all Mono Debug Scripts");
        }

        // Check for a local suppression
        if(source == null)
        {
            suppressLocalMonoDebug = true;
            Debug.LogError($"{name} has no defined source");
        }
    }
    /// <summary>
    ///     Checks to see if our script is currently suppressed
    /// </summary>
    /// <returns>True when suppressed</returns>
    private bool isSuppressed()
    {
        return suppressDebug || suppressLocalMonoDebug;
    }
    #endregion

    #region Text Drawing
    /// <summary>
    ///     Builds the initial objects for debugging
    /// </summary>
    private void Initialize()
    {
        // Check if we are suppressed
        if (isSuppressed())
            return;
        // Build objects
        BuildObjects();

        // Draw style
        if (debugStyle.Equals(DebugStyle.World))
            Draw_World();
        else
            Draw_Canvas();
    }

    /// <summary>
    ///     Builds the initla objects in the world
    /// </summary>
    private void Draw_World()
    {
        // Apply Settings
        canvas.renderMode = RenderMode.WorldSpace;

        // Apply Sizing
        canvas.GetComponent<RectTransform>().sizeDelta = text.rectTransform.sizeDelta = canvasScale_world * (Vector3.one + Vector3.right);
        // -> Correct sizing for billboarding
        text.rectTransform.localScale = new Vector3(1, -1, 1);
        canvas.worldCamera = Camera.main;
        text.fontSize = 0.15f;
        text.alignment = TextAlignmentOptions.BottomLeft;

        // Setup billboard
        billboardingActive = true;
    }
    /// <summary>
    ///     Builds the initial objects on a canvas
    /// </summary>
    private void Draw_Canvas()
    {
        // Apply Settings
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        // Apply Text Sizing
        text.rectTransform.sizeDelta = new Vector2(Screen.width - textPadding_canvas, Screen.height - textPadding_canvas);
        text.fontSize = 18;
        text.alignment = canvas_allignment;
    }

    /// <summary>
    ///     Builds draw and text objects
    /// </summary>
    private void BuildObjects()
    {
        // Make sure our draw object is built
        BuildDrawObject();
        // Make sure our text object is built
        BuildTextObject();
    }
    /// <summary>
    ///     Builds the draw object and orients in the world properly
    /// </summary>
    private void BuildDrawObject()
    {
        // Check if the object is null
        if (canvas != null)
            return;

        // Create obect
        GameObject sObject = new GameObject($"MonoDebug_Canvas_{source.name}");
        sObject.transform.parent = source.transform;
        // Add component
        canvas = sObject.gameObject.AddComponent<Canvas>();
        // Orient
        canvas.transform.localPosition = defaultOffset + offset;
    }
    /// <summary>
    ///     Builds the text object and orients in the world properly
    /// </summary>
    private void BuildTextObject()
    {
        // Check if the object is null
        if (text != null)
            return;

        // Create obect
        GameObject sObject = new GameObject($"MonoDebug_Text_{source.name}");
        sObject.transform.parent = canvas.transform;
        // Add component
        text = sObject.gameObject.AddComponent<TextMeshProUGUI>();
        // Orient
        text.transform.localPosition = Vector3.zero;

        // Color
        text.color = textColor;
    }
    #endregion
    #region Updating
    /// <summary>
    ///     Steps mono debug forward by 1
    /// </summary>
    private void Tick()
    {
        // Update the text
        UpdateText();
        // Update the billboard
        UpdateBillboard();
    }

    /// <summary>
    ///     Updates text
    /// </summary>
    private void UpdateText()
    {
        // Make sure text is set
        if (text == null)
            return;

        // Update the text
        text.text = source.ToString();
    }
    /// <summary>
    ///     Updates billboarding if active
    /// </summary>
    private void UpdateBillboard()
    {
        // Check if we are billboarding
        if (!billboardingActive)
            return;
        // Make sure our canvas object is set
        if (canvas == null)
            return;

        // Make canvas object face the main camera
        canvas.transform.LookAt(Camera.main.transform, Vector3.down);
    }
    #endregion
}
