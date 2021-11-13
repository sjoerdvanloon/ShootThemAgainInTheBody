using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{

    public float _moveSpeed = 5;
    PlayerController _controller;
    GunController _gunController;
    Camera _viewCamera;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();

        _viewCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * _moveSpeed;
        _controller.Move(moveVelocity);

        // Look input
        Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            _controller.LookAt(point);
        }

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            _gunController.Shoot();
        }

    }
}
