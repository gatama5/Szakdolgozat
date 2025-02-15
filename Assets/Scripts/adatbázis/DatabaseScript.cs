using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class FirebaseManager : MonoBehaviour
{
    private static readonly string projectId = "szakdolgozat-bt0psl";
    private static readonly string databaseUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/";

    public static async Task SendData(string collection, string document, object data)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{databaseUrl}{collection}/{document}"; // Pl. "players/player1"
            string json = JsonConvert.SerializeObject(new { fields = ConvertToFirestoreFormat(data) });

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PatchAsync(url, content);

            Debug.Log(await response.Content.ReadAsStringAsync());
        }
    }

    private static object ConvertToFirestoreFormat(object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        var formatted = new Dictionary<string, object>();

        // Különféle típusok kezelése, hogy Firestore helyes adatokat kapjon
        foreach (var item in parsed)
        {
            if (item.Key == "name" || item.Key == "gender" || item.Key == "email")
            {
                formatted[item.Key] = new Dictionary<string, object> { { "stringValue", item.Value.ToString() } };
            }
            else if (item.Key == "age" || item.Key == "simon_says_score")
            {
                formatted[item.Key] = new Dictionary<string, object> { { "integerValue", Convert.ToInt32(item.Value) } };
            }
            else if (item.Key.StartsWith("hit_place_scores") || item.Key.StartsWith("hit_time_scores"))
            {
                formatted[item.Key] = new Dictionary<string, object> { { "stringValue", item.Value.ToString() } };
            }
            else if (item.Key == "maze_scr")
            {
                formatted[item.Key] = new Dictionary<string, object> { { "doubleValue", Convert.ToDouble(item.Value) } };
            }
        }

        return formatted;
    }

    // Tesztadatok a Start() függvényben
    async void Start()
    {
        var playerData = new
        {
            name = "Player1",
            age = 25,
            gender = "Male",
            email = "player1@example.com",
            simon_says_score = 2000,
            hit_place_scores1 = "A1",
            hit_place_scores2 = "B2",
            hit_place_scores3 = "C3",
            hit_place_scores4 = "D4",
            hit_place_scores5 = "E5",
            hit_time_scores1 = 12.34,
            hit_time_scores2 = 23.45,
            hit_time_scores3 = 34.56,
            hit_time_scores4 = 45.67,
            hit_time_scores5 = 56.78,
            maze_scr = 89.10
        };

        await SendData("players", "player1", playerData);
    }
}


//// Import the functions you need from the SDKs you need
//import { initializeApp } from "firebase/app";
//import { getAnalytics } from "firebase/analytics";
//// TODO: Add SDKs for Firebase products that you want to use
//// https://firebase.google.com/docs/web/setup#available-libraries

//// Your web app's Firebase configuration
//// For Firebase JS SDK v7.20.0 and later, measurementId is optional
//const firebaseConfig = {
//  apiKey: "AIzaSyCo6lL1zHnByub1-KJ0C3OaBGrMh7_aoWk",
//  authDomain: "szakdolgozat-bt0psl.firebaseapp.com",
//  databaseURL: "https://szakdolgozat-bt0psl-default-rtdb.firebaseio.com",
//  projectId: "szakdolgozat-bt0psl",
//  storageBucket: "szakdolgozat-bt0psl.firebasestorage.app",
//  messagingSenderId: "131715627260",
//  appId: "1:131715627260:web:ed9c7c8298a185e56dff79",
//  measurementId: "G-SB546ZF7V7"
//};

//// Initialize Firebase
//const app = initializeApp(firebaseConfig);
//const analytics = getAnalytics(app);



//npm install firebase