using UnityEngine;
using UnityEngine.InputSystem;

public class SP_PlayerGrabber : MonoBehaviour
{
    [SerializeField] LayerMask _interactable;
    [SerializeField] private float _interactDistance;

    [SerializeField] private float _zoomSens;
    [SerializeField] private float _zoomSpeed;
    [SerializeField] private float _zoomMinDistance;
    [SerializeField] private float _zoomMaxDistance;

    private SP_Pickable _currentPickable;
    private PlayerInput _playerInput;
    private InputAction _interactInput;
    private InputAction _zoomInput;
    private Camera _cam;

    private Vector3 _startPosition;


    private void Start()
    {
        _playerInput = FindAnyObjectByType<PlayerInput>();
        _cam = Camera.main;
        _startPosition = transform.localPosition;

        _interactInput = _playerInput.actions["Interact"];
        _zoomInput = _playerInput.actions["Zoom"];

        _interactInput.performed += GrabObject;
        _interactInput.canceled += DropObject;
    }

    private void Update()
    {
        if (_currentPickable != null)
        {
            float zoomValue = _zoomInput.ReadValue<float>();
            Vector3 targetPos = transform.localPosition;
            targetPos.z += zoomValue * _zoomSens * Time.deltaTime;
            targetPos.z = Mathf.Clamp(targetPos.z, _zoomMinDistance, _zoomMaxDistance);

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, _zoomSpeed);
        }
        else
        {
            transform.localPosition = _startPosition;
        }
    }

    private void GrabObject(InputAction.CallbackContext context)
    {
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, _interactDistance, _interactable))
        {
            _currentPickable = hit.collider.GetComponent<SP_Pickable>();

            if (_currentPickable != null)
                _currentPickable.Interact(transform);
        }
    }

    private void DropObject(InputAction.CallbackContext context)
    {
        if (_currentPickable != null)
        {
            _currentPickable.Drop();
            _currentPickable = null;
            transform.localPosition = _startPosition;
        }
    }
}