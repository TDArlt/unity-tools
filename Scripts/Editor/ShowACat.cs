using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class ShowACat : EditorWindow
{

    [MenuItem("Tools/Show me a cat &c", false, 120)]
    public static void Init()
    {
        ShowACat window = (ShowACat)GetWindow(typeof(ShowACat));
        window.minSize = new Vector2(400, 450);
        window.titleContent = new GUIContent("Cat image");
        window.Show();
    }

    private static Texture2D image;
    private static UnityWebRequest download;

    void OnGUI()
    {

        if (GUILayout.Button("Refresh!"))
        {
            download = null;
            image = null;
        }


        if (image != null)
            GUI.DrawTexture(new Rect(0, 20, 400, 400), image, ScaleMode.ScaleToFit);
        else
        {
            GUI.Label(new Rect(0, 20, 400, 400), "Waaaait for it....");

            if (download == null)
            {
                download = UnityWebRequestTexture.GetTexture("http://thecatapi.com/api/images/get?format=src&type=jpg");
                download.SendWebRequest();
            }

            if (download.isDone)
            {
                image = ((DownloadHandlerTexture)download.downloadHandler).texture;
            }
        }
    }
    

}