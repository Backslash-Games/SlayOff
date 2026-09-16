using HFHandyUtils.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnableObject_Attribute
{
    #region Tags
    public enum Tag
    {
        // ---> Group 00 <---
        [InspectorName("00/0 - Explosive")] Explosive,
        [InspectorName("00/1 - Bounce")] Bounce,
        [InspectorName("00/2 - Quaking")] Quaking,
        [InspectorName("00/3 - Split")] Split,
        [InspectorName("00/4 - Crumbly")] Crumbly,
        [InspectorName("00/5 - Dripping")] Dripping,
        [InspectorName("00/6 - Trail")] Trail,
        [InspectorName("00/7 - Grounded")] Grounded,
        [InspectorName("00/8 - Rolling")] Rolling,
        [InspectorName("00/9 - Pogo")] Pogo,

        // ---> Group 01 <---
        [InspectorName("01/10 - Effect")] Effect,
        [InspectorName("01/11 - Pierce")] Pierce,
        [InspectorName("01/12 - Incorporeal")] Incorporeal,
        [InspectorName("01/13 - Homing")] Homing,
        [InspectorName("01/14 - Boomerang")] Boomerang,
        [InspectorName("01/15 - Knockback")] Knockback,
        [InspectorName("01/16 - Inhaling")] Inhaling,
        [InspectorName("01/17 - Propulsion")] Propulsion,
        [InspectorName("01/18 - Giant")] Giant,
        [InspectorName("01/19 - Small")] Small,

        // ---> Group 02 <---
        [InspectorName("02/20 - Four Dimensional")] FourDimensional,
        [InspectorName("02/21 - Wiggly")] Wiggly,
        [InspectorName("02/22 - Loopy")] Loopy,
        [InspectorName("02/23 - Sporadic")] Sporadic,
        [InspectorName("02/24 - Weightless")] Weightless,
        [InspectorName("02/25 - Australian")] Australian,
        [InspectorName("02/26 - Heavy")] Heavy,
        [InspectorName("02/27 - Light")] Light,
        [InspectorName("02/28 - Fluttering")] Fluttering,
        [InspectorName("02/29 - Controlled")] Controlled,

        // ---> Group 03 <---
        [InspectorName("03/30 - Tunneling")] Tunneling,
        [InspectorName("03/31 - Frosty")] Frosty,
        [InspectorName("03/32 - Flaming")] Flaming,
        [InspectorName("03/33 - Noxious")] Noxious,
        [InspectorName("03/34 - Hazardous")] Hazardous,
        [InspectorName("03/35 - Noisy")] Noisy,
        [InspectorName("03/36 - Stiff")] Stiff,
        [InspectorName("03/37 - Quick")] Quick,
        [InspectorName("03/38 - Slow")] Slow,
        [InspectorName("03/39 - TimeSick")] TimeSick,

        // ---> Group 04 <---
        [InspectorName("04/40 - Orbital")] Orbital,
        [InspectorName("04/41 - Spiraling")] Spiraling,
        [InspectorName("04/42 - Heavenly")] Heavenly,
        [InspectorName("04/43 - Elastic")] Elastic,
        [InspectorName("04/44 - Digitized")] Digitized,
        [InspectorName("04/45 - Chained")] Chained,
        [InspectorName("04/46 - Linking")] Linking,
        [InspectorName("04/47 - Blooming")] Blooming,
        [InspectorName("04/48 - Powdered")] Powdered,
        [InspectorName("04/49 - Melting")] Melting,

        // ---> Group 05 <---
        [InspectorName("05/50 - Swelling")] Swelling,
        [InspectorName("05/51 - Slimy")] Slimy,
        [InspectorName("05/52 - Slick")] Slick,
        [InspectorName("05/53 - Bursting")] Bursting,
    }
    private static readonly Dictionary<Tag, SpawnableObject_AttributeLogic> _scripts = new Dictionary<Tag, SpawnableObject_AttributeLogic>()
    {
        // ---> Group 00 <---
        { Tag.Explosive, new SOAL_Explosive() }, 
        { Tag.Bounce, new SOAL_Bouncy() }, 
        { Tag.Quaking, new SOAL_Quake() }, 
        { Tag.Split, new SOAL_Split() }, 
        { Tag.Crumbly, new SOAL_Crumbly() }, 
        { Tag.Dripping, new SOAL_Drippy() }, 
        { Tag.Trail, null }, 
        { Tag.Grounded, new SOAL_Grounded() }, 
        { Tag.Rolling, new SOAL_Rolling() }, 
        { Tag.Pogo, new SOAL_Pogo() }, 
        
        // ---> Group 01 <---
        { Tag.Effect, null }, 
        { Tag.Pierce, null }, 
        { Tag.Incorporeal, null }, 
        { Tag.Homing, new SOAL_Homing() }, 
        { Tag.Boomerang, null }, 
        { Tag.Knockback, new SOAL_Knockback() }, 
        { Tag.Inhaling, new SOAL_Inhaling() }, 
        { Tag.Propulsion, new SOAL_Propulsion() }, 
        { Tag.Giant, null }, 
        { Tag.Small, null }, 
        
        // ---> Group 02 <---
        { Tag.FourDimensional, null }, 
        { Tag.Wiggly, new SOAL_Wiggly() }, 
        { Tag.Loopy, null }, 
        { Tag.Sporadic, null }, 
        { Tag.Weightless, null }, 
        { Tag.Australian, null }, 
        { Tag.Heavy, null }, 
        { Tag.Light, null }, 
        { Tag.Fluttering, null }, 
        { Tag.Controlled, null }, 
        
        // ---> Group 03 <---
        { Tag.Tunneling, null },
        { Tag.Frosty, null },
        { Tag.Flaming, null },
        { Tag.Noxious, null },
        { Tag.Hazardous, null },
        { Tag.Noisy, null },
        { Tag.Stiff, null },
        { Tag.Quick, null },
        { Tag.Slow, null },
        { Tag.TimeSick, null },
        
        // ---> Group 04 <---
        { Tag.Orbital, null },
        { Tag.Spiraling, null },
        { Tag.Heavenly, null },
        { Tag.Elastic, null },
        { Tag.Digitized, null },
        { Tag.Chained, null },
        { Tag.Linking, null },
        { Tag.Blooming, null },
        { Tag.Powdered, null },
        { Tag.Melting, null },
        
        // ---> Group 05 <---
        { Tag.Swelling, new SOAL_Swelling() },
        { Tag.Slimy, new SOAL_Slimy() },
        { Tag.Slick, new SOAL_Slick() },
        { Tag.Bursting, new SOAL_Bursting() },
    };
    #endregion
    #region Variables
    /// <summary>
    ///     Name for organizing attributes in inspector
    /// </summary>
    [SerializeField] private string source = "unknown";
    /// <summary>
    ///     Serialized tag
    /// </summary>
    public List<Tag> tags = new List<Tag>();
    #endregion

    #region Constructors
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="tag">Attribute Tag</param>
    /// <param name="value">Attribute value</param>
    public SpawnableObject_Attribute()
    {
        tags = new List<Tag>();
    }
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="tag">Attribute Tag</param>
    /// <param name="value">Attribute value</param>
    public SpawnableObject_Attribute(List<Tag> tags)
    {
        this.tags = tags;
    }
    #endregion

    #region Attribute Logic
    /// <summary>
    ///     Runs attribute on fixed update
    /// </summary>
    public void OnInitialization(SpawnableObject source)
    {
        foreach (var item in tags)
            GetLogic(item).OnInitialization(source);
    }
    /// <summary>
    ///     Runs attribute on fixed update
    /// </summary>
    public void OnFixedUpdate(SpawnableObject source)
    {
        foreach (var item in tags)
            GetLogic(item).OnFixedUpdate(source);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    public void OnImpact(SpawnableObject source)
    {
        foreach (var item in tags)
            GetLogic(item).OnImpact(source);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    public void OnImpactEntity(SpawnableObject source, EntityData entity)
    {
        foreach (var item in tags)
            GetLogic(item).OnImpactEntity(source, entity);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    public void OnBreak(SpawnableObject source)
    {
        foreach (var item in tags)
            GetLogic(item).OnBreak(source);
    }
    #endregion
    #region Get Methods
    /// <summary>
    ///     Pulls logic from the dictionary
    /// </summary>
    /// <returns>Logic</returns>
    public SpawnableObject_AttributeLogic GetLogic(Tag tag)
    {
        // If there is no key associated with the tag, return a new attribute
        if (!_scripts.ContainsKey(tag)) return new SpawnableObject_AttributeLogic();
        return _scripts[tag];
    }
    #endregion
}
