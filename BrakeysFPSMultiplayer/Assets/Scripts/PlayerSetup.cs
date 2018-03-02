using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";

    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;
    private GameObject playerUIinstance;

    Camera sceneCamera;

    void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        } else
        {
            sceneCamera = Camera.main;
            if(sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }

            //Disable playerGraphiics for localPlayer
            SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            //Create playerUI
            playerUIinstance = Instantiate(playerUIPrefab);
            playerUIinstance.name = playerUIPrefab.name;

            //configutre player ui
            PlayerUI ui = playerUIinstance.GetComponent<PlayerUI>();
            if(ui == null)
            {
                Debug.LogError("noi player ui component on playerui prefab");
            }

            ui.SetController(GetComponent<PlayerController>());
        }

        GetComponent<Player>().Setup();
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    private void OnDisable()
    {
        Destroy(playerUIinstance);
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }

        GameManager.UnRegisterPlayer(transform.name);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();
        GameManager.RegisterPlayer(_netID, _player);
    }
}
