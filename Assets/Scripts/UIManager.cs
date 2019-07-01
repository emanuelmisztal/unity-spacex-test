using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private DataManager dataManager; // link to data manager
    private Text roadsterPositionText; // link to roadster position text

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindWithTag("Managers").GetComponent<DataManager>().serverRefresh += RefreshPositionText; // add refresher to delegate
        roadsterPositionText = GameObject.FindWithTag("UIroadsterpositiontext").GetComponent<Text>(); // find and link roadster position text
        dataManager = GameObject.FindObjectOfType<DataManager>().GetComponent<DataManager>(); // find and link data manager
    }

    // refresher
    public void RefreshPositionText() => roadsterPositionText.text = "Roadster position:\n" + dataManager.GetCurrentRoadsterVectorPosition();
}
