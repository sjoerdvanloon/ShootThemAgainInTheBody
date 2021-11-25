using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{

    public float MoveSpeed = 5;
    public Crosshairs Crosshairs;
    public float AimThresold = 4f;

    PlayerController _controller;
    GunController _gunController;
    Camera _viewCamera;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

    }

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();

        _viewCamera = Camera.main;
        var spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        _health = StartingHealth;
        var gun = _gunController.AllGuns[waveNumber-1];
        _gunController.EquipGun(gun);
    }

    // Update is called once per frame
    void Update()
    {
        // Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * MoveSpeed;
        _controller.Move(moveVelocity);

        // Look input
        Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * _gunController.GunHeight); // On the height of the kun
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            _controller.LookAt(point);
            Crosshairs.transform.position = point;
            Crosshairs.DetectTargets(ray);

            var cursorFromPlayer = (new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.y)).magnitude;
            if (cursorFromPlayer > AimThresold)
            {
                _gunController.Aim(point);
            }

        }

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            _gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _gunController.Reload();
        }

        if (Input.anyKeyDown)
        {
            if (int.TryParse(Input.inputString, out int numberPressed))
            {
                if (numberPressed <= _gunController.AllGuns.Length)
                {
                    var gun = _gunController.AllGuns[numberPressed - 1];
                    _gunController.EquipGun(gun);
                }
            }
        }


    }
}
