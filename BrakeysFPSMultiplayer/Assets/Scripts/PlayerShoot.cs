using UnityEngine.Networking;
using UnityEngine;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private PlayerWeapon currentWeapon;

    [SerializeField]
    private Camera cam;

    private WeaponManager weaponManager;

    [SerializeField]
    private LayerMask mask;

    private const string PLAYER_TAG = "Player";
	// Use this for initialization
	void Start () {
        weaponManager = GetComponent<WeaponManager>();
		if(cam == null)
        {
            Debug.LogError("PlayerShoot: no camera referance");
            this.enabled = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.isOn)
        {
            return;
        }
        if(currentWeapon == null)
        {
            return;
        }
        if(currentWeapon.fireRate <= 0)
        {
            //Debug.Log("Weapon found");
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        } else
        {
            if (Input.GetButton("Fire1"))
            {
                Debug.Log("ffffffffffffffff");
                InvokeRepeating("Shoot", 0f , currentWeapon.fireRate);
            } else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }

        
	}

    //called on server when player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //called on server when we hit sth 
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    //called on all clients muzzle flash e.g
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    //called on clients when hit sth
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffect, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }


    [Client]
    void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        //call on shoot method on server
        CmdOnShoot();
        RaycastHit _hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            if(_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            //We hit sth call the onhit on server
            CmdOnHit(_hit.point, _hit.normal);
            Debug.Log(_hit.point);
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage)
    {
        Debug.Log(_playerID + " has been shot");
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
        //GameObject.Find(_ID);
    }
}
