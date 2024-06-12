using Opc.Ua.InputModels;
using Opc.Ua.OpcBrowser;
using Opc.Ua.OpcBrowser.ViewModels;
using Opc.Ua.OpcConnectionFactory;
using Opc.Ua.OpcConnectionFactory.Models;
using Opc.Ua.OpcUaReader;
using Opc.Ua.OpcUaReader.InputModels;
using Opc.Ua.OpcUaWriter;
using Renci.SshNet;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;
using Workstation.ServiceModel.Ua;
using Node = Opc.Ua.InputModels.Node;
namespace OpcUaProject.console
{
    public static class Program
    {
       
        private static string host = "89.42.142.199";
        private static int port = 22;
        private static string username = "root";
        private static string password = "sCDX6E6664zj";
        private static string path1 = "/root/Arsen_Test_2/test.txt";
        private static string path2 = "/root/Arsen_Test_2/test2.txt";
        private static string path3 = "/root/Arsen_Test_2/test3.txt";
        private const AttributeId attributeId = AttributeId.Value;

        private static OpcUaConnectionFactory channelFactory = new OpcUaConnectionFactory();

        private static OpcUaReader opcReader = new OpcUaReader(channelFactory);

        private static OpcUaWriter opcWritter = new OpcUaWriter(channelFactory);
        
        private static OpcBrowser opcBrowser = new OpcBrowser(channelFactory);
        public static async Task Main(string[] args)
        {
            var selectedSignals = new List<string>();
            var UidConnection = await ConnectToServer();
            var client = new SftpClient(host, port, username, password);
            client.Connect();
            while (true)
            {
                var fileContent = sftpRead(client, path1);
                var fileContent2 = sftpRead(client, path2);
                await opcWritter.WriteTag(UidConnection, "GT13.UF02.UT08.MK01.MG01.GKP1480.VA01.CP020", 1, fileContent);
                await opcWritter.WriteTag(UidConnection, "GT13.UF02.UT08.MK01.MG01.GKP1480.VA01.CP030", 1, fileContent2);
                Task.Delay(5000).Wait();
                var result = await NodeIdDownloadAsync(selectedSignals, UidConnection);
                var data = await NodeIdUploadAsync(selectedSignals, UidConnection);
                sftpSend(client, path3, string.Join("\n", data));
                selectedSignals.Clear();
            }
        }
        private static async Task<Guid> ConnectToServer()
        {
            var cim = new ConnectionInputModel("opc.tcp://10.13.33.6:62450",
                new AnonymousIdentity(),
                SecurityPolicy.None,
                ".\\certificate\\",
                "OpcUaReader");
            return await channelFactory.CreateUaChannel(cim);
        }
        private static string sftpRead(SftpClient client, string FilePath)
        {
            var fileStream = new MemoryStream();
            client.DownloadFile(FilePath, fileStream);
            fileStream.Position = 0;
            StreamReader reader = new StreamReader(fileStream);
            string fileContent = reader.ReadToEnd();
            return fileContent;
            
        }
        private static void sftpSend(SftpClient client, string FilePath, string message)
        {
            var fileStream = new MemoryStream();
            var writer = new StreamWriter(fileStream);
            writer.Write(message);
            writer.Flush();
            fileStream.Position = 0;

            client.UploadFile(fileStream, FilePath, true);
        }
        private static async Task<List<Browsed>> NodeIdDownloadAsync(List<string> selectedSignals, Guid UidConnection)
        {
            var result = await opcBrowser.Browse(UidConnection, new Node("GT13.UF02.UT08.MK01.MG01.GKP1480", 1));

            foreach (var node in result)
            {
                if ((node.TypeDefinition.ToString() == "i=63") && node.DisplayName.ToString().ToUpper().StartsWith("V"))
                {
                    selectedSignals.Add(node.NodeId.NodeId.Identifier.ToString());


                }

            }
            return result;
        }

        private static async Task<List<string>> NodeIdUploadAsync(List<string> selectedSignals, Guid UidConnection)
        {
            var data = new List<string>();
            foreach (var signal in selectedSignals)
            {
                var result = await opcReader.Read(UidConnection, attributeId, new Node(signal, 1));
                foreach (var node in result)
                {
                    data.Add(node.Variant.ToString());
                    
                }
            }
            
            return data;
            
        }

    }

}
