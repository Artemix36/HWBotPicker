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
using HWPickerClassesLibrary;
using Org.BouncyCastle.Asn1.Misc;
using ZstdSharp.Unsafe;
using Microsoft.VisualBasic;
using Mysqlx.Crud;

namespace HWpicker_bot
{
    internal class Compare
    {
        CheckMessage checker = new CheckMessage();
        DB_HTTP_worker db = new DB_HTTP_worker();
        TGAPI tg = new TGAPI();
        SpecWriter_HTTP specWriter_HTTP = new SpecWriter_HTTP();

        public void ComparasignFindAllInfo(Interactions interaction) //получение подробной информации о сравнении по кнопке из меню
        {
            if(interaction.Message is not null )
            {
                Comparasign[] NeededComparasign = FindComaparsign(interaction.Message.Text).Result;
                if(NeededComparasign[0].Phone1.Manufacturer == string.Empty && NeededComparasign[0].Phone1.Model == string.Empty)
                {
                    tg.SendUserLog("[ERROR] Сравнения не найдены", "read_comp", NeededComparasign[0], interaction.Message);
                    return;
                }
                tg.SendDataTable(NeededComparasign, interaction.Message);
            }
            if(interaction.CallbackQuery is not null)
            {
                Comparasign[] NeededComparasign = FindComaparsign(interaction.CallbackQuery.Data).Result;
                if(NeededComparasign[0].Phone1.Manufacturer == string.Empty && NeededComparasign[0].Phone1.Model == string.Empty)
                {
                    tg.SendUserLog("[ERROR] Сравнения не найдены", "read_comp", NeededComparasign[0], interaction.CallbackQuery);
                    return;
                }
                CallBackEditing callBackEditing = new CallBackEditing();
                callBackEditing.AllInfoAboutComparasingCallback(NeededComparasign, interaction.CallbackQuery);
            }
        }

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
                    tg.SendUserLog(answer, module, newComparasign, message);
                }
                else
                {
                    Console.WriteLine("[ERROR] Не удалось распарсить ссылку или слишком длинное имя отправителя");
                    tg.SendUserLog("[INPUT ERROR]", module, new Comparasign(), message);
                }
            }
        }
        public void comparasing_photo_read(ITelegramBotClient telegram_bot, Message message, int page, int replyTo) //получениие всех сравнений по страницам
        {
            if(message.From is not null && message.From.Username is not null)
            {
                string module = "read_all_comp";
                string answer = db.GetComparasignsAsync(message.From.Username, page).Result.Replace("\\", "");
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        Comparasign[]? phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        if(phoneComparisons is not null){tg.SendDataAllComparasigns(telegram_bot, phoneComparisons, message, page, replyTo);}
                    }
                    else
                    {
                        tg.SendUserLog(answer, module, new Comparasign(), message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {answer} | {e.Message}");
                }
            }
        }
        public async Task<Comparasign[]> FindComaparsign(string Text)
        {
            if(Text != null)
            {
                (string name1, string name2) = checker.ParseRequestName(Text);

                if(name1 != string.Empty && name2 == string.Empty)
                {
                    return await RequestComparasign(name1);
                }
                if(name1 != string.Empty && name2 != string.Empty)
                {
                    return await RequestComparasign(name1, name2);
                }
                return new Comparasign[1];
            }
            return new Comparasign[1];
        }
        public async Task<Comparasign[]> RequestComparasign(string name)
        {
                Comparasign RequestComparasign = new Comparasign();
                (RequestComparasign.Phone1.Manufacturer, RequestComparasign.Phone1.Model) = checker.GetManufacturerAndModel(name);
                string answer = db.GetComparasignByNameAsync(RequestComparasign).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        answer = answer.Replace("\\", "");
                        Comparasign[]? phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        if(phoneComparisons is not null && phoneComparisons.Length == 1)
                        {
                            await specWriter_HTTP.GetCameraSpec(phoneComparisons[0].Phone1);
                            await specWriter_HTTP.GetCameraSpec(phoneComparisons[0].Phone2);
                        }
                        return phoneComparisons;
                    }
                    return new Comparasign[1];
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message}");
                }
                return new Comparasign[1];
        }
        public async Task<Comparasign[]> RequestComparasign(string name1, string name2)
        {
                Comparasign RequestComparasign = new Comparasign();
                (RequestComparasign.Phone1.Manufacturer, RequestComparasign.Phone1.Model) = checker.GetManufacturerAndModel(name1);
                (RequestComparasign.Phone2.Manufacturer, RequestComparasign.Phone2.Model) = checker.GetManufacturerAndModel(name2);
                string answer = db.GetComparasignByTwoNamesAsync(RequestComparasign).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        Comparasign[]? phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));

                        if(phoneComparisons is not null && phoneComparisons.Length == 1)
                        {
                            phoneComparisons[0].Phone1 = await specWriter_HTTP.GetCameraSpec(phoneComparisons[0].Phone1);
                            phoneComparisons[0].Phone2 = await specWriter_HTTP.GetCameraSpec(phoneComparisons[0].Phone2);
                        }
                        return phoneComparisons;
                    }
                    return new Comparasign[1];
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message} с ответом: {answer}");
                }
                return new Comparasign[1];
        }
    }
}
