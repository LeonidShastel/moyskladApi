using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RawPrint;
using RestSharp;
using RestSharp.Extensions;

namespace moyskladApi
{
    internal class Api
    {
        public List<Template> templates = new List<Template>();
        public List<Product> products = new List<Product>();
        private string urlTicket = "";
        public int selectedTemplate = 0;
        private string idOrder = "";
        public string selectedPrinter = "";
        MainWindow _MainWindow = (MainWindow)Application.Current.MainWindow;

        public string[] GetTemplate()
        {
            try
            {
                var client = new RestClient("https://online.moysklad.ru/api/remap/1.2/entity/assortment/metadata/customtemplate");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                request.AddHeader("Accept", "*/*");
                IRestResponse response = client.Execute(request);
                dynamic json = JObject.Parse(response.Content);
                string[] outpute = new string[json["rows"].Count];
                for (int i = 0; i < json["rows"].Count; i++)
                {
                    templates.Add(new Template(json["rows"][i]["name"].ToString(), json["rows"][i]["meta"].ToString()));
                    outpute[i] = json["rows"][i]["name"];
                }
                return outpute;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new string[0];
            }
        }
        private void GetId(string name)
        {
            try
            {
                _MainWindow.infoLabel.Content = "Поиск заказа по имени...";
                var client = new RestClient($"https://online.moysklad.ru/api/remap/1.2/entity/customerorder?search={name}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                IRestResponse response = client.Execute(request);
                dynamic api = JObject.Parse(response.Content);
                idOrder = api["rows"][0]["id"].ToString();
                if (idOrder.Length == 0)
                {
                    _MainWindow.infoLabel.Content = "Заказ не найден!";
                    throw new Exception("Заказ не найден");
                }
            }
            catch(Exception ex)
            {
                _MainWindow.infoLabel.Content = "Заказ не найден...";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void GetOwner(string name)
        {
            try
            {
                GetId(name);
                _MainWindow.infoLabel.Content = "Поиск товара в заказе...";
                var client = new RestClient($"https://online.moysklad.ru/api/remap/1.2/entity/customerorder/{idOrder}?expand=positions"); 
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                IRestResponse response = client.Execute(request);
                dynamic api = JObject.Parse(response.Content);
                urlTicket = api.attributes[0].value;
                for (int i = 0; i < api["positions"]["rows"].Count; i++)
                {
                    string id_product = api["positions"]["rows"][i]["id"].ToString();
                    string url = api["positions"]["rows"][i]["assortment"]["meta"]["href"].ToString();
                    int count = int.Parse(api["positions"]["rows"][i]["quantity"].ToString());
                    products.Add(new Product(id_product, url, count));
                }
                Directory.CreateDirectory("temp");
                SavePdf();
                SavePdfOfResponse();
                ChangedInfo();
                Print();
                Directory.Delete("temp", true);
                _MainWindow.infoLabel.Content = "Временная папка удалена. Документы отправлены на печать.";
            }
            catch (Exception ex)
            {
                _MainWindow.infoLabel.Content = "Ошибка товара в заказе...";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ChangedInfo()
        {
            try
            {
                _MainWindow.infoLabel.Content = "Изменение информации о заказе...";
                var client = new RestClient($"https://online.moysklad.ru/api/remap/1.2/entity/customerorder/{idOrder}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                request.AddHeader("Accept", "*/*");
                dynamic json = JObject.Parse(client.Execute(request).Content);
                string name = json["name"];
                name = (name[name.Length - 1] == ',' || name[name.Length - 1] == ' ') ? name.Substring(0, name.Length - 1): name;
                var body = @"{
" + "\n" +
@"    ""state"":{
" + "\n" +
@"        ""meta"":{
" + "\n" +
@"            ""href"": ""https://online.moysklad.ru/api/remap/1.2/entity/customerorder/metadata/states/9dc0676b-0f89-11eb-0a80-05a40006305a"",
" + "\n" +
@"            ""metadataHref"": ""https://online.moysklad.ru/api/remap/1.2/entity/customerorder/metadata"",
" + "\n" +
@"            ""type"": ""state"",
" + "\n" +
@"            ""mediaType"": ""application/json""
" + "\n" +
@"        }
" + "\n" +
@"    },
" + "\n" +
@"    ""name"": """+name+@"""
" + "\n" +
@"}";
                request = new RestRequest(Method.PUT);
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "*/*");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception ex)
            {
                _MainWindow.infoLabel.Content = "Ошибка в изменении информации о заказе!";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SavePdf()
        {
            try
            {
                _MainWindow.infoLabel.Content = "Сохранение PDF файла...";
                var client = new RestClient(urlTicket);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                byte[] response = client.DownloadData(request);
                File.WriteAllBytes("./temp/fieldurl.pdf", response);
            }
            catch (Exception ex)
            {
                _MainWindow.infoLabel.Content = "Ошибка в создании PDF файла!";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SavePdfOfResponse()
        {
            try
            {
                _MainWindow.infoLabel.Content = "Сохранение PDF файла...";
                foreach (Product pr in products)
                {
                    var client = new RestClient($"{pr.Url}/export");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Authorization", "Basic MUBtYWlsbWVkYWlseTpub3Bhc3M=");
                    request.AddHeader("Accept", "*/*");
                    var body = @"{
                " + "\n" +
                                    @"            ""organization"": {
                " + "\n" +
                                    @"              ""meta"": {
                " + "\n" +
                                    @"                ""href"": ""https://online.moysklad.ru/api/remap/1.2/entity/organization/9da8c74c-0f89-11eb-0a80-05a40006302d"",
                " + "\n" +
                                    @"                ""metadataHref"": ""https://online.moysklad.ru/api/remap/1.2/entity/organization/metadata"",
                " + "\n" +
                                    @"                ""type"": ""organization"",
                " + "\n" +
                                    @"                ""mediaType"": ""application/json"",
                " + "\n" +
                                    @"                ""uuidHref"": ""https://online.moysklad.ru/app/#mycompany/edit?id=9da8c74c-0f89-11eb-0a80-05a40006302d""
                " + "\n" +
                                    @"                }
                " + "\n" +
                                    @"            },
                " + "\n" +
                                    @"            ""count"": " + pr.Count + @",
                " + "\n" +
                                    @"            ""salePrice"": {
                " + "\n" +
                                    @"              ""priceType"": {
                " + "\n" +
                                    @"                ""meta"": {
                " + "\n" +
                                    @"                ""href"": ""https://online.moysklad.ru/api/remap/1.2/context/companysettings/pricetype/9dad2cdc-0f89-11eb-0a80-05a400063035"",
                " + "\n" +
                                    @"                ""type"": ""pricetype"",
                " + "\n" +
                                    @"                ""mediaType"": ""application/json""
                " + "\n" +
                                    @"                }
                " + "\n" +
                                    @"              }
                " + "\n" +
                                    @"            },
                " + "\n" +
                                    @"            ""template"": {
                " + "\n" +
                                    @"              ""meta"": " + templates[selectedTemplate].Meta + @"
                " + "\n" +
                                    @"            }
                " + "\n" +
                                    @"          }";
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    byte[] buffer = client.DownloadData(request);
                    MemoryStream ms = new MemoryStream(buffer);
                    FileStream file = new FileStream(@".\temp\" + pr.Id + ".pdf", FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                    file.Close();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                _MainWindow.infoLabel.Content = "Ошибка в создании PDF файла!";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Print()
        {
            _MainWindow.infoLabel.Content = "Создание очереди печати";
            foreach (string name in Directory.GetFiles("./temp"))
            {
                string Filepath = name;
                string Filename = name.Substring(7);
                IPrinter printer = new Printer();
                printer.PrintRawFile(selectedPrinter, Filepath, Filename);
            }
        }
    }
}

