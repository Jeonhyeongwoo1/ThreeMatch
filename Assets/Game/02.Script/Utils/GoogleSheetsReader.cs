using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

[Serializable]
public class Shop
{
    public int shopId;
    public int shopType;
    public int price;
    public int rewardType;
    public int rewardValue;
}

public class GoogleSheetsReader : MonoBehaviour
{
    // 스프레드시트 ID와 API 키를 변수로 설정
    private string spreadsheetId = "1ryvq__IhkvFjJF8Myu9cuvdFqfrQ3CctigMtgIBHS-g";
    private string apiKey = "AIzaSyD_6DeATDRre-hTEuJ4LLQPVFyOqfmUoXc";
    private string sheetName = "Shop"; // 시트 이름을 지정
    
    void Start()
    {
        // 코루틴 시작
        StartCoroutine(LoadGoogleSheetData());
    }

    private IEnumerator LoadGoogleSheetData()
    {
        // Google Sheets API URL 구성
        // string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{sheetName}?key={apiKey}";
        string url =
            $"https://sheets.googleapis.com/v4/spreadsheets/1ryvq__IhkvFjJF8Myu9cuvdFqfrQ3CctigMtgIBHS-g/values/Shop?key=AIzaSyD_6DeATDRre-hTEuJ4LLQPVFyOqfmUoXc";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // API 호출
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 응답 데이터를 JSON으로 파싱
                JObject json = JObject.Parse(request.downloadHandler.text);
                Debug.Log(json.ToString());
                JArray rows = (JArray)json["values"];
                List<Shop> dataList = JsonConvert.DeserializeObject<List<Shop>>(rows.ToString());

                List<Shop> list = new List<Shop>();
                foreach (JToken jToken in rows)
                {
                    Debug.Log(jToken);

                    // if (int.TryParse(row[0].ToString(), out int value))
                    // {
                    // }
                }
            }
            else
            {
                Debug.LogError("Failed to load data: " + request.error);
            }
        }
    }
}