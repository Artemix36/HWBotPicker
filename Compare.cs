﻿using HardWarePickerBot;
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

namespace HWpicker_bot
{
    internal class Compare
    {
        public class Comparasign
        {
            public string Phone1Name { get; set; }
            public string Phone1CameraSpec {  get; set; }
            public string Phone2Name { get; set; }
            public string Phone2CameraSpec { get; set; }
            public string CompLink { get; set; }
        }

        CheckMessage checker = new CheckMessage();
        DB_HTTP_worker db = new DB_HTTP_worker();
        TGAPI tg = new TGAPI();

        public async void comparasing_photo_write(ITelegramBotClient telegram_bot, Message message) //добавление сравнения
        {
            Comparasign newComparasign = new Comparasign();
            string link = checker.GetLink(message.Text);
            (newComparasign.Phone1Name, newComparasign.Phone2Name) = checker.GetAddComparasignName(message.Text);

            if (newComparasign.Phone1Name != null && newComparasign.Phone2Name != null)
            {
                newComparasign.CompLink = link;
                string answer = db.ComparasignAddAsync(newComparasign).Result;
                await telegram_bot.SendTextMessageAsync(message.Chat.Id, answer);
            }
            else
            {
                Console.WriteLine("[ERROR] Не удалось распарсить ссылку");
                await telegram_bot.SendTextMessageAsync(message.Chat.Id, "Неверный входной формат");
            }
        }

        public void comparasing_photo_read(ITelegramBotClient telegram_bot, Message message) //получениие всех сравнений
        {
            string answer = db.GetComparasignsAsync().Result;
            try
            {
                answer = answer.Replace("\\", "");
                Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                tg.sendDataTable(telegram_bot, phoneComparisons, message);
            }
            catch (Exception e) 
            {
                Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message}");
            }
        }

        public async void comparasing_find(ITelegramBotClient telegram_bot, Message message) //получение сравнений по имени телефонов
        {
            (string name1, string name2) = checker.GetFindComparasignName(message.Text);
            SpecWriter_HTTP specWriter_HTTP = new SpecWriter_HTTP();
            if (name1 != null && name2 != null)
            {
                string answer = db.GetComparasignByTwoNamesAsync(name1,name2).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        answer = answer.Replace("\\", "");
                        Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        await specWriter_HTTP.FindAndWriteSpecs(phoneComparisons[0].Phone1Name, phoneComparisons[0].Phone2Name);
                        (string Spec1, string Spec2) = await specWriter_HTTP.FindAndWriteSpecs(phoneComparisons[0].Phone1Name, phoneComparisons[0].Phone2Name);
                        if (!Spec1.Contains("ERROR") && !Spec2.Contains("ERROR"))
                        {
                            phoneComparisons[0].Phone1CameraSpec = Spec1;
                            phoneComparisons[0].Phone2CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else
                    {
                        await telegram_bot.SendTextMessageAsync(message.Chat.Id, answer.Replace("\"", ""));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message} с ответом: {answer}");
                }
            }
            if (name2 == null && name1 != null)
            {
                string answer = db.GetComparasignByNameAsync(name1).Result;
                try
                {
                    if (!answer.Contains("ERROR"))
                    {
                        answer = answer.Replace("\\", "");
                        Comparasign[] phoneComparisons = JsonConvert.DeserializeObject<Comparasign[]>(answer.Trim('"'));
                        if (phoneComparisons.Length <= 1)
                        {
                            await specWriter_HTTP.FindAndWriteSpecs(phoneComparisons[0].Phone1Name, phoneComparisons[0].Phone2Name);
                            string Spec1 = await db.GetCameraSpec(phoneComparisons[0].Phone1Name);
                            string Spec2 = await db.GetCameraSpec(phoneComparisons[0].Phone2Name);
                            phoneComparisons[0].Phone1CameraSpec = Spec1;
                            phoneComparisons[0].Phone2CameraSpec = Spec2;
                        }
                        tg.sendDataTable(telegram_bot, phoneComparisons, message);
                    }
                    else
                    {
                        await telegram_bot.SendTextMessageAsync(message.Chat.Id, answer);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] ошибка при парсинге ответа от БД: {e.Message}");
                }
            }
            if(name1 == null && name2 == null)
            {
                tg.sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Неверно введено имя сравнения! Проверьте синтаксис!</b>", reply: message.MessageId);
            }
        }
       
    }
}