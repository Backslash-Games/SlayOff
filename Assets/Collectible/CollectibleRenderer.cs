using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CollectibleRenderer : MonoBehaviour
{
    [SerializeField] private Collectible storedCollectible = null;
    [Space]
    [SerializeField] private Image renderImage = null;
    private Material material = null;

    #region Rendering
    /// <summary>
    ///     Renders collectible
    /// </summary>
    /// <param name="collectible">Input Collectible</param>
    public void RenderCollectible(Collectible collectible)
    {
        // Check if collectible is set
        if (collectible == null)
            return;

        // Set stored
        storedCollectible = new Collectible(collectible);
        // Set the render options
        SetRenderImageSprite(storedCollectible.GetDefinitionSprite()); // Image

        SetRenderTint(storedCollectible.GetQualityColor()); // Quality
        SetRenderMaterial((int)storedCollectible.GetMaterial()); // Material
        SetRendererScale(storedCollectible.GetDefectScale()); // Defect Scale
        SetDefectID((int)storedCollectible.GetDefect()); // Defect
        SetAnomaly(storedCollectible.GetAnomaly()); // Anomaly
    }
    public void RenderCollectible(uint binary)
    {
        RenderCollectible(new Collectible(binary));
    }

    private void SetRenderImageSprite(Sprite sprite) { GetRenderImage().sprite = sprite; }
    private void SetRenderTint(Color32 color) { GetRenderMaterial().SetColor("_TintColor", color); }
    private void SetRenderMaterial(int materialID) { GetRenderMaterial().SetFloat("_Material_ID", materialID); }
    private void SetRendererScale(Vector2 scale) { transform.localScale = scale; }
    private void SetDefectID(int defectID) { GetRenderMaterial().SetFloat("_Defect_ID", defectID); }
    private void SetAnomaly(bool state) { GetRenderMaterial().SetInt("_Anomaly", state ? 1 : 0); }
    #endregion

    #region Get Method
    /// <summary>
    ///     Gets the render image attached to the gameobject
    /// </summary>
    /// <returns>Image</returns>
    private Image GetRenderImage()
    {
        if (renderImage == null)
            renderImage = GetComponent<Image>();
        return renderImage;
    }
    /// <summary>
    ///     Gets the render material attached to the gameobject
    /// </summary>
    /// <returns>Material</returns>
    private Material GetRenderMaterial()
    {
        // Check if the material is null
        if(material == null)
        {
            // Create
            material = new Material(GetRenderImage().material);
            GetRenderImage().material = material;
        }

        return material;
    }
    #endregion

    #region Debug
    /// <summary>
    ///     Debug method to render a completely random collectible
    /// </summary>
    public void RenderArbitraryCollectible()
    {
        RenderCollectible(CollectibleGenerator.Instance.GenerateNewCollectible());
    }
    /// <summary>
    ///     Pulls binary from collectible generator and renders
    /// </summary>
    public void RenderBinaryCollectible()
    {
        RenderCollectible(CollectibleGenerator.Instance.GetCollectibleIdentifier());
    }
    #endregion
}
