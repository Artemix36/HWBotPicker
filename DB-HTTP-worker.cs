using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using static HWpicker_bot.Compare;
using Telegram.Bot.Types;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Mysqlx;

namespace HardWarePickerBot
{
    internal class DB_HTTP_worker
    {
        CheckMessage checker = new CheckMessage();
        public class CamSpec
        {
            public string nameOfPhone { get; set; }
            public string cameraSpec { get; set; }
        }
        static private HttpClient client = new HttpClient();
        public async Task<string> GetComparasignsAsync()
        {
            Console.WriteLine("[INFO] DB access..");
            try
            {
                var url = "http://localhost:5074/Comparasign/Get";
                var msg = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await client.SendAsync(msg);
                var content = await res.Content.ReadAsStringAsync();

                if (content.ToString().Length >= 15)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {content}");
                    return content.ToString();
                }
                else
                {
                    return "[ERROR] Не найдено";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ("[ERROR] Ошибка при обращении к БД");
            }
        } //все сравнения
        public async Task<string> GetComparasignByNameAsync(string name, string RequestedBy)
        {
            Console.WriteLine("[INFO] DB access..");
            if (name != null || name != "error"){
                try
                {
                    var url = $"http://localhost:5074/Comparasign/Get/{name},{RequestedBy}";
                    var msg = new HttpRequestMessage(HttpMethod.Get, url);
                    var res = await client.SendAsync(msg);
                    var content = await res.Content.ReadAsStringAsync();

                    if (content.ToString().Length >= 15)
                    {
                        Console.WriteLine($"[INFO] получен ответ от слоя БД: {content}");
                        return content.ToString();
                    }
                    else
                    {
                        return "[ERROR] Не найдено";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ("[ERROR] Ошибка при обращении к БД");
                }
            }
            else
            {
                return "Не найдено имя";
            }
        } //по одному имени
        public async Task<string> GetComparasignByTwoNamesAsync(string name1, string name2, string AddedBy)
        {
            Console.WriteLine("[INFO] DB access..");
            if (name1 != null && name2 != null)
            {
                try
                {
                    var url = $"http://localhost:5074/Comparasigns/Get/{name1},{name2},{AddedBy}";
                    var msg = new HttpRequestMessage(HttpMethod.Get, url);
                    var res = await client.SendAsync(msg);
                    var content = await res.Content.ReadAsStringAsync();

                    if (content.ToString().Length >= 15)
                    {
                        Console.WriteLine($"[INFO] получен ответ от слоя БД: {content}");
                        return content.ToString();
                    }
                    else
                    {
                        return "[ERROR] Не найдено";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении сравнений по двум именам {ex.Message}");
                    return ("[ERROR] Ошибка при обращении к БД");
                }
            }
            else
            {
                return "Не найдено имя";
            }
        }  //по двум именам
        public async Task<string> ComparasignAddAsync(Comparasign newComparasign)
        {
            Console.WriteLine("[INFO] DB access..");
            try
            {
                var url = $"http://localhost:5074/Comparasign/Add";
                var jsonObject = new
                {
                    Phone1Name = checker.FixPhoneNameToUpper(newComparasign.Phone1Name),
                    Phone2Name = checker.FixPhoneNameToUpper(newComparasign.Phone2Name),
                    CompLink = newComparasign.CompLink,
                    AddedBy = newComparasign.AddedBy,
                };
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
                var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var res = await client.PutAsync(url, request);
                var content = await res.Content.ReadAsStringAsync();

                if (content.ToString().Length >= 15)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {content}");
                    return content.ToString();
                }
                else
                {
                    return "[ERROR] Не найдено";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при добавлении сравнениий {ex.Message} StackTrace {ex.StackTrace}");
                return ("[ERROR] Ошибка при обращении к БД");
            }
        }  //добавить сравнение
        public async Task<string> GetCameraSpec(string name)
        {
            Console.WriteLine("[INFO] DB access..");
            if (name != null)
            {
                try
                {
                    var url = $"http://localhost:5074/CameraSpec/Get/{name}";
                    var msg = new HttpRequestMessage(HttpMethod.Get, url);
                    var res = await client.SendAsync(msg);
                    var content = await res.Content.ReadAsStringAsync();

                    if (content.ToString().Length >= 15)
                    {
                        //Console.WriteLine($"[INFO] получен ответ от слоя БД: {content}");
                        content = content.Trim('"');
                        //content = content.Trim('[', ']');
                        content = Regex.Unescape(content);
                        CamSpec[] camSpec = JsonConvert.DeserializeObject<CamSpec[]>(content);
                        return camSpec[0].cameraSpec.ToString();
                    }
                    else
                    {
                        return "[ERROR] Не найдено";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] ошибка при получении характеристик камер от БД " + ex.Message);
                    return ("[ERROR] Ошибка при обращении к БД");
                }
            }
            else
            {
                return null;
            }
        } //Получаем есть ли характеристики камеры в БД
        public async Task<string> AddSpec(string name, string specs)
        {
            try
            {
                var url = $"http://localhost:5074/CameraSpec/Add";
                var jsonObject = new
                {
                    NameOfPhone = name,
                    CameraSpec = specs,
                };
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
                var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var res = await client.PutAsync(url, request);
                var content = await res.Content.ReadAsStringAsync();

                if (content.ToString().Length >= 15)
                {
                    Console.WriteLine($"[INFO] DB answer: {Regex.Unescape(content)}");
                    return content.ToString();
                }
                else
                {
                    return "[ERROR] Не найдено";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при записи характеристик камер { ex.Message}");
                return ($"[ERROR] Ошибка при записи характеристик камер");
            } //добавить хар-ки камеры
        } //Добавление характеристик камер в БД
    }
}
