using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Threading;
using UnityEngine.Windows;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
public class SFTP : MonoBehaviour
{
    private string ServerFilePath = "/root/Arsen_Test_2/test.txt";
    private string ServerFilePath2 = "/root/Arsen_Test_2/test2.txt";
    private string ServerFilePath3 = "/root/Arsen_Test_2/test3.txt";
    private string directory = "/root/Arsen_Test_2";

    public  TMP_InputField host;
    public  TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField message;
    public TMP_InputField message2;
    public TMP_Text ServMsg;
    public GameObject SetButton;
    
    private SftpClient client;

    private void ConnectToSftp()
    {
        try
        {
            client = new SftpClient(host.text, 22, username.text, password.text);
            client.Connect();
            if (client.IsConnected)
            {
                StartCoroutine(Get());
            }
        }
        catch (Exception ex)
        {
            Debug.Log("An error occurred: " + ex.Message);
            Disconnect(); // При возникновении ошибки закрываем соединение
        }
    }

    public void Connect()
    {
        SSHConnectionData.host = host.text; 
        SSHConnectionData.port = 22;
        SSHConnectionData.username = username.text;
        SSHConnectionData.password = password.text;
        ConnectToSftp();
    }

    public void Set()
    {
        ConnectToSftp();
        try
        {
            if (client.IsConnected)
            {
                Debug.Log("Connected");
            }
            Upload(message.text, ServerFilePath);
            Upload(message2.text, ServerFilePath2);
        }
        catch (Exception ex)
        {
            Debug.Log("An error occurred: " + ex.Message);
        }
        finally
        {
            Disconnect(); // Всегда закрываем соединение после использования
        }
    }


    public void NextScene()
    {
        Disconnect();
        Debug.Log("disconnected");
        SceneManager.LoadScene("2");
    }


    public IEnumerator Get()
    {
        while (true)
        {
            try
            {
                if (!client.IsConnected)
                {
                    ConnectToSftp();
                }
                ServMsg.text = Download(ServerFilePath3);
            }
            catch (Exception ex)
            {
                Debug.Log("An error occurred: " + ex.Message);
            }
            yield return new WaitForSeconds(3);
        }
    }

    private void Upload(string msg, string filePath)
    {
        using (var fileStream = new MemoryStream())
        using (var writer = new StreamWriter(fileStream))
        {
            writer.Write(msg);
            writer.Flush();
            fileStream.Position = 0;
            client.UploadFile(fileStream, filePath, true);
        }
        Debug.Log("Message '" + msg + "' sent successfully.");
    }

    private string Download(string filePath)
    {
        using (var fileStream = new MemoryStream())
        {
            client.DownloadFile(filePath, fileStream);
            fileStream.Position = 0;
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    private void Disconnect()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }
    }
}
public static class SSHConnectionData
{
    public static string host;
    public static int port;
    public static string username;
    public static string password;
}