using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ReulstInfo
{
    public string info;
    public string timer;
}


public class MainControlScript : MonoBehaviour
{

    public GameObject loginPanel;
    public GameObject loadingPanel;

    public GameObject timerObject;
    public GameObject textPanel;
    public InputField playerName;
    public Button submitButton;
    public Text loadingText;
    public Text messageText;
    private string pName = "";


    public GameObject timerPanel;
    public Text timerText;
    public InputField chatMessage;
    public Text locationText;

    public AudioSource newMessageSound;

    //public List<string> locations;
    //public List<double> longtitudes;
    //public List<double> latitudes;
    //public List<double> distances;

    public List<string> loginList;

    public OnlineMaps map;
    public OnlineMapsUIImageControl mapControl;


    float timerInt;
    bool firstEntry;

    void  Update()
    {
        if (timerInt > 0)
        {
            timerInt -= Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds((int)timerInt);


            timerText.text = time.Hours + ":" + time.Minutes + ":" + time.Seconds;
        }
        else
        {
            timerText.text = "00:00:00";
        }
    }

    // Use this for initialization
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Handheld.PlayFullScreenMovie("intro.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
        StartCoroutine(ShowPlayerInput());

        firstEntry = false;

        loginList.Add("Шериф");
        loginList.Add("Следователь");
        loginList.Add("Медэксперт");
        loginList.Add("Патрульный");
        loginList.Add("Бенджамин");
        loginList.Add("Маркс Доу");
        loginList.Add("Сесил");
        loginList.Add("Андрэ");
        loginList.Add("Мари Майн");
        loginList.Add("Репортер");
        loginList.Add("Фотограф");
        loginList.Add("Редактор");
        loginList.Add("Мэр");
        loginList.Add("Детектив");
        loginList.Add("Cумасшедшая");
        loginList.Add("Блогер");
        loginList.Add("Агент ФБР Он");
        loginList.Add("Агент ФБР Она");
        loginList.Add("Священник");
        loginList.Add("Отставной военный");
        loginList.Add("Кэсси");
        loginList.Add("Мэри");
        loginList.Add("Кэйт");
        loginList.Add("Мейбл");
        loginList.Add("Джуди");
        loginList.Add("kosru");
        loginList.Add("Nadinka");
        loginList.Add("Морфиус");
        loginList.Add("Извер");
        loginList.Add("Алекс");
        loginList.Add("МН1");
        loginList.Add("МН2");
        loginList.Add("МН3");
        loginList.Add("МН4");
        loginList.Add("МН5");



        FillLocationDatabase();

        mapControl.OnMapDrag += eventDragger;

    }

    void eventDragger()
    {
        DistanceToMarketCalc();
    }

    void DistanceToMarketCalc()
    {
        string candidate = "";
        double distance = double.MaxValue;
        Vector2 curDIST = Vector3.zero;

        for (int s = 0; s < map.markers.Length; s++)
        {

            if (map.markers[s].label == "Текущая Позиция")
            {
                curDIST = map.markers[s].position;
            }
        }

        for (int s = 0; s < map.markers.Length; s++)
        {

            if (map.markers[s].label == "Текущая Позиция")
            {
                continue;
            }

            double distanceS = OnlineMapsUtils.DistanceBetweenPointsD(map.markers[s].position, curDIST);

            if (distanceS * 1000 < distance)
            {
                distance = distanceS * 1000;
                candidate = map.markers[s].label;
            }



        }

        if (distance > 2)
        {
            locationText.text = candidate + " " + ((int)distance) + " " + " метров";
        }
        else
        {
            locationText.text = candidate;
        }
    }

    void FillLocationDatabase()
    {
        //locations.Add("Полицейское управление");
        //latitudes.Add(59.4780698);
        //longtitudes.Add(25.0188571);
        //distances.Add(100);

       // locations.Add("Maxima");
       // latitudes.Add(54.9332387);
        //longtitudes.Add(23.8871722);
        //distances.Add(100);
    }

    public void AppQuit()
    {
        Application.Quit();
    }

    public void SubmitPlayerName()
    {
        if (playerName.text.Length > 1 && loginList.Contains(playerName.text))
        {
            pName = playerName.text;
            loadingPanel.SetActive(true);
            loginPanel.SetActive(false);
            StartCoroutine(ServerContact());
        }
        else
        {
            playerName.text = "Нет в списке";
        }
    }

    IEnumerator ShowPlayerInput()
    {
        yield return new WaitForSeconds(0.1f);
        playerName.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
    }

    public void SendMessageToChat()
    {
        if (chatMessage.text.Length > 0)
        {
            var createuser_url = "http://urbanbaldai.lt/chatsubmit.php" + "?msg=" + System.Uri.EscapeUriString(chatMessage.text) + "&username=" + pName;
            var cu_get = new WWW(createuser_url);
            Debug.Log(createuser_url);
            chatMessage.text = "";
        }
    }

    IEnumerator ServerContact()
    {
        if (!Input.location.isEnabledByUser)
        {
            loadingText.text = "Функция GPS не включена?";
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            loadingText.text = "Функция GPS не смогла инициализироваться";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            loadingText.text = "Невозможно установить локацию";
            yield break;
        }
        else
        {

            while (true)
            {


                var createuser_url = "http://urbanbaldai.lt/datasubmit.php" + "?latitude=" + Input.location.lastData.latitude + "&longtitude=" + Input.location.lastData.longitude + "&altitude=" + Input.location.lastData.altitude + "&horizontalAccuracy=" + Input.location.lastData.horizontalAccuracy + "&timestamp=" + Input.location.lastData.timestamp + "&username=" + pName;
                var cu_get = new WWW(createuser_url);
                Debug.Log("quering: " + createuser_url);
                yield return cu_get;

                if (cu_get.error != null)
                {
                    loadingText.text = "Ошибка передачи данных " + cu_get.error;
                }
                else
                {

                    loadingText.gameObject.SetActive(false);
                    loadingPanel.SetActive(false);
                    textPanel.SetActive(true);
                    messageText.gameObject.SetActive(true);

                    string result = cu_get.text;
                    ReulstInfo stringArray = JsonUtility.FromJson<ReulstInfo>(result);

                    string lastMessage = messageText.text;

                    messageText.text = stringArray.info;

                    if (lastMessage != messageText.text && firstEntry)
                    {
                        newMessageSound.Play();
                    }

                    firstEntry = true;


                    timerInt = int.Parse(stringArray.timer);

                    if (timerInt > 0)
                    {
                        timerObject.SetActive(true);
                    }
                    else
                    {
                        timerObject.SetActive(false);
                    }


                    DistanceToMarketCalc();
                    yield return new WaitForSeconds(5f);
                }


            }

        }
    }
}


public class Coordinates
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
public static class CoordinatesDistanceExtensions
{
    public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
    {
        return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Kilometers);
    }

    public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates, UnitOfLength unitOfLength)
    {
        var baseRad = Math.PI * baseCoordinates.Latitude / 180;
        var targetRad = Math.PI * targetCoordinates.Latitude / 180;
        var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
        var thetaRad = Math.PI * theta / 180;

        double dist =
            Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
            Math.Cos(targetRad) * Math.Cos(thetaRad);
        dist = Math.Acos(dist);

        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;

        return unitOfLength.ConvertFromMiles(dist);
    }
}

public class UnitOfLength
{
    public static UnitOfLength Meters = new UnitOfLength(0.001609344);
    public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
    public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
    public static UnitOfLength Miles = new UnitOfLength(1);

    private readonly double _fromMilesFactor;

    private UnitOfLength(double fromMilesFactor)
    {
        _fromMilesFactor = fromMilesFactor;
    }

    public double ConvertFromMiles(double input)
    {
        return input * _fromMilesFactor;
    }
}