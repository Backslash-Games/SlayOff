using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MonoSave : MonoBehaviour
{
    private bool encode = false;

    public string save_filePath;
    private bool pathSet = false;
    public string save_fileName;
    private string save_path = string.Empty;

    private bool validLoad = true;
    private int actionCount = 0;

    // Ensures all following calls are stable
    private void Validate()
    {
        SetFilePath();
        ValidateFile();
    }

    // Makes sure the game is looking in the right location
    private void SetFilePath()
    {
        // Check if the path has already been set
        if (pathSet)
            return;

        // Append the application path to the start of the file path
        save_filePath = Application.persistentDataPath + "/" + save_filePath;
        save_path = save_filePath + "/" + save_fileName;
        pathSet = true;
    }

    // Ensure the file path exists
    private void ValidateFile()
    {
        // Check if the path exists
        if (!Directory.Exists(save_filePath))
        {
            // If the path doesn't exist then make it
            Debug.LogWarning("MonoSave -> Creating directory " + save_filePath);
            Directory.CreateDirectory(save_filePath);
        }

        // Check if the file exists
        if (!File.Exists(save_path))
        {
            // If the file doesn't exists then make it
            Debug.LogWarning("MonoSave -> Creating file " + save_path);
            FileStream fs = File.Create(save_path);
            fs.Close();
        }
    }

    // Encode information
    string EncodeString(string value)
    {
        return value;
    }
    string DecodeString(string value)
    {
        return value;
    }

    // Save information
    public void Save()
    {
        Save(this);
    }
    public void Save(object value)
    {
        // First run validation methods
        Validate();

        if (!validLoad && !Application.isEditor)
        {
            Debug.LogError("MonoSave -> Last load was invalid, throwing early to perserve data");
            return;
        }

        // Send out a message that the file is being saved
        Debug.Log($"MonoSave ({actionCount}) -> Saving {value.GetType()}");

        // Pull the save data from the provided value
        string saveData = JsonUtility.ToJson(value, true);
        // Write information
        File.WriteAllText(save_path, EncodeString(saveData));

        // Increase the action count for debugging
        actionCount++;
    }

    // Load information
    public void Load(object value)
    {
        // First run validation methods
        Validate();

        // Set valid load to false
        validLoad = false;
        string loadData = DecodeString(File.ReadAllText(save_path));

        // Attempt to load
        try
        {
            JsonUtility.FromJsonOverwrite(loadData, value);
        }
        catch (Exception e)
        {
            Debug.LogError("MonoSave -> Load failed with execption...\n" + e);
            return;
        }

        // Send out a message that the file is being loaded
        Debug.Log($"MonoSave ({actionCount}) -> Loading {value.GetType()}");

        // Flag the load as valid
        validLoad = true;

        // Increase the action count for debugging
        actionCount++;
    }

    public virtual void BindInformation(object value) { }

    // Processing Methods
    private void Awake()
    {
        // Load handler data
        Debug.Log($"MonoSave -> Loading information for file with name {save_fileName}\n\nPath: {save_filePath}");
        Load(this);
        OnAwake();
    }
    public virtual void OnAwake() { }

    private void OnDisable()
    {
        // Save on close
        Save(this);
    }
    public virtual void Disable() { }
}
