﻿using HWpicker_bot;
using TelegramApi;
using System;
using System.Collections.Generic;
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
using static System.Net.Mime.MediaTypeNames;
using HardWarePickerBot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using System.Runtime.CompilerServices;
using YAMLvarsReader;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace HW_picker_bot
{
    class Program
    {
        static int[] rate = new int[5];
        static Message? messageForCallback = null;
        static MessagePool<ContextOfMsg> MsgPool = new MessagePool<ContextOfMsg>();
        static private Compare comparator = new Compare();
        static private TGAPI telegram = new TGAPI();
        static void Main(string[] args)
        {
            Console.WriteLine(")                    )           (   (             )     (     \n( /( (  (         (  ( /(   *   )   )\\ ))\\ )  (    ( /(     )\\ )\n)\\()))\\))(   '  ( )\\ )\\())` )  /(  (()/(()/(  )\\   )\\())(  (()/(  \n((_)\\((_)()\\ )   )((_|(_)\\  ( )(_))  /(_))(_)|((_)|((_)\\ )\\  /(_)) \n_((_)(())\\_)() ((_)_  ((_)(_(_())  (_))(_)) )\\___|_ ((_|(_)(_))\n| || \\ \\((_)/ /  | _ )/ _ \\|_   _|  | _ \\_ _((/ __| |/ /| __| _ \\  \n| __ |\\ \\/\\/ /   | _ \\ (_) | | |    |  _/| | | (__  ' < | _||   \n|_||_| \\_/\\_/    |___/\\___/  |_|    |_| |___| \\___|_|\\_\\|___|_|_\")");
            Thread ConfListening = new Thread(async () => await ConfigureListener());
            ConfListening.Start();
            Console.Read();
        }

        static Task ConfigureListener() //поток прослушивания сообщения
        {
            Console.WriteLine($"[INF] {Thread.CurrentThread.ThreadState} New thread started. Starting bot");   
            string path = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/var.txt";
            BotVars botVars = new BotVars();
            YamlReader reader = new YamlReader();
            botVars = reader.ReadVars();
            try
            {
                if(botVars.TGtoken != "not found" && botVars.DBBaseURL is not null)
                {
                    TelegramBotClient telegram_bot = new TelegramBotClient(botVars.TGtoken);
                    ReceiverOptions receiverOptions = new ReceiverOptions
                    {
                        AllowedUpdates = new[]
                        {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                    },
                        ThrowPendingUpdates = true,
                    };
                    DB_HTTP_worker.DBBaseURL = botVars.DBBaseURL;
                    SpecWriter_HTTP.GSMarenaBotToken = botVars.GSMarenaBotToken;
                    SpecWriter_HTTP.GSMarenaBotUrl = botVars.GSMarenaBotUrl;
                    SpecWriter_HTTP.timeout = botVars.Timeout;
                    TGAPI.StartupMessage = botVars.StarttupMessage;
                    TGAPI.ComparasignModuleMessage = botVars.ComparasignModuleMessage;
                    Program Program = new Program();
                    
                    telegram_bot.StartReceiving(OnUpdate, Handle_errors, receiverOptions);
                    Console.WriteLine($"[INF] SUCCESS. Bot got token and DB base url {DB_HTTP_worker.DBBaseURL} and started listening");
                    return Task.CompletedTask;
                }
                if(botVars.TGtoken == "not found" || botVars.DBBaseURL is null)
                {
                    return Task.CompletedTask;
                    throw new Exception("[ERROR] Не получен токен бота");
                }
                return Task.CompletedTask;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return Task.CompletedTask;
            }
        }

        private static void Handle_errors(ITelegramBotClient telegram_bot, Exception exception, CancellationToken token)
        {
            string ErrorMessage = exception.ToString();
            Console.WriteLine(ErrorMessage);
        }

        static void OnUpdate(ITelegramBotClient telegram_bot, Update? update, CancellationToken token)
        {
            TGAPI telegram = new TGAPI();
            if(update is not null)
            {
                try
                {
                    if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is not null)
                    {
                        var callback = update.CallbackQuery;
                        Console.WriteLine($"[CLBCK UPDATE] {update.CallbackQuery.From.FirstName} | {update.CallbackQuery.From.Id}");
                        Thread CheckNewCallback= new Thread(async () => await ParseCallback(telegram_bot, update, callback));
                        CheckNewCallback.Start();
                        return;
                    }
                    if (update.Type == UpdateType.Message && update.Message is not null)
                    {
                        var message = update.Message;
                        if (message.Text == null) return;
                        Console.WriteLine($"[MSG UPDATE] {telegram.getName(message).Item1} | {telegram.getName(message).Item2}");

                        Thread CheckNewMessage = new Thread(async () => await ParseMessage(telegram_bot, message, update));
                        CheckNewMessage.Start();
                        return;
                    }
                    return;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    return;
                }
            }
        }
        async static Task ParseMessage(ITelegramBotClient telegram_bot, Message message, Update update)
        {
            if(message.Text is not null && message.From is not null)
            {
                string receivedText = message.Text.ToLower();

                if(receivedText == "/start" || receivedText == "/start@hw_picker_bot")
                {
                    Console.WriteLine($"[INFO] Запрошено главное меню через {receivedText}");
                    telegram.SendMainMenu(telegram_bot, message);
                    return;
                }

                if(receivedText == "/comparasign" || receivedText == "/comparasign@hw_picker_bot")
                {
                    Console.WriteLine($"[INFO] Запрошено меню сравнения через {receivedText}");
                    telegram.SendComparasignMenu(telegram_bot, message);
                    return;
                }

                if (receivedText.Contains("миронов"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "@ReversFlash25 купи 12су за 45к и в доставку!");
                    return;
                }

                if (receivedText.Contains("фролов"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    telegram.sendMessage(telegram_bot, "document", message.Chat.Id, document: "https://tenor.com/qX1eCt0OjDO.gif");
                    return;
                }

                if (receivedText.Contains("остановить работу сейчас же"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "Вырубаюсь");
                    return;
                }

                if (receivedText.Contains("добавить ссылку") || receivedText.Contains("добавь ссылку") || receivedText.Contains("добавить сравнение") || receivedText.Contains("добавь сравнение"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    comparator.comparasing_photo_write(telegram_bot, message);
                    return;
                }

                if (receivedText == "покажи сравнения" || receivedText == "покажи мои сравнения")
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    comparator.comparasing_photo_read(telegram_bot, message, 1);
                    return;
                }

                if (receivedText.Contains("добавь отзыв на") || receivedText.Contains("добавить отзыв на"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    try
                    {
                        messageForCallback = message;
                        CheckMessage checker = new CheckMessage();
                        string name = checker.GetReviewName(message.Text);

                        if (name != null)
                        {
                            string[] buttons = new string[] { "Общая оценка", "Система", "Камера", "Батарея", "Экран" };

                            for (int i = 0; i < buttons.Length; i++)
                            {
                                InlineKeyboardMarkup ikmReviews = (new[]
                                {
                                    new []
                                    {
                                    InlineKeyboardButton.WithCallbackData(text: "1", callbackData: $"{buttons[i]}: 1"),
                                    InlineKeyboardButton.WithCallbackData(text: "2", callbackData: $"{buttons[i]}: 2"),
                                    InlineKeyboardButton.WithCallbackData(text: "3", callbackData: $"{buttons[i]}: 3"),
                                    InlineKeyboardButton.WithCallbackData(text: "4", callbackData: $"{buttons[i]}: 4"),
                                    InlineKeyboardButton.WithCallbackData(text: "5", callbackData: $"{buttons[i]}: 5"),
                                    },
                                });
                                await telegram_bot.SendTextMessageAsync(message.Chat.Id, $"{buttons[i]}:", replyMarkup: ikmReviews);
                            }
                            ContextOfMsg messageWorker = MsgPool.getObj(update);
                            messageWorker.SetValues(0, update, message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "Ошибка при добавлении");
                    }
                }

                if (receivedText.Contains("покажи сравнение") || receivedText.Contains("покажи сравнения") || receivedText.Contains("покажи мои сравнения"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    comparator.comparasing_find(telegram_bot, message, "message");
                    return;
                }

            }
        }

       async static Task ParseCallback(ITelegramBotClient telegram_bot, Update update, CallbackQuery callback)
        {
            if(callback is not null && callback.Data is not null && callback.Message is not null)
            {
                if(callback.Data.Contains('['))
                {
                    Console.WriteLine($"[INFO] запрос поиска подробной информации по {callback.Data.Trim('[')}");
                    Message message = new Message();
                    message.Chat = callback.Message.Chat;
                    message.MessageId = callback.Message.MessageId;
                    message.From = callback.From;
                    message.Text = $"покажи сравнение {callback.Data.Trim('[')}";
                    
                    comparator.ComparasignFindAllInfo(telegram_bot, message);
                    try
                    {
                        await telegram_bot.AnswerCallbackQueryAsync(callbackQueryId: callback.Id);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"[ERROR] не получилось ответить на Callback от {callback.From} {ex.Message}");
                    }
                }
                if(callback.Data.Contains("page:"))
                {
                    Message message = new Message();
                    message.Chat = callback.Message.Chat;
                    message.MessageId = callback.Message.MessageId;
                    message.From = callback.From;
                    message.AuthorSignature = "CLBK";
                    message.Text = $"{callback.Data.Replace("page:", "")}";
                    int page_num;
                    Int32.TryParse(callback.Data.Replace("page:", ""), out page_num);
                    comparator.comparasing_photo_read(telegram_bot, message, page_num);
                    await telegram_bot.AnswerCallbackQueryAsync(callbackQueryId: callback.Id);
                }
                if(callback.Data == "/comparasign")
                {
                    Message message = new Message();
                    message.Chat = callback.Message.Chat;
                    message.MessageId = callback.Message.MessageId;
                    message.From = callback.From;
                    message.Text = $"{callback.Data}";
                    
                    telegram.SendComparasignMenu(telegram_bot, message);
                    await telegram_bot.AnswerCallbackQueryAsync(callbackQueryId: callback.Id);
                }
                if(callback.Data == "comp main menu")
                {
                    Message message = new Message();
                    message.Chat = callback.Message.Chat;
                    message.MessageId = callback.Message.MessageId;
                    message.From = callback.From;
                    message.Text = $"{callback.Data}";
                    
                    comparator.comparasing_photo_read(telegram_bot, message, 1);
                    await telegram_bot.AnswerCallbackQueryAsync(callbackQueryId: callback.Id);
                }
            }
        }

    }

    class ContextOfMsg
    {
        static public int[] rate = new int[5];
        static public int counter { get; set; }
        static public Message messageForCallback {  get; set; } = new Message();
        static public Update update {  get; set; } = new Update();

        static ContextOfMsg Obj = new ContextOfMsg();

        public bool isCallBackFromSameGuy(Update update2)
        {
            if(update is not null && update.Message is not null && update2 is not null && update2.CallbackQuery is not null && update.Message.From is not null){
                var msg = update.Message.From;
                var msg2 = update2.CallbackQuery.From;
                TGAPI tg = new TGAPI();

                if (tg.getCallbackName(msg).Item2 == tg.getCallbackName(msg2).Item2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void SetValues(int count, Update updated, Message Message)
        {
            ContextOfMsg.counter = count;
            ContextOfMsg.update = updated;
            var msg = update.Message;
            ContextOfMsg.messageForCallback = Message;
        }
        public void ParseCallback(ITelegramBotClient telegram_bot, Update update)
        {
            Program progr = new Program();
            //Phone_Menu pm = new Phone_Menu();
            string[] buttons = new string[] { "Общая оценка", "Система", "Камера", "Батарея", "Экран" };
            var callback = update.CallbackQuery;
            if(callback is not null && callback.Data is not null)
            {
                string data = callback.Data.ToString();

                for (int i = 0; i < 5; i++)
                {
                    if (data.Contains(buttons[i]))
                    {
                        data = data.Replace(buttons[i] + ":", "");
                        int.TryParse(data, out rate[i]);
                        counter++;
                    }
                }

                if (counter == 5)
                {
                    counter = 0;
                    //pm.PhoneReview(telegram_bot, rate, messageForCallback, update);
                    messageForCallback = new Message();
                }
                else
                {
                    telegram_bot.SendTextMessageAsync(update.Id, "Потерян контекст или кто то нажал кнопки за вас");
                }
            }
        }

    }

    public class MessagePool<T> where T: new()
    {
        private List<T> Messagelist = new List<T>();
        private int ObjCounter = 0;
        private int MaxObjects = 10;

        public int getCount()
        {
            return ObjCounter;
        }

        public T getObj(Update update2)
        {
            T MessageObj;
            if (ObjCounter > 0)
            {
                MessageObj = Messagelist[0];
                ObjCounter --;
                return MessageObj;
            }
            else
            {
                T obj = new T();
                return obj;
            }
        }
        public void releaseObj(T item)
        {
            if (ObjCounter < MaxObjects)
            {
                Messagelist.Add(item);
                ObjCounter++;
            }
        }

    }
}