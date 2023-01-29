using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementInputController : MonoBehaviour
{
    public Vector2 _move;
    public Vector2 _look;

    private bool isGrounded;

    public float rotationPower = 3f;
    public float rotationLerp = 0.5f;

    public float speed = 1f;


    public GameObject followTransform;
    private CharacterController control;
    private float gravity = -9.8f;
    public LayerMask groundmask;
    private Vector3 v;
    private bool canRotate;

    public CinemachineVirtualCamera vcam;

    private Renderer[] renderers;

    private void Awake()
    {
        control = GetComponent<CharacterController>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnScroll(InputValue value)
    {
        var v2 = value.Get<Vector2>();
        var comp = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (v2.y >= 120 && comp.CameraDistance > 0)
        {
            comp.CameraDistance -= .5f;
        }
        if (v2.y <= -120 && comp.CameraDistance < 5)
        {
            comp.CameraDistance += .5f;
        }

        SetAlpha(Vector3.Distance(vcam.Follow.position, vcam.transform.position) / 3.0f);
    }

    public void OnLook(InputValue value)
    {
        _look = value.Get<Vector2>();
    }
    public void OnRotateView(InputValue value)
    {
        canRotate = value.isPressed;
    }

    public void SetAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var item = renderers[i];
            for (int j = 0; j < item.materials.Length; j++)
            {
                var mat = item.materials[j];

                mat.SetFloat("_Alpha", alpha);
                
            }
            if (alpha <= .19)
            {
                item.enabled = false;
            }
            else
            {
                item.enabled = true;
            }
        }
    }

    private void Update()
    {
        #region Player Based Rotation

        //Move the player based on the X input on the controller
        //transform.rotation *= Quaternion.AngleAxis(_look.x * rotationPower, Vector3.up);

        #endregion

        if (canRotate)
        {
            followTransform.transform.rotation *= Quaternion.AngleAxis(_look.x * rotationPower, Vector3.up);
            followTransform.transform.rotation *= Quaternion.AngleAxis(-_look.y * rotationPower, Vector3.right);

            var angles = followTransform.transform.localEulerAngles;
            angles.z = 0;

            var angle = followTransform.transform.localEulerAngles.x;

            //Clamp the Up/Down rotation
            if (angle > 180 && angle < 340)
            {
                angles.x = 340;
            }
            else if (angle < 180 && angle > 40)
            {
                angles.x = 40;
            }


            followTransform.transform.localEulerAngles = angles;

            //Set the player rotation based on the look transform
            transform.rotation = Quaternion.Euler(0, followTransform.transform.rotation.eulerAngles.y, 0);
            //reset the y rotation of the look transform
            followTransform.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }

        isGrounded = Physics.CheckSphere(transform.position, .2f, groundmask);

        if (isGrounded && v.y < 0)
        {
            v.y = -2f;
        }

        Vector3 move = transform.forward * speed * _move.y * Time.deltaTime +
            transform.right * speed * _move.x * Time.deltaTime;
        control.Move(move);

        v.y += gravity * Time.deltaTime;
        control.Move(v * Time.deltaTime);







    }


}
