using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Mirror;
using UnityEngine.Rendering.PostProcessing;

public class movement : NetworkBehaviour {
    
    //The physics item attached to this
    public Rigidbody rb;
    public float acceleration = 150f;
    public float maxSpeed = 75f;
    public float minSpeed = 25f;
    public float drag = 60f;
    public float turnSpeed = 7f;
    public Transform player;
    public float velocity = 0;
	public Transform cameraParent;
	public float camZoom = 45;
	
	[Space] [Header("Particles")] public ParticleSystem trail;
	public ParticleSystem circle;
	public ParticleSystem barrel;
	public ParticleSystem stars;


	
	private PostProcessVolume postprocess;
	private Camera cam;
	private CinemachineVirtualCamera gameCam;
	private float tempDrag;
	private float tempAcceleration;
	private float tempMinSpeed;
	private float tempMaxSpeed;
    
    // Start is called before the first frame update
    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        player = gameObject.GetComponentInChildren<PlayerMovement>().transform;
		cam = Camera.main;
		cameraParent = transform.GetChild(0);
		postprocess = cam.GetComponent<PostProcessVolume>();
		gameCam = cameraParent.GetComponentInChildren<CinemachineVirtualCamera>();
			
		tempDrag = drag;
		tempAcceleration = acceleration;
		tempMinSpeed = minSpeed;

		if (!isLocalPlayer) {
			cameraParent.gameObject.SetActive(false);
		}

	}

    // Update is called once per frame
    void FixedUpdate() {
		if (!isLocalPlayer) { return; }
        rb.AddForce((player.forward * velocity) - rb.velocity, ForceMode.VelocityChange);
	}

	void Update() {
	if (!isLocalPlayer) { return; }
		float speedUp = Input.GetAxis("SpeedUp");
        float h = Input.GetAxis("Horizontal");
		
		tempMaxSpeed = maxSpeed;

		
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + (h * turnSpeed), 0);
		
		if (Input.GetButtonDown("Boost")) {
			Boost(true);
		}

		if (Input.GetButtonUp("Boost")) {
			Boost(false);
		}

		if (Input.GetButtonDown("Break")) {
			Break(true);
		}

		if (Input.GetButtonUp("Break")) {
			Break(false);
		}

		if (Input.GetButtonDown("TriggerL") || Input.GetButtonDown("TriggerR")) {
			Debug.Log(6);
			int dir = Input.GetButtonDown("TriggerL") ? -1 : 1;
			QuickSpin(dir);
		}

		if (Input.GetButton("Boost")) {
			velocity = velocity + (tempAcceleration * Time.deltaTime);
		}
        
        //Add some drag
        velocity = velocity - (tempDrag * Time.deltaTime);
        
        //Clamp velocity
        velocity = Mathf.Min(tempMaxSpeed, Mathf.Max(velocity, tempMinSpeed));
		
		DOVirtual.Float(gameCam.m_Lens.FieldOfView, camZoom, 1, SetCameraFOV);

		Debug.Log(tempMaxSpeed + " - " + tempDrag);
	}


	public void QuickSpin(int dir) {
		if (!DOTween.IsTweening(player)) {
			player.DOLocalRotate(
				new Vector3(player.localEulerAngles.x, player.localEulerAngles.y, 360 * -dir), .4f,
				RotateMode.LocalAxisAdd).SetEase(Ease.OutSine);
			barrel.Play();
		}
	}

	void SetCameraZoom(float zoom) {
		camZoom = zoom;
	}
	void SetCameraFOV(float zoom) {
		gameCam.m_Lens.FieldOfView = zoom;
	}

	void DistortionAmount(float x) {
		postprocess.profile.GetSetting<LensDistortion>().intensity.value = x;
	}

	void FieldOfView(float fov) {
		gameCam.m_Lens.FieldOfView = fov;
	}

	void Chromatic(float x) {
		postprocess.profile.GetSetting<ChromaticAberration>().intensity.value = x;
	}


	void Boost(bool state) {
		if (state) {
			gameObject.GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
			trail.Play();
			circle.Play();
		}
		else {
			trail.Stop();
			circle.Stop();
		}

		trail.GetComponent<TrailRenderer>().emitting = state;

		float origFov = state ? 40 : 55;
		float endFov = state ? 55 : 40;
		float origChrom = state ? 0 : 1;
		float endChrom = state ? 1 : 0;
		float origDistortion = state ? 0 : -30;
		float endDistorton = state ? -30 : 0;
		float starsVel = state ? -20 : -1;
		float zoom = state ? 70 : 45;

		DOVirtual.Float(origChrom, endChrom, .5f, Chromatic);
		DOVirtual.Float(origFov, endFov, .5f, FieldOfView);
		DOVirtual.Float(origDistortion, endDistorton, .5f, DistortionAmount);
		var pvel = stars.velocityOverLifetime;
		pvel.z = starsVel;
		tempDrag = state ? drag / 5 : drag;
		tempAcceleration = state ? acceleration * 5 : acceleration;
		//tempMaxSpeed = state ? maxSpeed * 2 : maxSpeed;

		
		SetCameraZoom(zoom);
	}

	void Break(bool state) {
		tempDrag = state ? drag * 5 : drag;
		tempMinSpeed = state ? minSpeed / 5 : minSpeed;
		float zoom = state ? 30 : 45;

		//DOVirtual.Float(dolly.m_Speed, speed, .15f, SetSpeed);
		SetCameraZoom(zoom);
	}    
    
    
}
