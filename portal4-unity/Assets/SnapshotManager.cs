using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;


[Serializable]
public class NftStorage
{
    public string ok;
    public NftStorageValue value;
    public NftStorageLinks links;
}


[Serializable]
public class NftStorageValue
{
    public string cid;
}

[Serializable]
public class NftStorageLinks
{
    public string ipfs;
}


public class SnapshotManager : MonoBehaviour
{
    // Start is called before the first frame update

    public TextMeshPro ipfsText;
    public TextMeshPro hashText;

    private static string NFT_STORAGE_PRIVATE_KEY =
        "XXXXXXXXXXXXXXX";

    private static string NFT_STORAGE_URL = "https://nft.storage/api/upload";
    // void OnEnable()
    // {
    //     RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    // }
    // void OnDisable()
    // {
    //     RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    // }
    // private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    // {
    //     OnPostRender();
    // }

    void Start()
    {
        Debug.Log("Let's Go");
        hashText.text = "";
        ipfsText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Let's Snap");
            grab = true;
        }
    }


    private static bool grab = false;
    private static bool inProgress = false;

    [DllImport("__Internal")]
    private static extern void CreateNFT( string name, string cid, string snapHash, string apikey);

    [DllImport("__Internal")]
    private static extern void SnapshotAlert();

    private void OnPostRender()
    {
        
        // Debug.Log("OnPostRender");
        snap();
    }

    private void snap()
    {
        if (grab && inProgress == false)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
           SnapshotAlert();
#endif
            inProgress = true;
            grab = false;
            Debug.Log("Snap !!");

            int width = Screen.width;
            int height = Screen.height;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.LoadRawTextureData(texture.GetRawTextureData());
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            Object.Destroy(texture);

            var snapName = DateTime.Now.ToString("ddMMyyhhmmss", CultureInfo.CurrentCulture);
            StartCoroutine(sendTexture(snapName, bytes));
        }
        else
        {
            grab = false;
        }
    }

    public static UnityWebRequest POST(string uri, byte[] bodyData) => new UnityWebRequest(uri, "POST",
        (DownloadHandler) new DownloadHandlerBuffer(), (UploadHandler) new UploadHandlerRaw(bodyData));


    IEnumerator sendTexture(string snapName, byte[] data)
    {
        SHA256 sha256 = SHA256.Create();
        // StringBuilder builder = new StringBuilder();
        // for (int i = 0; i < shaBin.Length; i++)
        // {
        //     builder.Append(shaBin[i].ToString("x2"));
        // }
        //
        // var computeHash = builder.ToString();

#if UNITY_EDITOR
        File.WriteAllBytes(Application.dataPath + "/../" + snapName + ".png", data);
#endif
        StringBuilder sb = new StringBuilder();
        var computeHash = sha256.ComputeHash(data);
        foreach (Byte b in computeHash)
        {
            sb.Append(b.ToString("X2"));
        }

        var snapHash = sb.ToString();
        Debug.Log("HASH : " + snapHash);

        UnityWebRequest post = POST(NFT_STORAGE_URL, data);

        post.SetRequestHeader("Authorization", "Bearer " + NFT_STORAGE_PRIVATE_KEY);

        yield return post.SendWebRequest();

        if (post.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(post.error);
        }
        else
        {
            Debug.Log(post.responseCode);
            var downloadHandlerText = post.downloadHandler.text;
            Debug.Log(downloadHandlerText);
            NftStorage nftStorage = JsonUtility.FromJson<NftStorage>(downloadHandlerText);
            var cid = nftStorage.value.cid;
            ipfsText.text = "ipfs://" + cid;
            hashText.text = "HASH: " + snapHash;

#if UNITY_WEBGL && !UNITY_EDITOR
            CreateNFT(snapName, nftStorage.value.cid, snapHash,NFT_STORAGE_PRIVATE_KEY);
#endif
            Debug.Log("Finished Uploading Screenshot");
        }

        inProgress = false;
    }
}