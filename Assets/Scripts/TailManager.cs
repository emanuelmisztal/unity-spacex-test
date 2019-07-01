using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailManager : MonoBehaviour
{
    private Queue<GameObject> positionQueue;
    private GameObject tailDotPrefab;
    private DataManager dataManager;
    private int year;
    private int month;
    private int day;

    // Start is called before the first frame update
    void Start()
    {
        positionQueue = new Queue<GameObject>(); // init new queue for tail objects
        dataManager = GameObject.FindWithTag("Managers").GetComponent<DataManager>(); // find and link data manager
        tailDotPrefab = Resources.Load<GameObject>("Prefabs/Tail Dot");
        dataManager.serverRefresh += RefreshTail;
        StartCoroutine(StartupWait()); // begin startup wait to retrive data
    }

    private IEnumerator StartupWait()
    {
        while (!dataManager.GetCSVreadFlag()) yield return new WaitForSeconds(1);

        RG.OrbitalElements.Vector3Double position;
        year = System.DateTime.Now.Year;
        month = System.DateTime.Now.Month;
        day = System.DateTime.Now.Day;

        for (int i = 0; i < 20; i++)
        {
            if (day == 1)
            {
                if (month == 1)
                {
                    year--;
                    month = 12;
                    day = 31;
                }
                else
                {
                    month--;
                    if (month == 2) day = 28;
                    else if (month % 2 == 1) day = 31;
                    else day = 30;
                }
            }
            else day--;

            positionQueue.Enqueue(GameObject.Instantiate<GameObject>(tailDotPrefab));
            position = dataManager.GetRoadsterVectorPosition(year, month, day);
            positionQueue.Peek().transform.position = new Vector3((float)position.x, (float)position.y, 0);
        }
    }

    private void RefreshTail()
    {
        RG.OrbitalElements.Vector3Double position;

        for (int i = 0; i < 20; i++)
        {
            if (day == 1)
            {
                if (month == 1)
                {
                    year--;
                    month = 12;
                    day = 31;
                }
                else
                {
                    month--;
                    if (month == 2) day = 28;
                    else if (month % 2 == 1) day = 31;
                    else day = 30;
                }
            }
            else day--;

            positionQueue.Enqueue(GameObject.Instantiate<GameObject>(tailDotPrefab));
            position = dataManager.GetRoadsterVectorPosition(year, month, day);
            positionQueue.Peek().transform.position = new Vector3((float)position.x * 2, (float)position.y * 0.000002f, 0);
        }
    }
}
