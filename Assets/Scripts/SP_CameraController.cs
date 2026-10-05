using Unity.Cinemachine;
using UnityEngine;

public class SP_CameraController : MonoBehaviour
{
    [SerializeField] private float _mouseSens;

    private void Start()
    {
        CinemachineInputAxisController inputController =
            GetComponent<CinemachineInputAxisController>();

        if (inputController != null)
        {
            inputController.Controllers[0].Input.Gain = _mouseSens;
            inputController.Controllers[1].Input.Gain = -_mouseSens;
        }
    }
}