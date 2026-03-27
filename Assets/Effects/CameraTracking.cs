using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [SerializeField] private Camera other_camera;
    [SerializeField] private Camera this_camera;
    // Update is called once per frame
    void LateUpdate()
    {
        this_camera.fieldOfView = other_camera.fieldOfView;
    }
}
