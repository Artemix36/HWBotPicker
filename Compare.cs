using HardWarePickerBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramApi;
using System.Runtime.Serialization;
using static System.Net.Mime.MediaTypeNames;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace HWpicker_bot
{
    internal class Compare
    {
        public class Comparasign
        {
            public Phone Phone1 {get; set;} = new Phone();
            public Phone Phone2 {get; set;} = new Phone();
            public string CompareLink { get; set; } = string.Empty;
            public string AddedBy { get; set; } = string.Empty;
        }

        CheckMessage checker = new CheckMessage();
        DB_HTTP_worker db = new DB_HTTP_worker();
        TGAPI tg = new TGAPI();
        SpecWriter_HTTP specWriter_HTTP = new SpecWriter_HTTP();

        public void comparasing_photo_write(ITelegramBotClient telegram_bot, Message? message) //добавление сравнения
        {
            Comparasign newComparasign = new Comparasign();

            if(message is not null && message.Text is not null && message.From is not null && message.From.Username is not null)
            {
                string module = "write_comp";
                string link = checker.GetLink(message.Text);
                (newComparasign.Phone1.Manufacturer, newComparasign.Phone1.Model, newComparasign.Phone2.Manufacturer, newComparasign.Phone2.Model) = checker.GetAddComparasignName(message.Text);

                if (newComparasign.Phone1.Manufacturer != null && newComparasign.Phone2.Manufacturer != null && message.From.Username.Length <= 30)
                {
                    newComparasign.CompareLink = link;
                    newComparasign.AddedBy = $"{message.From.Username}";
                    
                    string answer = db.ComparasignAddAsync(newComparasign).Result;
                    tg.SendUserLog(answer, module, newComparasign, telegram_bot, message);
                }
                else
                {
                    Console.WriteLine("[ERROR] Не удалось распарсить ссылку или слишком длинное имя отправителя");
                    tg.SendUserLog("[INPUT ERROR]", module, null, telegram_bot, message);
                }
            }
        }

        public void comparasing_photo_read(ITelegramBotClient telegram_bot, Message message, string RequestedBy) //получениие всех сравнений
        {
            string module = "read_all_comp";
            string answer = db.GetComparasignsAsync(RequestedBy).Result;
            try
            {
                if (!answer.Contains("ERROR"))
                {
                    answer = answer.Replace("\\", "");
                    Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                    tg.sendDataTable(telegram_bot, phoneComparisons, message);
                }
                else
                {
                    tg.SendUserLog(answer, module, null, telegram_bot, message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {answer} | {e.Message}");
            }
        }

        public async void comparasing_find(ITelegramBotClient telegram_bot, Message message, string RequestedBy) //получение сравнений по имени телефонов
        {
            string module = "read_comp";
            (string name1, string name2) = checker.GetFindComparasignName(message.Text);

            if (name1 != null && name2 != null)
            {
                Comparasign RequestComparasign = new Comparasign();
                RequestComparasign.AddedBy = RequestedBy;
                (RequestComparasign.Phone1.Manufacturer, RequestComparasign.Phone1.Model) = checker.GetManufacturerAndModel(name1);
                (RequestComparasign.Phone2.Manufacturer, RequestComparasign.Phone2.Model) = checker.GetManufacturerAndModel(name2);
                string answer = db.GetComparasignByTwoNamesAsync(RequestComparasign).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1.Manufacturer} {phoneComparisons[0].Phone1.Model}", $"{phoneComparisons[0].Phone2.Manufacturer} {phoneComparisons[0].Phone2.Model}");
                        (string Spec1, string Spec2) = await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1.Manufacturer} {phoneComparisons[0].Phone1.Model}", $"{phoneComparisons[0].Phone2.Manufacturer} {phoneComparisons[0].Phone2.Model}");
                        if (!Spec1.Contains("ERROR") && !Spec2.Contains("ERROR"))
                        {
                            phoneComparisons[0].Phone1.CameraSpec = Spec1;
                            phoneComparisons[0].Phone2.CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else{tg.SendUserLog(answer, module, null , telegram_bot, message);}
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message} с ответом: {answer}");
                }
            }
            if (name2 == null && name1 != null)
            {
                Comparasign RequestComparasign = new Comparasign();
                RequestComparasign.AddedBy = RequestedBy;
                (RequestComparasign.Phone1.Manufacturer, RequestComparasign.Phone1.Model) = checker.GetManufacturerAndModel(name1);
                string answer = db.GetComparasignByNameAsync(RequestComparasign).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        answer = answer.Replace("\\", "");
                        Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        if (phoneComparisons.Length <= 1)
                        {
                            await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1.Manufacturer} {phoneComparisons[0].Phone1.Model}", $"{phoneComparisons[0].Phone2.Manufacturer} {phoneComparisons[0].Phone2.Model}");
                            string Spec1 = await db.GetCameraSpec($"{phoneComparisons[0].Phone1.Manufacturer} {phoneComparisons[0].Phone1.Model}");
                            string Spec2 = await db.GetCameraSpec($"{phoneComparisons[0].Phone2.Manufacturer} {phoneComparisons[0].Phone2.Model}");
                            phoneComparisons[0].Phone1.CameraSpec = Spec1;
                            phoneComparisons[0].Phone2.CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else{tg.SendUserLog(answer, module, null, telegram_bot, message);}
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message}");
                }
            }

            if(name1 is null && name2 is null){tg.SendUserLog("[BAD NAMES]", module, null, telegram_bot, message);}
        }
       
       public void comparasign_rename()
       {
            
       }
    }
}
