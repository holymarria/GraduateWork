using Renci.SshNet.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class FIleManager : MonoBehaviour
{
    private SshClient sshClient;
    public RawImage filePrefab; // ������, ������� ����� ����������� ��� ������� �����
    public Transform parentTransform;
    private void Start()
    {
        // �������� ������ � ����������� �� ������������ ������
        string host = SSHConnectionData.host;
        int port = SSHConnectionData.port;
        string username = SSHConnectionData.username;
        string password = SSHConnectionData.password;

        // ������� ����������� SSH
        sshClient = new SshClient(host, port, username, password);
        sshClient.Connect();

        if (sshClient.IsConnected)
        {

            Debug.Log("Connected to SSH server.");

            // ��������� ������� ls �� ��������� ������� ��� ��������� ������ ������ � ����������
            var command = sshClient.RunCommand("ls /root/Arsen_Test_2");
            string result = command.Result;

            // ��������� ��������� �� ������, ����� �������� ������ ������
            List<string> fileNames = new List<string>(result.Split("\n"));
           
            // ������� ������� ��� ������� �����
            CreatePrefabs(fileNames);
        }
        else
        {
            Debug.Log("Failed to connect to SSH server.");
        }
    }
    private void CreatePrefabs(List<string> fileNames)
    {
        RectTransform canvasRect = parentTransform.GetComponent<RectTransform>();

        float xOffset = 0f;
        float yOffset = 0f;
        float maxWidth = 0f;

        float paddingX = 10f; // ������� �� �����������
        float paddingY = 10f; // ������� �� ���������

        for (int i = 0; i < fileNames.Count; i++)
        {
            string fileName = fileNames[i];
            Debug.Log(fileName + "\n");
            // ���������� ������ ������
            if (string.IsNullOrEmpty(fileName))
                continue;

            // ������� ������ ��� �����
            RawImage newPrefab = Instantiate(filePrefab, parentTransform);

            // ������� TextMeshPro ������ �������
            TextMeshProUGUI textMeshPro = newPrefab.GetComponentInChildren<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = fileName;
                Debug.Log(textMeshPro.text);
            }
            else
            {
                Debug.Log("TextMeshPro component not found in the prefab.");
            }

            // ��������� ��������� RawImageTrigger, ����� ������������ ������ �� RawImage
            RawImage rawImage = newPrefab.GetComponentInChildren<RawImage>();
            if (rawImage != null)
            {
                rawImage.gameObject.AddComponent<RawImageTrigger>();
            }

            // ��������� ��������� EventTrigger ��� ��������� ������ �������
            EventTrigger eventTrigger = newPrefab.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = newPrefab.AddComponent<EventTrigger>();
            }

            // ��������� ���������� ��� ������ �� �������
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnPrefabClicked(newPrefab); }); // �������� ����� OnPrefabClicked ��� ������
            eventTrigger.triggers.Add(entry);

            // �������� ������� �������
            RectTransform prefabRect = newPrefab.GetComponent<RectTransform>();
            Vector2 sizeDelta = prefabRect.sizeDelta;

            // ���������� ������ � ������������ � ��� ��������� � ������ ��������
            prefabRect.anchoredPosition = new Vector2(-canvasRect.sizeDelta.x / 2 + sizeDelta.x / 2 + xOffset + paddingX,
                                                      canvasRect.sizeDelta.y / 2 - sizeDelta.y / 2 - yOffset - paddingY);

            // ��������� ���������� ��� ���������� �������
            xOffset += sizeDelta.x + paddingX;
            maxWidth = Mathf.Max(maxWidth, sizeDelta.x);
            if (xOffset + maxWidth > canvasRect.sizeDelta.x)
            {
                xOffset = 0f;
                yOffset += sizeDelta.y + paddingY;
                maxWidth = 0f;
            }
        }
    }

    // �����, ������� ���������� ��� ������ �� ��������� �������
    private void OnPrefabClicked(RawImage prefab)
    {
        // ������� TextMeshPro ������ �������
        TextMeshProUGUI textMeshPro = prefab.GetComponentInChildren<TextMeshProUGUI>();

        if (textMeshPro != null)
        {
            string fileName = textMeshPro.text;
            string remoteFilePath = "/root/Arsen_Test_2/" + fileName;
            string localFilePath = Path.Combine(Application.persistentDataPath, fileName);

            // ��������� ���� � ���������� ������� �� ��������� ���������
            DownloadFileFromRemoteServer(remoteFilePath, localFilePath);

            // ��������� ����, ���� �� ��� ������� ��������
            if (File.Exists(localFilePath))
            {
                Debug.Log("Opening file: " + fileName);
                // ��������� ����
                Application.OpenURL("file://" + localFilePath);
            }
            else
            {
                Debug.LogWarning("Failed to download file: " + fileName);
            }
        }
        else
        {
            Debug.LogWarning("TextMeshPro component not found in the prefab.");
        }
    }

    // ����� ��� �������� ����� � ���������� ������� �� ��������� ���������
    private void DownloadFileFromRemoteServer(string remoteFilePath, string localFilePath)
    {
        using (var client = new SftpClient(SSHConnectionData.host, 22, SSHConnectionData.username, SSHConnectionData.password)) 
        {
            client.Connect();

            if (client.IsConnected)
            {
                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    client.DownloadFile(remoteFilePath, fileStream);
                }
                client.Disconnect();
            }
        }
    }

private void OnDestroy()
    {
        if (sshClient != null && sshClient.IsConnected)
        {
            sshClient.Disconnect();
        }
    }


}
