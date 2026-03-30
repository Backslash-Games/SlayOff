using UnityEngine;

[System.Serializable]
public class Collectible
{
    [SerializeField] private CollectibleDefinition definition = null;
    private byte definitionID = 0;

    private static readonly char charIgnore = '?';

    private static readonly int[] qWeight = new int[] { 1000, 500, 250, 125, 62, 31, 15, 1 };
    private static readonly float[] qPrice = new float[] { 0.75f, 1, 1.25f, 1.5f, 2, 2.75f, 4, 8};
    private static readonly string[] qNames = new string[] { $"{charIgnore}", "Uncommon", "Rare", "Very Rare", "Super Rare", "Legendary", "Ultra Legendary", "Super Ultra Legendary" };
    private static readonly Color32[] qRenderOptions = new Color32[] {
        new Color32(255, 255, 255, 255), // White
        new Color32(144, 254, 132, 255), // Green
        new Color32(132, 140, 254, 255), // Blue
        new Color32(132, 222, 254, 255), // Cyan
        new Color32(215, 132, 254, 255), // Purple
        new Color32(254, 254, 132, 255), // Yellow
        new Color32(254, 181, 132, 255), // Orange
        new Color32(254, 132, 132, 255) // Red
    };
    public enum cQuality { Normal, Uncommon, Rare, VeryRare, SuperRare, Legendary, UltraLegendary, SuperUltraLegendary }
    [SerializeField] private cQuality quality = cQuality.Normal;


    private static readonly int[] mWeight = new int[] { 30, 70, 150, 500, 160, 80, 40, 1 };
    private static readonly float[] mPrice = new float[] { 0.25f, 0.5f, 0.75f, 1, 1.5f, 2, 4, 10 };
    private static readonly string[] mNames = new string[] { "Dirty", "Slimy", "Infested", $"{charIgnore}", "Foiled", "Gilded", "Chroma", "Ethereal" };
    public enum cMaterial { Dirty, Slimy, Infested, Normal, Foiled, Gilded, Chroma, Ethereal }
    [SerializeField] private cMaterial material = cMaterial.Normal;


    private static readonly int[] dWeight = new int[] { 1000, 300, 200, 100, 50, 25, 10, 1 };
    private static readonly float[] dPrice = new float[] { 1, 0.9f, 0.95f, 1.2f, 1.3f, 1.4f, 1.5f, 1.75f };
    private static readonly string[] dNames = new string[] { $"{charIgnore}", "Squashed", "Stretched", "Grown", "Shrunk", "Bloated", "Twirled", "Mirrored" };
    private static readonly Vector2[] dRenderOptions = new Vector2[]
    {
        new Vector2(1, 1),
        new Vector2(1.1f, 0.9f),
        new Vector2(0.9f, 1.1f),
        new Vector2(1.1f, 1.1f),
        new Vector2(0.9f, 0.9f),
        new Vector2(1, 1),
        new Vector2(1, 1),
        new Vector2(-1, -1)
    };
    public enum cDefect { Normal, Squashed, Stretched, Grown, Shrunk, Bloated, Hollowed, Mirrored }
    [SerializeField] private cDefect defect = cDefect.Normal;


    private static readonly int[] aWeight = new int[] { 750, 1 };
    private static readonly float[] aPrice = new float[] { 1, 100 };
    private static readonly string[] aNames = new string[] { $"{charIgnore}", "Anomaly" };
    [SerializeField] private bool anomaly = false;

    #region Constructor
    public Collectible(byte definitionID)
    {
        SetDefinition(definitionID);
        SetRandomAttributes();
    }
    public Collectible(Collectible collectible)
    {
        SetDefinition(collectible.GetDefinitionID());

        quality = collectible.GetQuality();
        material = collectible.GetMaterial();
        defect = collectible.GetDefect();
        anomaly = collectible.GetAnomaly();
    }
    public Collectible(uint binary)
    {
        SetDefinition((byte)Mathm.GetBinaryRange(binary, binary_definitionIndex, 8));

        quality = (cQuality)Mathm.GetBinaryRange(binary, binary_qualityIndex, 4);
        material = (cMaterial)Mathm.GetBinaryRange(binary, binary_materialIndex, 4);
        defect = (cDefect)Mathm.GetBinaryRange(binary, binary_defectIndex, 4);
        SetOtherFlagsFromInt((int)Mathm.GetBinaryRange(binary, binary_otherIndex, 4));
    }
    #endregion

    #region Attributes
    /// <summary>
    ///     Sets random attributes
    /// </summary>
    public void SetRandomAttributes()
    {
        SetRandomQuality();
        SetRandomMaterial();
        SetRandomDefect();
        SetRandomAnomaly();
    }

    /// <summary>
    ///     Sets random quality
    /// </summary>
    private void SetRandomQuality() { quality = (cQuality)GetRandomQuality(); }
    public static int GetRandomQuality() { return Randomu.GetWeightedIndex(qWeight); }
    /// <summary>
    ///     Sets random material
    /// </summary>
    private void SetRandomMaterial() { material = (cMaterial)Randomu.GetWeightedIndex(mWeight); }
    public static int GetRandomMaterial() { return Randomu.GetWeightedIndex(mWeight); }
    /// <summary>
    ///     Sets random defect
    /// </summary>
    private void SetRandomDefect() { defect = (cDefect)Randomu.GetWeightedIndex(dWeight); }
    public static int GetRandomDefect() { return Randomu.GetWeightedIndex(dWeight); }
    /// <summary>
    ///     Sets random anomaly
    /// </summary>
    private void SetRandomAnomaly() { anomaly = Randomu.GetWeightedIndex(aWeight) == 1; }
    public static int GetRandomAnomaly() { return Randomu.GetWeightedIndex(aWeight); }
    #endregion
    #region String Methods
    /// <summary>
    ///     Ouputs the Quality as a string
    /// </summary>
    /// <returns>string</returns>
    private string QualityToString() { return qNames[(int)quality]; }
    /// <summary>
    ///     Ouputs the Material as a string
    /// </summary>
    /// <returns>string</returns>
    private string MaterialToString() { return mNames[(int)material]; }
    /// <summary>
    ///     Ouputs the Defect as a string
    /// </summary>
    /// <returns>string</returns>
    private string DefectToString() { return dNames[(int)defect]; }
    /// <summary>
    ///     Ouputs the Anomaly as a string
    /// </summary>
    /// <returns>string</returns>
    private string AnomalyToString() { return aNames[anomaly ? 1 : 0]; }


    /// <summary>
    ///     Ouputs the Name  as a string ~ <Material> <Defect> <Quality> <CollectibleName> <Anomaly>
    /// </summary>
    /// <returns>string</returns>
    public string GetName()
    {
        // Check if our definition is set
        if (definition == null)
            return "No Definition Set";

        string cName = $"{MaterialToString()} {DefectToString()} {QualityToString()} {definition.GetName()} {AnomalyToString()}";
        // Replace ignore strings
        cName = cName.Replace($"{charIgnore} ", "");
        cName = cName.Replace($"{charIgnore}", "");
        // Return name
        return cName;
    }

    public override string ToString()
    {
        return GetName();
    }
    #endregion
    #region Render Options
    public Color32 GetQualityColor() { return qRenderOptions[(int)GetQuality()]; }
    public Vector2 GetDefectScale() { return dRenderOptions[(int)GetDefect()]; }
    #endregion
    #region Price
    public int GetPrice()
    {
        float modifier = qPrice[(int)quality] * mPrice[(int)material] * dPrice[(int)defect] * aPrice[anomaly ? 1 : 0];
        return Mathf.CeilToInt(definition.GetPrice() * modifier);
    }
    #endregion
    #region Get Methods
    public cQuality GetQuality() { return quality; }
    public cMaterial GetMaterial() { return material; }
    public cDefect GetDefect() { return defect; }
    public bool GetAnomaly() { return anomaly; }
    /// <summary>
    ///     For now returns anomaly as 1 if true or 0 if false
    /// </summary>
    /// <returns>Anomaly as int</returns>
    public int GetOtherFlagsAsInt()
    {
        return anomaly ? 1 : 0;
    }


    public CollectibleDefinition GetDefinition() { return definition; }
    public byte GetDefinitionID() { return definitionID; }
    public string GetDefinitionName() { return GetDefinition().GetName(); }
    public int GetDefinitionPrice() { return GetDefinition().GetPrice(); }
    public Sprite GetDefinitionSprite() { return GetDefinition().GetSprite(); }
    #endregion
    #region Set Methods
    private void SetDefinition(byte id)
    {
        definitionID = id;
        definition = CollectibleGenerator.Instance.GetDefinitionFromId(id);
    }
    /// <summary>
    ///     For now sets anomaly as 1 if true or 0 if false
    /// </summary>
    /// <returns>Anomaly as int</returns>
    public void SetOtherFlagsFromInt(int input)
    {
        anomaly = input == 1;
    }
    #endregion

    #region Static Conversions
    static readonly byte binary_definitionIndex = 0;
    static readonly byte binary_qualityIndex = 8;
    static readonly byte binary_materialIndex = 12;
    static readonly byte binary_defectIndex = 16;
    static readonly byte binary_otherIndex = 20;

    public static uint CollectibleToBinary(Collectible input)
    {
        uint binary = 0;

        Mathm.SetBinaryRange(binary_definitionIndex, input.GetDefinitionID(), ref binary); // Set Definition

        Mathm.SetBinaryRange(binary_qualityIndex, (uint)input.GetQuality(), ref binary); // Set Quality
        Mathm.SetBinaryRange(binary_materialIndex, (uint)input.GetMaterial(), ref binary); // Set Material
        Mathm.SetBinaryRange(binary_defectIndex, (uint)input.GetDefect(), ref binary); // Set Defect
        Mathm.SetBinaryRange(binary_otherIndex, (uint)input.GetOtherFlagsAsInt(), ref binary); // Set Other

        return binary;
    }
    public static uint CollectibleToBinary(byte definitionID, byte quality, byte material, byte defect, byte other)
    {
        uint binary = 0;

        Mathm.SetBinaryRange(binary_definitionIndex, definitionID, ref binary); // Set Definition

        Mathm.SetBinaryRange(binary_qualityIndex, quality, ref binary); // Set Quality
        Mathm.SetBinaryRange(binary_materialIndex, material, ref binary); // Set Material
        Mathm.SetBinaryRange(binary_defectIndex, defect, ref binary); // Set Defect
        Mathm.SetBinaryRange(binary_otherIndex, other, ref binary); // Set Other

        return binary;
    }
    #endregion
}
