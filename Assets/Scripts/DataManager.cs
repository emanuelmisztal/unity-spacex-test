using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    // class storing data from web needed to calculate vector position
    private class OrbitalPosition
    {
        public double semi_major_axis_au; // semimajor axis
        public double eccentricity; // eccentricity
        public double inclination; // inclination
        public double longitude; // longitude
        public double periapsis_arg; // periapsis argument
        public double true_anomaly;

        // default constructor filling fields with zero
        public OrbitalPosition()
        {
            semi_major_axis_au = 0;
            eccentricity = 0;
            inclination = 0;
            longitude = 0;
            periapsis_arg = 0;
            true_anomaly = 0;
        }

        public OrbitalPosition(double _semi_major_axis_au, double _eccentricity, double _inclination, double _longitude, double _periapsis_arg, double _true_anomaly)
        {
            semi_major_axis_au = _semi_major_axis_au;
            eccentricity = _eccentricity;
            inclination = _inclination;
            longitude = _longitude;
            periapsis_arg = _periapsis_arg;
            true_anomaly = _true_anomaly;
        }
    }

    public delegate void ServerRefresh(); // define delegate collecting methods to refresh
    public ServerRefresh serverRefresh; // init delegate

    private OrbitalPosition roadsterOrbitalPosition; // stores orbital elements of tesla roadster
    private const string teslaURL = "https://api.spacexdata.com/v3/roadster"; // api link to tesla roadster from spacex
    private Dictionary<string, OrbitalPosition> orbitalPositionByDate; // store dictionary where key is date and value is true anomaly
    private bool CSVdataRead; // flag indicating csv data reading is finished
    private bool WebDataRead; // flag indicating web data reading is finished
    private float timer; // timer for counting to refresh

    // Start
    private void Start()
    {
        roadsterOrbitalPosition = new OrbitalPosition(); // init new orbital position storage
        CSVdataRead = false; // reset flag
        WebDataRead = false; // reset flag
        ReadCSVtoDictionary(); // read from csv to dictionary
        StartCoroutine(RefreshAllData()); // download data from spacex
        timer = 0; // reset timer
    }

    // Update
    private void Update()
    {
        if (timer < 610) timer += Time.deltaTime; // increment timer if less than 10min 10s
        else // timer over
        {
            StartCoroutine(RefreshAllData()); // start refreshing data
            timer = 0; // reset timer
        }
    }

    public bool GetCSVreadFlag() => CSVdataRead;

    // read date and true anomaly form csv
    private void ReadCSVtoDictionary()
    {
        TextAsset file = Resources.Load<TextAsset>("roadster"); // reference of csv file
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(file.text); // create byte array for text2bytes
        MemoryStream stream = new MemoryStream(byteArray); // create memory stream from previously created byte array
        orbitalPositionByDate = new Dictionary<string, OrbitalPosition>(); // create new empty dictionary for date and true anomaly

        using (var reader = new StreamReader(stream)) // we'll be reading from stream
        {
            reader.ReadLine(); // read first line cause it's labels
            while (!reader.EndOfStream) // untile there is data in stream
            {
                var values = reader.ReadLine().Split(','); // get line and split it
                if (!orbitalPositionByDate.ContainsKey(values[1]))
                    orbitalPositionByDate.Add(
                        values[1],
                        new OrbitalPosition(
                            double.Parse(values[2]),
                            double.Parse(values[3]),
                            double.Parse(values[4]),
                            double.Parse(values[5]),
                            double.Parse(values[6]),
                            double.Parse(values[8])
                            )); // if there is no such key in dictionary, create it
            }
        }

        CSVdataRead = true; // data has been read so set flag
    }

    // calculate vector position from orbital position
    public RG.OrbitalElements.Vector3Double GetRoadsterVectorPosition(int year, int month, int day)
    {
        string currentDate = year + "-" + month + "-" + day + " 00:00:00"; // get current date in specific format

        if (!orbitalPositionByDate.ContainsKey(currentDate)) currentDate = "2019-06-22 00:00:00"; // if there is no such date in dictionary keys set safe one
        
        RG.OrbitalElements.Vector3Double vector = RG.OrbitalElements.Calculations.CalculateOrbitalPosition( // create new vector3double with calculated values
            orbitalPositionByDate[currentDate].semi_major_axis_au, // parameter
            orbitalPositionByDate[currentDate].eccentricity, // parameter
            orbitalPositionByDate[currentDate].inclination, // parameter
            orbitalPositionByDate[currentDate].longitude, // parameter
            orbitalPositionByDate[currentDate].periapsis_arg, // parameter
            orbitalPositionByDate[currentDate].true_anomaly // true anomaly read from csv
            );

        return vector; // return calculated vector
    }

    // calculate vector position from orbital position
    public RG.OrbitalElements.Vector3Double GetRoadsterVectorPosition(int year, int month, int day, double _semi_major_axis, double _eccentricity, double _inclination, double _longitude, double _periapsis_arg)
    {
        string currentDate = year + "-" + month + "-" + day + " 00:00:00"; // get current date in specific format

        if (!orbitalPositionByDate.ContainsKey(currentDate)) currentDate = "2019-06-22 00:00:00"; // if there is no such date in dictionary keys set safe one

        RG.OrbitalElements.Vector3Double vector = RG.OrbitalElements.Calculations.CalculateOrbitalPosition( // create new vector3double with calculated values
            _semi_major_axis, // parameter
            _eccentricity, // parameter
            _inclination, // parameter
            _longitude, // parameter
            _periapsis_arg, // parameter
            orbitalPositionByDate[currentDate].true_anomaly // true anomaly read from csv
            );

        return vector; // return calculated vector
    }

    // get data from internet and parse it
    private IEnumerator GetTeslaRequest(string uri)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri); // set GET url

        yield return webRequest.SendWebRequest(); // request and wait for the desired page

        string[] pages = uri.Split('/'); // split url
        int page = pages.Length - 1; // get url length

        if (webRequest.isNetworkError) Debug.LogError(pages[page] + ": Error: " + webRequest.error); // an error occured
        else // an error did not occured
        {
            roadsterOrbitalPosition = JsonUtility.FromJson<OrbitalPosition>(webRequest.downloadHandler.text); // parse roadster orbital position data to object
            WebDataRead = true; // set flag to indicate done reading from web
        }
    }

    // return calculated roadster position in units 10^3 km or zero vector
    public RG.OrbitalElements.Vector3Double GetCurrentRoadsterVectorPosition()
    {
        return CSVdataRead && WebDataRead ? // if there is something to return, indicated by data read flags
            GetRoadsterVectorPosition(
                System.DateTime.Now.Year, // current year
                System.DateTime.Now.Month, // current month
                System.DateTime.Now.Day,  // current day
                roadsterOrbitalPosition.semi_major_axis_au, // semimajor axis from roadster orbital position
                roadsterOrbitalPosition.eccentricity, // eccentricity from roadster orbital position
                roadsterOrbitalPosition.inclination, // inclination from roadster orbital position
                roadsterOrbitalPosition.longitude, // longitude from roadster orbital position
                roadsterOrbitalPosition.periapsis_arg // periapsis argument from roadster orbital position
                ) // return it
            : 
            new RG.OrbitalElements.Vector3Double(0, 0, 0); // otherwise return default vector
    }

    // coroutine for refreshing all data
    private IEnumerator RefreshAllData()
    {
        CSVdataRead = false;
        WebDataRead = false;
        yield return StartCoroutine(GetTeslaRequest(teslaURL)); // wait wile getting data from web
        serverRefresh(); // call delegate methods
    }
}
