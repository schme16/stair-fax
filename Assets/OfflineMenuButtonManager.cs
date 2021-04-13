using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OfflineMenuButtonManager : MonoBehaviour
{

    NetworkManager manager;

	void Awake() {
        manager = GetComponent<NetworkManager>();
    }

	public void HostGame() {
		Debug.Log("Hosting");
		manager.StartHost();
	}
	
	public void JoinGame() {
		Debug.Log("Joining");
		manager.StartClient();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
