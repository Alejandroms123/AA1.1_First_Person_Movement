using UnityEngine;

public class SP_Pickable : MonoBehaviour
{
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;

    private Rigidbody _rb;
    private Transform _holdPoint;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_holdPoint == null)
            return;

        Vector3 dir = _holdPoint.position - _rb.position;
        Vector3 force = dir * _acceleration - _rb.linearVelocity * _deceleration; _rb.AddForce(force, ForceMode.VelocityChange);
    }

    public void Interact(Transform holdPoint)
    {
        _holdPoint = holdPoint;
        Grab();
    }

    private void Grab()
    {
        _rb.useGravity = false;
    }

    public void Drop()
    {
        _holdPoint = null;
        _rb.freezeRotation = false;
        _rb.useGravity = true;
    }
}