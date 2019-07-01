using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RG.OrbitalElements;

public class Roadster : MonoBehaviour
{
    private RG.OrbitalElements.Vector3Double vectorPosition; // current vector position
    private DataManager dataManager; // link to data manager

    // Start is called before the first frame update
    void Start()
    {
        dataManager = GameObject.FindObjectOfType<DataManager>().GetComponent<DataManager>(); // find and link data manager
        dataManager.serverRefresh += RefreshPosition; // add refresher to delegate
    }

    // refreshing roadster position
    private void RefreshPosition()
    {
        vectorPosition = dataManager.GetCurrentRoadsterVectorPosition(); // get vector position
        transform.position = new Vector3((float)vectorPosition.x * 2, 0, (float)vectorPosition.z) * 0.000002f; // transform position
    }
}
