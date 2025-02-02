using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystemForMenu {

    public static void SaveDataFromMainMenu(MainMenuScript menu) {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/savedData.thing";
        FileStream stream = new FileStream(path, FileMode.Create);

        RetrievedMenuData data = new RetrievedMenuData(menu);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static RetrievedMenuData LoadDataFromMainMenu() {
        string path = Application.persistentDataPath + "/savedData.thing";
        if (File.Exists(path)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            RetrievedMenuData data = formatter.Deserialize(stream) as RetrievedMenuData;

            stream.Close();
            return data;
        }
        else {
            Debug.LogError("No save file oopsey whoopsey");
            return null;
        }
    }

}
