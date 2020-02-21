using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class MessageEntity
{
    public string messages;
    public int characterId;
}

public class JsonHelper
{
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

[Serializable]
public class CharacterInfo
{
    public int id;
    public string name;
    public string description;
}

[Serializable]
public class LocationEntity
{
	public int id;
	public string name;
	// public string description;
	public float latitude;
	public float longitude;
}



public class MainControlScript : MonoBehaviour
{
	private readonly String HOST_URL = "http://198.211.109.9:3001"; // "http://192.168.1.6:3001";

    public GameObject loginPanel;
    public GameObject loadingPanel;

    public GameObject textPanel;
    public InputField playerName;
    public Button submitButton;
    public Text loadingText;
    public Text messageText;
	public ScrollRect scrollView;
	public OnlineMaps map;

    private string currentCharacterName = "";
    private int currentCharacterId = -1;

    public InputField chatMessage;
    public Text locationText;

    public AudioSource newMessageSound;

    //public List<string> locations;
    //public List<double> longtitudes;
    //public List<double> latitudes;
    //public List<double> distances;

	public List<CharacterInfo> loginList = new List<CharacterInfo>();
	private List<OnlineMapsMarker> markers = new List<OnlineMapsMarker>();
	private string lastChatMessage = "";
	private string lastReceivedChatLine = "";

    public OnlineMapsUIImageControl mapControl;


	bool soundPlayed;

    void  Update()
    {

    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("\t\tStart");


        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Handheld.PlayFullScreenMovie("intro.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
        StartCoroutine(ShowPlayerInput());

        StartCoroutine(FillRoleList());


		messageText.text = "";

        mapControl.OnMapDrag += eventDragger;

    }

    IEnumerator FillRoleList() {
		
        using (UnityWebRequest cu_get = SendCharactersRequest())
        {
            CharacterInfo[] characters;
            yield return cu_get.SendWebRequest();
            if (cu_get.error != null)
            {
                Debug.Log("\t\t" + "Transmission error \n" + cu_get.error);
                loadingText.text = "Ошибка передачи данных \n" + cu_get.error;
                yield return new WaitForSeconds(5f);
            }
            else
            {
                string result = cu_get.downloadHandler.text;
                Debug.Log("CHARS RESULT: |" + result + "|");
                characters = JsonHelper.getJsonArray<CharacterInfo>(result);
                for (int i = 0; i < characters.Length; i++)
                {
                    loginList.Add(characters[i]);
                }
            }
        }
    }


//	IEnumerator FillLocationDatabase()
//	{
//		Debug.Log("\t\tFillLocationDatabase");
//		using (UnityWebRequest cu_get = SendLocationsRequest())
//		{
//			LocationEntity[] locations;
//			yield return cu_get.SendWebRequest();
//			if (cu_get.error != null)
//			{
//				Debug.Log("\t\t" + "Transmission error \n" + cu_get.error);
//				loadingText.text = "Ошибка передачи данных \n" + cu_get.error;
//				yield return new WaitForSeconds(5f);
//			}
//			else
//			{
//				string result = cu_get.downloadHandler.text;
//				Debug.Log("LOCATIONS RESULT: |" + result + "|");
//				locations = JsonHelper.getJsonArray<LocationEntity>(result);
//				markers = new List<OnlineMapsMarker>();
//
//				Debug.Log (map);
//				Debug.Log (OnlineMaps.instance);
//
//				for (int i = 0; i < locations.Length; i++)
//				{
//					Debug.Log ("Processing marker for " + locations [i].name);
//					OnlineMapsMarker marker = new OnlineMapsMarker ();
//					marker.SetPosition (locations [i].longitude, locations [i].latitude);
//					marker.label = locations [i].name;
//					markers.Add (marker);
//				}
//			}
//		}
//	}


    void eventDragger()
    {
        DistanceToMarkerCalc();
    }

    void DistanceToMarkerCalc()
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

    public void AppQuit()
    {
        Debug.Log("\t\tQuit");
        Application.Quit();
    }

    public void SubmitPlayerName()
    {
        Debug.Log("\t\tSubmitPlayerName");
        CharacterInfo currentCharacter = loginList.Find(item => item.name.Equals(playerName.text.ToString().Trim()));
        if (playerName.text.Length > 1 && currentCharacter != null)
        {
            currentCharacterName = currentCharacter.name;
            currentCharacterId = currentCharacter.id;
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
        Debug.Log("\t\tShowPlayerInput");
        yield return new WaitForSeconds(0.1f);
        playerName.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
    }

    public void SendMessageToChat()
    {
        Debug.Log("\t\tSendMessageToChat");
        if (chatMessage.text.Trim().Length > 0)
        {

            StartCoroutine(HandleMessageSending());
        }
    }

    IEnumerator HandleMessageSending() {
        using (UnityWebRequest chatReq = SendChatMessage(currentCharacterId, currentCharacterName, chatMessage.text))
        {
            yield return chatReq.SendWebRequest();

            if (chatReq.error != null)
            {
                Debug.Log("\t\t" + "Transmission error on chat \n" + chatReq.error);
                loadingText.text = "Ошибка передачи данных в чат \n" + chatReq.error;
            }
            else
            {
				lastChatMessage = chatMessage.text + "\n";
                string result = chatReq.downloadHandler.text;
                messageText.text = messageText.text + messageToChatLine(result);
                chatMessage.text = "";
				soundPlayed = false;
				ScrollChatDown ();
            }
        }
    }

	String messageToChatLine(string message) {
		string[] parts = message.Split(new String[] { "<#>" }, System.StringSplitOptions.None);
		String time = UnixTimeStampToDateTime(Convert.ToDouble(parts[2])).ToString("HH:mm:ss");

		return string.Format ("<color=grey>{0}</color> <b>{1}:</b> {2}\n", time, parts[0], parts[1].Trim('\n'));
	}

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

	void ScrollChatDown ()
	{
		scrollView.normalizedPosition = new Vector2 (0, 0);
	}

	void LocationUpdate() {
 
			Debug.Log ("\t\tLocation update round");
			// Debug.Log ("\t\t MAP == " + map);

			using (UnityWebRequest locReq = SendLocationsRequest ()) {
				LocationEntity[] locations;
				locReq.SendWebRequest ();
				while (!locReq.isDone) {
					
				}
				if (locReq.error != null) {
					Debug.Log ("\t\t" + "Transmission error for locations \n" + locReq.error);
					loadingText.text = "Ошибка передачи данных локаций \n" + locReq.error;
					return;
				} else {
					string result = locReq.downloadHandler.text;
					// Debug.Log ("LOCATIONS RESULT: |" + result + "|");
					locations = JsonHelper.getJsonArray<LocationEntity> (result);
					markers = new List<OnlineMapsMarker> ();

					for (int i = 0; i < locations.Length; i++) {
						// Debug.Log ("PROCESSING MARKER FOR " + locations [i].name);
						OnlineMapsMarker marker = new OnlineMapsMarker ();
						marker.SetPosition (locations [i].longitude, locations [i].latitude);
						marker.label = locations [i].name;
						markers.Add (marker);
					}


					for (int i = 0; i< map.markers.Length; i++)
					{

						if (map.markers[i].label != "Текущая Позиция")
						{
							map.RemoveMarker (map.markers [i], true);
						}
					}
				 
					Debug.Log ("ADDING LOCATION MARKERS");
					markers.ForEach (marker => map.AddMarker (marker));
				}
			}
	}

    IEnumerator ServerContact()
    {
        Debug.Log("\t\tServerContact");
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("\t\tGPS disabled");
            loadingText.text = "Функция GPS не включена?\nВключите и подождите.";
			yield return new WaitForSeconds(10);
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("\t\tGPS init round");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("\t\tCan't init gps");
            loadingText.text = "Функция GPS не смогла инициализироваться, давайте перезапустимся?";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("\t\tCan't get location");
            loadingText.text = "Невозможно узнать геолокацию.\n Подождем и попробуем еще.";
			yield return new WaitForSeconds(10);
        }
        else
        {
			int reqCount = 0;

            while (true)
            {

                Debug.Log("\t\t Data submit round");

				using (UnityWebRequest dataReq = SendDataRequest(currentCharacterId))
                {
                    yield return dataReq.SendWebRequest();

                    if (dataReq.error != null)
                    {
                        Debug.Log("\t\t" + "Transmission error for data \n" + dataReq.error);
                        loadingText.text = "Ошибка передачи данных \n" + dataReq.error;
                        yield return new WaitForSeconds(10f);
                    }
                    else
                    {
						loadingText.gameObject.SetActive(false);
						loadingPanel.SetActive(false);
						textPanel.SetActive(true);
						messageText.gameObject.SetActive(true);

                        string result = dataReq.downloadHandler.text;
						MessageEntity charMessages = JsonUtility.FromJson<MessageEntity>(result);
						if (charMessages.characterId == currentCharacterId && charMessages.messages.Length > 0) {
							List<string> chatLines = new List<string> ();
							string[] lines = charMessages.messages.Split (new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
							for (int i = 0; i < lines.Length; i++) {
								chatLines.Add (messageToChatLine (lines [i]));
							}
							Debug.Log ("GOT " + chatLines.Count + " MESSAGES");
							messageText.text = String.Join ("", chatLines.ToArray ());
							if (!chatLines [chatLines.Count - 1].EndsWith (lastChatMessage)
							    && (!soundPlayed || !chatLines [chatLines.Count - 1].Equals (lastReceivedChatLine))) {
								newMessageSound.Play ();
								soundPlayed = true;
								lastReceivedChatLine = chatLines [chatLines.Count - 1];
							}
							ScrollChatDown();
							if (reqCount % 10 == 0) {
								LocationUpdate ();
							}
							reqCount++;
						} else {
							Debug.Log ("THOSE ARE NOT OURS: " + charMessages.characterId + "|" + charMessages.messages.Length);
						}
                        DistanceToMarkerCalc();
                        yield return new WaitForSeconds(10f);
                    }
                }


            }

        }
    }

    private UnityWebRequest SendCharactersRequest()
    {
        return UnityWebRequest.Get(HOST_URL + "/api/characters");
    }

	private UnityWebRequest SendLocationsRequest()
	{
		return UnityWebRequest.Get(HOST_URL + "/api/locations");
	}


    private UnityWebRequest SendDataRequest(int characterId)
    {
        WWWForm form = new WWWForm();
        form.AddField("characterId", Convert.ToString(characterId));
        form.AddField("latitude", Convert.ToString(Input.location.lastData.latitude));
        form.AddField("longitude", Convert.ToString(Input.location.lastData.longitude));
        form.AddField("timestamp", Convert.ToString((int) Input.location.lastData.timestamp));

        return UnityWebRequest.Post(HOST_URL + "/api/coordinates/player/location", form);
    }

    private UnityWebRequest SendChatMessage(int characterId, string characterName, string message)
    {
        string processedMsg = characterName + "<#>" + message.Replace('"', '\'');
        string payload = "{\"type\": \"msg\", \"characterId\": " + Convert.ToString(characterId) + ", \"message\":\"" + processedMsg + "\"}";

        Debug.Log("CHAT PAYLOAD: |" + payload + "|");
        WWWForm form = new WWWForm();
        form.AddField("characterId", Convert.ToString(characterId));
        form.AddField("type", "msg");
        form.AddField("message", processedMsg);
        UnityWebRequest result = UnityWebRequest.Post(HOST_URL + "/api/message", form);
        return result;
    }
}
