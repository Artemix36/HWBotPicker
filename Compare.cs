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
            public string Phone1Manufacturer { get; set; } = string.Empty;
            public string Phone1Model { get; set; } = string.Empty;
            public string Phone1CameraSpec {  get; set; } = string.Empty;
            public string Phone2Manufacturer { get; set; } = string.Empty;
            public string Phone2Model { get; set; } = string.Empty;
            public string Phone2CameraSpec { get; set; } = string.Empty;
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
                (newComparasign.Phone1Manufacturer, newComparasign.Phone1Model, newComparasign.Phone2Manufacturer, newComparasign.Phone2Model) = checker.GetAddComparasignName(message.Text);

                if (newComparasign.Phone1Manufacturer != null && newComparasign.Phone2Manufacturer != null && message.From.Username.Length <= 30)
                {
                    newComparasign.CompareLink = link;
                    newComparasign.AddedBy = $"{message.From.Username}";
                    
                    string answer = db.ComparasignAddAsync(newComparasign).Result;
                    tg.SendUserLog(answer, module, telegram_bot, message);
                }
                else
                {
                    Console.WriteLine("[ERROR] Не удалось распарсить ссылку или слишком длинное имя отправителя");
                    tg.SendUserLog("[INPUT ERROR]", module, telegram_bot, message);
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
                    tg.SendUserLog(answer, module, telegram_bot, message);
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
            (string? name1, string? name2) = checker.GetFindComparasignName(message.Text);

            if (name1 != null && name2 != null)
            {
                string answer = db.GetComparasignByTwoNamesAsync(name1, name2, RequestedBy).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Replace("\\", "").Trim('"'));
                        await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1Manufacturer} {phoneComparisons[0].Phone1Model}", $"{phoneComparisons[0].Phone2Manufacturer} {phoneComparisons[0].Phone2Model}");
                        (string Spec1, string Spec2) = await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1Manufacturer} {phoneComparisons[0].Phone1Model}", $"{phoneComparisons[0].Phone2Manufacturer} {phoneComparisons[0].Phone2Model}");
                        if (!Spec1.Contains("ERROR") && !Spec2.Contains("ERROR"))
                        {
                            phoneComparisons[0].Phone1CameraSpec = Spec1;
                            phoneComparisons[0].Phone2CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else{tg.SendUserLog(answer, module, telegram_bot, message);}
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
                (RequestComparasign.Phone1Manufacturer, RequestComparasign.Phone1Model) = checker.GetManufacturerAndModel(name1);
                string answer = db.GetComparasignByNameAsync(RequestComparasign).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        answer = answer.Replace("\\", "");
                        List<Comparasign> phoneComparisons = JsonConvert.DeserializeObject<List<Comparasign>>(answer.Trim('"'));
                        if (phoneComparisons.Count <= 1)
                        {
                            await specWriter_HTTP.FindAndWriteSpecs($"{phoneComparisons[0].Phone1Manufacturer} {phoneComparisons[0].Phone1Model}", $"{phoneComparisons[0].Phone2Manufacturer} {phoneComparisons[0].Phone2Model}");
                            string Spec1 = await db.GetCameraSpec($"{phoneComparisons[0].Phone1Manufacturer} {phoneComparisons[0].Phone1Model}");
                            string Spec2 = await db.GetCameraSpec($"{phoneComparisons[0].Phone2Manufacturer} {phoneComparisons[0].Phone2Model}");
                            phoneComparisons[0].Phone1CameraSpec = Spec1;
                            phoneComparisons[0].Phone2CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else{tg.SendUserLog(answer, module, telegram_bot, message);}
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message}");
                }
            }

            if(name1 is null && name2 is null){tg.SendUserLog("[BAD NAMES]", module, telegram_bot, message);}
        }
       
       public void comparasign_rename()
       {
            
       }
    }
}
