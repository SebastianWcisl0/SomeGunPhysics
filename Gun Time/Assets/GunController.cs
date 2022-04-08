using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    //variables that don't change through out code
    [Header("Gun Settings")]
    public float fireRate = .1f;
    public int clipSize = 30;
    public int reservedAmmo = 270;
    public float laserDuration = .03f;

    //variables that change through out code
    bool canShoot;
    int currentAmmo;
    int ammoInReserve;

    //Muzzle Flash
    public Image muzzleFlashImage;
    public Sprite[] flashes;

    //Aiming (Zoom in, not 100% necessary)
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;

    public float aimSmoothing = 10;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 1;
    Vector2 currentRotation;
    public float weaponSwayAmount = -10;

    //weapon recoil (random)
    public bool randomizeRecoil;
    public Vector2 randomRecoilConstraints;

    private void Start()
    {
        currentAmmo = clipSize;
        ammoInReserve = reservedAmmo;
        canShoot = true;
    }

    private void Update()
    {

        DetermineRotation(); //looking around
        DetermineAim(); //again for zooming in
        if (Input.GetMouseButton(0) && canShoot && currentAmmo > 0) //if left mouse button, canshoot, and there is ammo
        {
            canShoot=false;
            currentAmmo--;
            StartCoroutine(ShootGun()); //coroutine runs off of "main thread" which means we can work with timers without freezing unity
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo < clipSize && ammoInReserve > 0)
        {
            int amountNeeded = clipSize - currentAmmo; //holds amount we can add to clip, ex: 30 - 27, so we need 3 bullets
            if(amountNeeded >= ammoInReserve) //is the amount we need is larger than what we have
            {
                currentAmmo += ammoInReserve;
                ammoInReserve -= amountNeeded;
            } 
            else //amount needed is less then reserve
            {
                currentAmmo = clipSize;
                ammoInReserve -= amountNeeded;
            }

        }
    }

    private void DetermineRotation()
    {
        Vector2 mouseAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseAxis *= mouseSensitivity;
        currentRotation += mouseAxis;

        currentRotation.y = Mathf.Clamp(currentRotation.y, -90, 90);

        transform.localPosition += (Vector3)mouseAxis * weaponSwayAmount / 1000; //weaponsway, not necessary

        transform.root.localRotation = Quaternion.AngleAxis(currentRotation.x, Vector3.up);
        transform.parent.localRotation = Quaternion.AngleAxis(-currentRotation.y, Vector3.right);


    }

    void DetermineAim() //for zooming in
    {
        Vector3 target = normalLocalPosition; //default
        if(Input.GetMouseButton(1)) //right mouse button change position
        {
            target = aimingLocalPosition;
        }

        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimSmoothing); //starting position, ending position, how long itll take to get there

        transform.localPosition = desiredPosition;
    }

    void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.1f;

        if (randomizeRecoil)
        {
            float xRecoil = Random.Range(-randomRecoilConstraints.x, randomRecoilConstraints.x); //random recoil
            float yRecoil = Random.Range(-randomRecoilConstraints.y, randomRecoilConstraints.y);

            Vector2 recoil = new Vector2(xRecoil, yRecoil); //determines recoid

            currentRotation += recoil; //change rotation by generated random recoil
        }

    }

    IEnumerator ShootGun() //necessary for coroutines
    {
        StartCoroutine(MuzzleFlash());
        //add a laser?
        DetermineRecoil();

        RayCastForEnemy(); // for enemy
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlashImage.sprite = flashes[Random.Range(0,flashes.Length)];
        muzzleFlashImage.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        muzzleFlashImage.sprite = null;
        muzzleFlashImage.color = new Color(0, 0, 0, 0);
    }

    void RayCastForEnemy()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, transform.parent.forward, out hit, 1 << LayerMask.NameToLayer("Enemy")))
        {
            try
            {
                Debug.Log("Hit enemy");
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(transform.parent.transform.forward * 500); //adds force away from player
            }
            catch
            {

            }
        }
    }


}
