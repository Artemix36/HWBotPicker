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
using System.Net;
using ZstdSharp.Unsafe;
using HWPickerClassesLibrary;

namespace HardWarePickerBot
{
    internal class DB_HTTP_worker
    {
        CheckMessage checker = new CheckMessage();
        public static string? DBBaseURL {get; set;}
        public async Task<string> GetComparasignsAsync(string RequestedBy, int page) //все сравнения
        {
            Console.WriteLine($"[INFO] Обращение в БД за всеми сравнениями от {RequestedBy}..");
            try
            {
                HttpClient client = new HttpClient();
                var url = $"{DBBaseURL}/Comparasign/Get/All/{page}";
                var msg = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await client.SendAsync(msg);
                var content = await res.Content.ReadAsStringAsync();

                if(res.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return content.ToString().Trim('"');
                }
                if(res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось найти сравнения";
                }
                if(res.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось найти сравнения";
                }
                else
                {
                    Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                    client.Dispose();
                    return "[ERROR] Не обработанная ошибка";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ("[ERROR] Ошибка при обращении к БД");
            }
        }
        public async Task<string> GetComparasignByNameAsync(Comparasign comparasign) //по одному имени
        {
            Console.WriteLine($"[INFO] Обращение в базу данных для поиска сравнения по имени {comparasign.Phone1.Manufacturer} {comparasign.Phone1.Model} от {comparasign.AddedBy}..");
            if (comparasign.Phone1.Manufacturer != null || comparasign.Phone1.Manufacturer != "error"){
                try
                {
                    var url = $"{DBBaseURL}/Comparasign/Get";
                    HttpClient client = new HttpClient();
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(comparasign);
                    var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(url, request);
                    var content = await res.Content.ReadAsStringAsync();

                if(res.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return content.ToString().Trim('"');
                }
                if(res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось найти сравнения";
                }
                if(res.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось найти сравнения";
                }
                else
                {
                    Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                    client.Dispose();
                    return "[ERROR] Не обработанная ошибка";
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
        public async Task<string> GetComparasignByTwoNamesAsync(Comparasign comparasign) //по двум именам
        {
            Console.WriteLine($"[INFO] Обращение в базу данных для поиска сравнения по имени {comparasign.Phone1.Manufacturer} {comparasign.Phone1.Model} vs {comparasign.Phone2.Manufacturer} {comparasign.Phone2.Model} от {comparasign.AddedBy}..");
            if ((comparasign.Phone1.Manufacturer != null || comparasign.Phone1.Manufacturer != "error") && (comparasign.Phone2.Manufacturer != null || comparasign.Phone2.Manufacturer != "error"))
            {
                try
                {
                    var url = $"{DBBaseURL}/Comparasign/Get";
                    HttpClient client = new HttpClient();
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(comparasign);
                    var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(url, request);
                    var content = await res.Content.ReadAsStringAsync();

                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                        client.Dispose();
                        return content.ToString().Trim('"');
                    }
                    if(res.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                        client.Dispose();
                        return "[ERROR] Не получилось найти сравнения";
                    }
                    if(res.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                        client.Dispose();
                        return "[ERROR] Не получилось найти сравнения";
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                        client.Dispose();
                        return "[ERROR] Не обработанная ошибка";
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
            Console.WriteLine($"[INFO] Обращение в базу данных для записи нового сравнения {newComparasign.Phone1.Manufacturer} {newComparasign.Phone1.Model} VS {newComparasign.Phone2.Manufacturer} {newComparasign.Phone2.Model} от {newComparasign.AddedBy}...");
            try
            {
                var url = $"{DBBaseURL}/Comparasign/Add";
                HttpClient client = new HttpClient();
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(newComparasign);
                var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var res = await client.PutAsync(url, request);
                var content = await res.Content.ReadAsStringAsync();

                if(res.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return content.ToString().Trim('"');
                }
                if(res.StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return content.ToString().Trim('"');
                }
                if(res.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                    client.Dispose();
                    return content.ToString().Trim('"');
                }
                else
                {
                    Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                    client.Dispose();
                    return "[ERROR] Не обработанная ошибка";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при добавлении сравнениий {ex.Message} StackTrace {ex.StackTrace}");
                return "[ERROR] Ошибка при обращении к БД";
            }
        }
        public async Task<Phone> GetCameraSpec(Phone phone) //Получаем есть ли характеристики камеры в БД
        {
            Console.WriteLine($"[INFO] Обращение в БД для поиска Характеристик {phone.Manufacturer} {phone.Model}..");
            if (phone.Model != string.Empty)
            {
                try
                {
                    var url = $"{DBBaseURL}/Spec/Get";
                    HttpClient client = new HttpClient();
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(phone);
                    var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(url, request);
                    var content = await res.Content.ReadAsStringAsync();

                    if(res.StatusCode == HttpStatusCode.OK)
                    {
                        client.Dispose();
                        content = content.Trim('"');
                        content = Regex.Unescape(content);
                        content = content.Replace("\\", "");
                        Phone foundPhone = JsonConvert.DeserializeObject<Phone>(content);  
                        return foundPhone;
                    }
                    if(res.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                        client.Dispose();
                        return phone;
                    }
                    if(res.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                        client.Dispose();
                        return phone;
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                        client.Dispose();
                        return phone;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] ошибка при получении характеристик камер от БД " + ex.Message);
                    return (phone);
                }
            }
            else
            {
                return phone;
            }
        }
        public async Task<string> AddSpec(Phone phone) //Добавление характеристик камер в БД
        {
            Console.WriteLine($"[INFO] Обращение в БД для добавления характеристик камеры {phone.Manufacturer} {phone.Model}");
            try
            {
                var url = $"{DBBaseURL}/CameraSpec/Add";
                HttpClient client = new HttpClient();
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(phone);
                var request = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var res = await client.PutAsync(url, request);
                var content = await res.Content.ReadAsStringAsync();

                if(res.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {Regex.Unescape(content)}");
                    client.Dispose();
                    return content.ToString();
                }
                if(res.StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"[INFO] получен ответ от слоя БД: {res.StatusCode} {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось добавить характеристики камеры";
                }
                if(res.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"[ERROR] Внутренняя ошибка на стороне БД {res.StatusCode} Ответ: {content}");
                    client.Dispose();
                    return "[ERROR] Не получилось добавить характеристики камеры";
                }
                else
                {
                    Console.WriteLine($"[ERROR] Непонятная ошибка {res.StatusCode}");
                    client.Dispose();
                    return "[ERROR] Не обработанная ошибка";
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
