using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

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

    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for(int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }
        SetDefaults();
    }

    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }
        Collider _col = GetComponent<Collider>();
        if (_col != null) _col.enabled = true;

    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        SetDefaults();
        Transform _startPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _startPoint.position;
        transform.rotation = _startPoint.rotation;
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
        Collider _col = GetComponent<Collider>();
        if (_col != null) _col.enabled = false;

        Debug.Log(transform.name + " is Dead!");

        //Call Respawn Method
        StartCoroutine(Respawn());
    }
 
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	//void Update () {
 //       if (!isLocalPlayer)
 //       {
 //           return;
 //       }
 //       if (Input.GetKeyDown(KeyCode.K))
 //       {
 //           RpcTakeDamage(999);
 //       }
	//}
}
