using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody ))]
public class PlayerController : MonoBehaviour
{
    Rigidbody _myRigidBody;
    Vector3 _velocity;
    // Start is called before the first frame update
    void Start()
    {
        _myRigidBody = GetComponent<Rigidbody>();
    }


    public void Move(Vector3 velocity)
    {
        _velocity =velocity;
    }

    public void FixedUpdate()
    {
        _myRigidBody.MovePosition(_myRigidBody.position + _velocity * Time.fixedDeltaTime);
    }

    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
}
