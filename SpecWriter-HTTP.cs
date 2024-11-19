using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using YamlDotNet.Core.Tokens;

namespace HardWarePickerBot
{
    internal class SpecWriter_HTTP
    {
        static public string? GSMarenaBotToken {get; set;}
        static public string? GSMarenaBotUrl {get; set;}
        static public TimeSpan timeout {get; set;} = new TimeSpan(0, 0, 10);
        static private HttpClient client = new HttpClient();
        DB_HTTP_worker db = new DB_HTTP_worker(); 
        public async Task<string>FindAndWriteSpecs(string name1) //интеграция с GsmArenaBot
        {
            client.Timeout = timeout;
            try
            {
                    var msg1 = new HttpRequestMessage(HttpMethod.Get, GSMarenaBotUrl);
                    msg1.Headers.Add("Authorization", GSMarenaBotToken);
                    msg1.Headers.Add("SPEC-QUERY", name1);
                    msg1.Headers.Add("SPEC-TYPE", "cameras");
                    var res = await client.SendAsync(msg1);
                    var content1 = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"[INFO] Получен ответ от GsmArenaBot: {cleanupSpec(content1.Replace("\n", ""))}");
                    string specs1 = cleanupSpec(content1.Replace("\n", ""));
                    client.Dispose();
                    return specs1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] ошибка при запросе в GsmArenaBot {ex.Message} | {ex.StackTrace}");
                    return string.Empty;
                }
        }

        private string cleanupSpec(string content1)
        {
            try
            {
                content1 = content1.Replace("\"", "");
                content1  = content1.Trim(']', '[');
                content1 = content1.Replace("(wide)", "(Ширик)").Replace("(ultrawide)", "(Ультраширик)").Replace("(telephoto)", "(Телевик)").Replace("(periscope telephoto)", "(Телевик-перископ)").Replace("\"]", "").Replace("u00b5", "u").Replace("u02da", "(Градусов)").Replace("u221e", "бесконечно").Replace("\\", "");
                return content1;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при очистке сообщения от GsmArenaBot {ex.Message}");
                return content1;
            }
        }
    }
}
    