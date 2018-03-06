using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour {

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 5f;
    [SerializeField]
    private float thrusterForce = 1000f;
    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    [SerializeField]
    private LayerMask environmentMask;


    [Header("Spring Settings")]
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxFloat = 40f;

    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    // Use this for initialization
    void Start () {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
	}
	
	// Update is called once per frame
	void Update () {

        if (PauseMenu.isOn)
        {
            return;
        }
        //setting target position for spring, set height of spring to new height
        RaycastHit _hit;
        if(Physics.Raycast(transform.position, Vector3.down, out _hit, 100f))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        } else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        //Calculate movement velocity as Vector3
        float _xMov = Input.GetAxis("Horizontal");
        float _zMov = Input.GetAxis("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        //Final movement Vector
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

        //animate movement
        animator.SetFloat("ForwardVelocity", -_zMov);

        //Apply movement
        motor.Move(_velocity);

        //Calculate rotation as 3D vector(turning around)
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

        //Apply rotation
        motor.Rotate(_rotation);

        //Calculate camer rota as 3D vector
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensitivity;

        //Apply rotation
        motor.RotateCamera(_cameraRotationX);

        Vector3 _thrusterForce = Vector3.zero;
        //Apply thruster force
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0){
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if(thrusterFuelAmount > 0.1f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }
            
        } else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;
            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0, 1);

        motor.ApplyThruster(_thrusterForce);
    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive {positionSpring = _jointSpring, maximumForce = jointMaxFloat };
    }

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }
}
