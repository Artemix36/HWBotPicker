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
        public async Task<string> GetComparasignsAsync(string RequestedBy) //все сравнения
        {
            Console.WriteLine($"[INFO] Обращение в БД за всеми сравнениями от {RequestedBy}..");
            try
            {
                var url = $"http://localhost:5074/Comparasign/Get/{RequestedBy}";
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
        public async Task<string> GetComparasignByNameAsync(string name, string RequestedBy) //по одному имени
        {
            Console.WriteLine($"[INFO] Обращение в базу данных для поиска сравнения по имени от {RequestedBy}..");
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
        } 
        public async Task<string> GetComparasignByTwoNamesAsync(string name1, string name2, string AddedBy) //по двум именам
        {
            Console.WriteLine($"[INFO] Обращение в базу данных для получения сравнения по двум именам от {AddedBy}..");
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
        }
        public async Task<string> ComparasignAddAsync(Comparasign newComparasign) //добавить сравнение
        {
            Console.WriteLine($"[INFO] Обращение в базу данных для записи нового сравнения от {newComparasign.AddedBy}...");
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
                return "[ERROR] Ошибка при обращении к БД";
            }
        }
        public async Task<string> GetCameraSpec(string name) //Получаем есть ли характеристики камеры в БД
        {
            Console.WriteLine("[INFO] Обращение в БД для поиска Характеристик Камер..");
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
        }
        public async Task<string> AddSpec(string name, string specs) //Добавление характеристик камер в БД
        {
            Console.WriteLine($"[INFO] Обращение в БД для добавления характеристик камеры {name}");
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
            } 
        }
    }
}
