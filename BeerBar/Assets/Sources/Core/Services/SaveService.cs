using Core.Save;
using UnityEngine;

namespace Core.Services
{
    public class SaveService : IService
    {
        public SaveData SaveData;

        public void Save()
        {
            SaveData.SaveParameters.BeforeSave();

            var json = JsonUtility.ToJson(SaveData, true);

            Debug.Log($"Saving data: {json}");

            PlayerPrefs.SetString("SaveData", json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            var json = PlayerPrefs.GetString("SaveData");

            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("No any saved data");
                SaveData = new SaveData();
                return;
            }

            SaveData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"Loaded data: {json}");
        }

        public T GetParameter<T>(string key) => SaveData.SaveParameters.GetValue<T>(key);

        public void SetParameter<T>(string key, T value) => SaveData.SaveParameters.SetValue(key, value);
    }
}