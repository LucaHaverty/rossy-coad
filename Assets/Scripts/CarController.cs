using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarController : MonoBehaviour
{

    [Header("Refs")]
    [SerializeField] private WheelCollider backLeft;
    [SerializeField] private WheelCollider backRight;
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;

    [SerializeField] private Transform backLeftTransform;
    [SerializeField] private Transform backRightTransform;
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform frontRightTransform;

    [SerializeField] private GameObject brakeLights;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text eggText;
    [SerializeField] private Rigidbody rb;

    [Space] [Header("Settings")]
    [SerializeField, Range(0, 400)] private float polloMultiplier = 200;
    [SerializeField, Range(0, 1)] private float boostMultiplier = .25f;
    [SerializeField, Range(0, 1500)] private float acceleration = 750f;
    [SerializeField, Range(0, 500)] private float breakingForce = 300f;
    [SerializeField, Range(0, 180)] private float maxTurnAngle = 15f;

    [SerializeField] private Vector3 carSpawn;
    
    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;
    private Quaternion wheelShift;
    private double speed;

    private void Start()
    {
        brakeLights.SetActive(false);
    }

    private void FixedUpdate()
    {
        currentAcceleration = acceleration * Input.GetAxisRaw("Vertical");
        speed = rb.velocity.magnitude * 3.6;

        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBreakForce;
        frontLeft.brakeTorque = currentBreakForce;
        backRight.brakeTorque = currentBreakForce;
        backLeft.brakeTorque = currentBreakForce;

        currentTurnAngle = maxTurnAngle * Input.GetAxisRaw("Horizontal");

        frontRight.steerAngle = currentTurnAngle;
        frontLeft.steerAngle = currentTurnAngle;

        UpdateWheel(frontRight, frontRightTransform, false);
        UpdateWheel(backRight, backRightTransform, false);

        UpdateWheel(frontLeft, frontLeftTransform, true);
        UpdateWheel(backLeft, backLeftTransform, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentBreakForce = breakingForce;
            brakeLights.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            currentBreakForce = 0f;
            brakeLights.SetActive(false);
        }
        
        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Delete))
            Respawn();

        speedText.text = Math.Round(speed).ToString();
    }

    void Respawn()
    {
        transform.position = carSpawn;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.velocity = Vector3.zero;
    }

    void Boost()
    {
        rb.AddForce(transform.forward * boostMultiplier * 3.6f, ForceMode.VelocityChange);
    }

    void UpdateWheel(WheelCollider col, Transform trans, Boolean left)
    {
        col.GetWorldPose(out Vector3 position, out Quaternion rotation);

        wheelShift = Quaternion.Euler(0, left ? 270 : 90, 0);
        Quaternion newRotation = rotation * wheelShift;

        trans.position = position;
        trans.rotation = newRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 carVel = rb.velocity;

        if (collision.gameObject.CompareTag("pollo"))
        {
            collision.transform.GetComponent<Rigidbody>().AddForce(carVel * polloMultiplier 
                                                                   + transform.up * (polloMultiplier/2*carVel.magnitude) );
            Vector3 chickenPos = collision.transform.position;
            
            Instantiate(Settings.instance.deathEffect, chickenPos, Quaternion.identity, collision.transform);
            Destroy(Instantiate(Settings.instance.featherEffect, chickenPos, Quaternion.identity, Settings.instance.effectsContainer), 2f);
            Destroy(collision.gameObject, 2f);
            Boost();
            PlayerData.instance.Eggs ++;
            EggUpdate();
        }
        if (collision.gameObject.CompareTag("coop"))
        {
            collision.transform.GetComponent<Rigidbody>().AddForce(carVel * polloMultiplier 
                                                                   + transform.up * (polloMultiplier/2*carVel.magnitude) );

            Vector3 coopPos = collision.transform.position;
            Destroy(Instantiate(Settings.instance.eggEffect, coopPos, Quaternion.identity, Settings.instance.effectsContainer), 2f);
            Destroy(collision.gameObject, 2f);
            PlayerData.instance.Eggs += 5;
            EggUpdate();
        }
    }

    private void EggUpdate()
    {
        eggText.text = PlayerData.instance.Eggs.ToString();
    }
}
