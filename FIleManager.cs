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
    public RawImage filePrefab; // Префаб, который будет создаваться для каждого файла
    public Transform parentTransform;
    private void Start()
    {
        // Получаем данные о подключении из статического класса
        string host = SSHConnectionData.host;
        int port = SSHConnectionData.port;
        string username = SSHConnectionData.username;
        string password = SSHConnectionData.password;

        // Создаем подключение SSH
        sshClient = new SshClient(host, port, username, password);
        sshClient.Connect();

        if (sshClient.IsConnected)
        {

            Debug.Log("Connected to SSH server.");

            // Выполняем команду ls на удаленном сервере для получения списка файлов в директории
            var command = sshClient.RunCommand("ls /root/Arsen_Test_2");
            string result = command.Result;

            // Разбиваем результат на строки, чтобы получить список файлов
            List<string> fileNames = new List<string>(result.Split("\n"));
           
            // Создаем префабы для каждого файла
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

        float paddingX = 10f; // Отступы по горизонтали
        float paddingY = 10f; // Отступы по вертикали

        for (int i = 0; i < fileNames.Count; i++)
        {
            string fileName = fileNames[i];
            Debug.Log(fileName + "\n");
            // Игнорируем пустые строки
            if (string.IsNullOrEmpty(fileName))
                continue;

            // Создаем префаб для файла
            RawImage newPrefab = Instantiate(filePrefab, parentTransform);

            // Находим TextMeshPro внутри префаба
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

            // Добавляем компонент RawImageTrigger, чтобы обрабатывать щелчки на RawImage
            RawImage rawImage = newPrefab.GetComponentInChildren<RawImage>();
            if (rawImage != null)
            {
                rawImage.gameObject.AddComponent<RawImageTrigger>();
            }

            // Добавляем компонент EventTrigger для обработки других событий
            EventTrigger eventTrigger = newPrefab.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = newPrefab.AddComponent<EventTrigger>();
            }

            // Добавляем обработчик для щелчка на префабе
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnPrefabClicked(newPrefab); }); // Вызываем метод OnPrefabClicked при щелчке
            eventTrigger.triggers.Add(entry);

            // Получаем размеры префаба
            RectTransform prefabRect = newPrefab.GetComponent<RectTransform>();
            Vector2 sizeDelta = prefabRect.sizeDelta;

            // Перемещаем префаб в соответствии с его размерами с учетом отступов
            prefabRect.anchoredPosition = new Vector2(-canvasRect.sizeDelta.x / 2 + sizeDelta.x / 2 + xOffset + paddingX,
                                                      canvasRect.sizeDelta.y / 2 - sizeDelta.y / 2 - yOffset - paddingY);

            // Обновляем координаты для следующего префаба
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

    // Метод, который вызывается при щелчке на созданном префабе
    private void OnPrefabClicked(RawImage prefab)
    {
        // Находим TextMeshPro внутри префаба
        TextMeshProUGUI textMeshPro = prefab.GetComponentInChildren<TextMeshProUGUI>();

        if (textMeshPro != null)
        {
            string fileName = textMeshPro.text;
            string remoteFilePath = "/root/Arsen_Test_2/" + fileName;
            string localFilePath = Path.Combine(Application.persistentDataPath, fileName);

            // Загружаем файл с удаленного сервера на локальный компьютер
            DownloadFileFromRemoteServer(remoteFilePath, localFilePath);

            // Открываем файл, если он был успешно загружен
            if (File.Exists(localFilePath))
            {
                Debug.Log("Opening file: " + fileName);
                // Открываем файл
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

    // Метод для загрузки файла с удаленного сервера на локальный компьютер
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
