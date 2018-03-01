using UnityEngine.Networking;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour {

    public PlayerWeapon weapon;

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private GameObject weaponGFX;
    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private LayerMask mask;

    private const string PLAYER_TAG = "Player";
	// Use this for initialization
	void Start () {
		if(cam == null)
        {
            Debug.LogError("PlayerShoot: no camera referance");
            this.enabled = true;
        }

        weaponGFX.layer = LayerMask.NameToLayer(weaponLayerName);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }
	}

    [Client]
    void Shoot()
    {
        RaycastHit _hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, weapon.range, mask))
        {
            if(_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name, weapon.damage);
            }
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
