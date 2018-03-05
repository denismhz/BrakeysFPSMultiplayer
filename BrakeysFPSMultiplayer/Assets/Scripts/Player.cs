using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    GameObject[] disableGameObjectsOnDeath;

    [SerializeField]
    private GameObject deathEvent;

    [SerializeField]
    private GameObject spawnEvent;

    private bool firstSetup = true;

    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIinstance.SetActive(true);
        }
        CmdBroadCastPlayerSetup();
    }

    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
            Debug.Log("setting player active");
        }

        for (int i = 0; i < wasEnabled.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        

        Collider _col = GetComponent<Collider>();
        if (_col != null) _col.enabled = true;

        GameObject _spawnGFX = Instantiate(spawnEvent, transform.position, Quaternion.Euler(-90f,0,0));
        Destroy(_spawnGFX, 3);

    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform _startPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _startPoint.position;
        transform.rotation = _startPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        SetupPlayer();
        Debug.Log(transform.name + " respawned");
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount)
    {
        if (isDead)
        {
            return;
        }
        currentHealth -= _amount;
        Debug.Log(transform.name + " now has " + currentHealth + " health");
        if(currentHealth <= 0)
        {

            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        
        //Disable COmponents
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }
        //disable gameobjects
       
        Collider _col = GetComponent<Collider>();
        if (_col != null) _col.enabled = false;

        GameObject _explosionGFX= Instantiate(deathEvent, transform.position, Quaternion.identity);
        Destroy(_explosionGFX, 3);

        Debug.Log(transform.name + " is Dead!");

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        //switch camera
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIinstance.SetActive(false);
        }

        //Call Respawn Method
        StartCoroutine(Respawn());
    }
 
    // Use this for initialization
    void Start () {
		
	}

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(999);
        }
    }

    [Command]
    private void CmdBroadCastPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }
        }
        firstSetup = false;
        SetDefaults();
    }
}
