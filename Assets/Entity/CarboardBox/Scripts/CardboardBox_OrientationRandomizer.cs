using UnityEngine;

public class CardboardBox_OrientationRandomizer : MonoBehaviour
{
    private static readonly float[] rotations = new float[] { 0, 90, 180, 270 };

    private void Awake()
    {
        transform.localEulerAngles += Vector3.up * rotations[Random.Range(0, 4)];
    }
}
