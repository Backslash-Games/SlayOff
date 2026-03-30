using UnityEngine;

[System.Serializable]
public class CameraModifier
{
    public enum Axis { yaw, pitch, roll };
    [SerializeField] private Axis targetAxis;
    [SerializeField] private float influence;
    [SerializeField] private float reductionRate = 1;

    public CameraModifier(Axis targetAxis, float influence, float reductionRate)
    {
        this.targetAxis = targetAxis;
        this.influence = influence;
        this.reductionRate = reductionRate;
    }

    public Axis GetAxis() { return targetAxis; }
    public float GetInfluence() { return influence; }
    public void ReduceInfluence() { influence = Mathf.Lerp(influence, 0, Time.deltaTime * reductionRate); }
}
