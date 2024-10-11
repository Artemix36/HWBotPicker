using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HardWarePickerBot
{
    internal class SpecWriter_HTTP
    {
        static private HttpClient client = new HttpClient();
        DB_HTTP_worker db = new DB_HTTP_worker(); 
        public async Task<(string, string)>FindAndWriteSpecs(string name1, string name2) //интеграция с GsmArenaBot
        {
            string Spec1 = await db.GetCameraSpec(name1);
            string Spec2 = await db.GetCameraSpec(name2);
            Console.WriteLine($"[INFO] Найденные характеристики камер в БД:\nPhone 1 = {name1} Camera = {Spec1}\nPhone 2 = {name2} Camera = {Spec2}");
            
            if (Spec1.Contains("ERROR") || Spec2.Contains("ERROR"))
            {
                try
                {
                    string path = @"C:\token.txt";
                    try
                    {
                        string[] token = System.IO.File.ReadAllLines(path);//getting token

                        var url = "http://gsmarbot.ru/api/data";
                        var msg1 = new HttpRequestMessage(HttpMethod.Get, url);
                        msg1.Headers.Add("API-KEY", token[1]);
                        msg1.Headers.Add("SPEC-QUERY", name1);
                        msg1.Headers.Add("SPEC-TYPE", "cameras");
                        var res = await client.SendAsync(msg1);
                        var content1 = await res.Content.ReadAsStringAsync();
                        Console.WriteLine($"[INFO] Получен ответ от GsmArenaBot: {cleanupSpec(content1.Replace("\n", ""))}");
                        string specs1 = cleanupSpec(content1.Replace("\n", ""));
                        await db.AddSpec(name1, specs1);

                        if (name2 != null)
                        {
                            var msg2 = new HttpRequestMessage(HttpMethod.Get, url);
                            msg2.Headers.Add("API-KEY", token[1]);
                            msg2.Headers.Add("SPEC-QUERY", name2);
                            msg2.Headers.Add("SPEC-TYPE", "cameras");
                            var res2 = await client.SendAsync(msg2);
                            var content2 = await res2.Content.ReadAsStringAsync();
                            Console.WriteLine($"[INFO] Получен ответ от GsmArenaBot: {cleanupSpec(content2.Replace("\n", ""))}");
                            string specs2 = cleanupSpec(content2.Replace("\n", ""));
                            await db.AddSpec(name2, specs2);
                            return (specs1, specs2);
                        }
                        return (specs1, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] ошибка при запросе в GsmArenaBot {ex.Message} | {ex.StackTrace}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] ошибка при запросе в GsmArenaBot {ex.Message} | {ex.StackTrace}");
                }
            }
            return (Spec1, Spec2);
        }

        private string cleanupSpec(string content1)
        {
            content1 = content1.Replace("\"", "");
            content1  = content1.Trim(']', '[');
            content1 = content1.Replace("(wide)", "(Ширик)").Replace("(ultrawide)", "(Ультраширик)").Replace("(telephoto)", "(Телевик)").Replace("(periscope telephoto)", "(Телевик-перископ)").Replace("\"]", "").Replace("u00b5", "u").Replace("u02da", "(Градусов)").Replace("u221e", "бесконечно").Replace("\\", "");
            return content1;
        }
    }
}
    