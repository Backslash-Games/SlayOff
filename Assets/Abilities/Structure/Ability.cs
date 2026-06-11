using HFHandyUtils;
using System;
using UnityEngine;

public class Ability : MonoBehaviour, ITrigger
{
    [Header("Data")]
    [SerializeField] public Sprite icon;
    [SerializeField] public float cooldownTime = 1f;

    public void OnTrigger()
    {
        HFLogger.Log(name);
    }
}
