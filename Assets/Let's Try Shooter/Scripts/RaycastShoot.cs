using UnityEngine;
using System.Collections;

public class RaycastShoot : MonoBehaviour {

	public int gunDamage = 1;
	public float fireRate = .25f; 	// Controls how often player can fire weapon
	public float weaponRange = 50f; // How far ray will cast into screen (50 units)
	public float hitForce = 100f; 	// When Raycast hits object with RigidBody component attached, we will apply this force
	public Transform gunEnd; 		// Marks position at end of gun where our laser line (ray cast) will begin

	private Camera fpsCam;
	private WaitForSeconds shotDuration = new WaitForSeconds(.07f); // Object determining time between shots
	private AudioSource gunAudio; // Holds audio source that we already have attached to gun to play sound when player fires
	private LineRenderer laserLine; // Takes array of two or more lines in 3D space and draws line between each one in game view
	private float nextFire; // Holds time of next time when player will be allowed to fire again after firing


	void Start () {
		laserLine = GetComponent<LineRenderer> ();
		gunAudio = GetComponent<AudioSource> ();
		fpsCam = GetComponentInParent<Camera> (); // Searches self first, then all parent hierarchy for requested component
	}


	// Most of our game logic goes here
	void Update () {
		if (Input.GetButtonDown ("Fire1") && Time.time > nextFire) 
		{
			nextFire = Time.time + fireRate;

			StartCoroutine (ShotEffect ());

			// We always want this to be at the center of our camera
			// ViewToWorldPoint takes a point in ViewPort space and converts it to world space
			// Viewport space is normalized from 0,0 to 1,1, and the Z-axis moves the point out into the frustum (pyramid with tip chopped off)
			Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
			RaycastHit hit;

			laserLine.SetPosition (0, gunEnd.position);

			// Now we will use physics.raycast to determine the endcast of our raycast
			// Later we will use physics.raycast to apply forces and deal damage
			// Physics.raycast returns boolean (hit == true)

			// out keyword allows us to store additional return info from a function
			if (Physics.Raycast (rayOrigin, fpsCam.transform.forward, out hit, weaponRange)) {
				// if we fire into the sky, our raycast will hit nothing
				// if we hit something with the raycast, set the second position of our line to hit.point
				laserLine.SetPosition (1, hit.point);

				// Check if the thing we hit had a ShootableBox attached
				ShootableBox health = hit.collider.GetComponent<ShootableBox> ();

				if (health != null) 
				{
					health.Damage (gunDamage);
				}
					
				if (hit.rigidbody != null) {
					// Hit.normal is emerging from the surface of the object we hit, in the direction of the protruding surface
					// -Hit.normal allows us to apply appropriate force
					hit.rigidbody.AddForce (-hit.normal * hitForce);
				}
			} 
			else 
			{
				laserLine.SetPosition (1, rayOrigin + (fpsCam.transform.forward * weaponRange));
			}
		}
	}

	// Co-routine
	private IEnumerator ShotEffect()
	{
		gunAudio.Play ();
		laserLine.enabled = true;
		// Yields to the WaitForSeconds object
		// yield indicates taht we will resume execution on the next line after the yield in the next frame
		// allows for frame-by-frame calc without having to finish an entire co-routine
		yield return shotDuration;
		laserLine.enabled = false;
	}
}
