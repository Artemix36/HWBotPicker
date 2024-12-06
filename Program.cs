using HWpicker_bot;
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
        static internal List<Interactions> CallbackInteractions = new List<Interactions>();
        static void Main(string[] args)
        {
            Console.WriteLine(")                    )           (   (             )     (     \n( /( (  (         (  ( /(   *   )   )\\ ))\\ )  (    ( /(     )\\ )\n)\\()))\\))(   '  ( )\\ )\\())` )  /(  (()/(()/(  )\\   )\\())(  (()/(  \n((_)\\((_)()\\ )   )((_|(_)\\  ( )(_))  /(_))(_)|((_)|((_)\\ )\\  /(_)) \n_((_)(())\\_)() ((_)_  ((_)(_(_())  (_))(_)) )\\___|_ ((_|(_)(_))\n| || \\ \\((_)/ /  | _ )/ _ \\|_   _|  | _ \\_ _((/ __| |/ /| __| _ \\  \n| __ |\\ \\/\\/ /   | _ \\ (_) | | |    |  _/| | | (__  ' < | _||   \n|_||_| \\_/\\_/    |___/\\___/  |_|    |_| |___| \\___|_|\\_\\|___|_|_\")");
            Thread ConfListening = new Thread(async () => await ConfigureListener());
            ConfListening.Start();
            Console.Read();
        }

        static Task ConfigureListener() //поток прослушивания сообщения
        {
            Console.WriteLine($"[INF] {Thread.CurrentThread.ThreadState}: New thread started. Starting bot");   
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
                        DropPendingUpdates = true,
                    };
                    DB_HTTP_worker.DBBaseURL = botVars.DBBaseURL;
                    SpecWriter_HTTP.GSMarenaBotToken = botVars.GSMarenaBotToken;
                    SpecWriter_HTTP.GSMarenaBotUrl = botVars.GSMarenaBotUrl;
                    SpecWriter_HTTP.timeout = botVars.Timeout;
                    TGAPI.StartupMessage = botVars.StarttupMessage;
                    TGAPI.ComparasignModuleMessage = botVars.ComparasignModuleMessage;
                    TGAPI.telegram_bot = telegram_bot;
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
            Interactions interaction = new Interactions();
            interaction.Message = message;
            if(message.Text is not null && message.From is not null)
            {
                interaction.From = $"{message.From.FirstName} {message.From.LastName} {message.From.Username}";
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
                    telegram.SendComparasignMenu(message);
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
                    comparator.ComparasignFindAllInfo(interaction, interaction.Module[0]);
                    return;
                }

                if (receivedText.Contains("покажи сравнение") || receivedText.Contains("покажи сравнения") || receivedText.Contains("покажи мои сравнения"))
                {
                    Console.WriteLine("[INFO] Начало обоаботки полученного сообщения");
                    comparator.ComparasignFindAllInfo(interaction, interaction.Module[2]);
                    return;
                }

            }
        }
        
        async static Task ParseCallback(ITelegramBotClient telegram_bot, Update update, CallbackQuery callback)
        {
            Interactions interaction = new Interactions();
            interaction.CallbackQuery = callback;
            interaction.From = $"{callback.From.Id} {callback.From.Username}";
            
            if(interaction.CallbackQuery is not null && interaction.CallbackQuery.Data is not null && interaction.CallbackQuery.Message is not null)
            {
                if(CallbackInteractions.Count != 0 && CanProcessCallback(interaction))
                {
                    if(interaction.CallbackQuery.Data.Contains('['))
                    {
                        Console.WriteLine($"[INFO] запрос поиска подробной информации по {interaction.CallbackQuery.Data.Trim('[')}");

                        interaction.CallbackQuery.Data = interaction.CallbackQuery.Data.Trim('[');
                        
                        comparator.ComparasignFindAllInfo(interaction, interaction.Module[2]);

                        try
                        {
                            await telegram_bot.AnswerCallbackQuery(callbackQueryId: callback.Id);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[ERROR] не получилось ответить на Callback от {callback.From} {ex.Message}");
                        }
                    }
                    if(interaction.CallbackQuery.Data.Contains("page:"))
                    {
                        interaction.CallbackQuery.Data = interaction.CallbackQuery.Data.Replace("page:", "");

                        comparator.ComparasignFindAllInfo(interaction, interaction.Module[0]);

                        try
                        {
                            await telegram_bot.AnswerCallbackQuery(callbackQueryId: callback.Id);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[ERROR] не получилось ответить на Callback от {callback.From} {ex.Message}");
                        }
                    }
                    if(callback.Data == "/comparasign")
                    {   
                        telegram.SendComparasignMenu(interaction.CallbackQuery);

                        try
                        {
                            await telegram_bot.AnswerCallbackQuery(callbackQueryId: callback.Id);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[ERROR] не получилось ответить на Callback от {callback.From} {ex.Message}");
                        }
                    }
                    if(callback.Data == "comp main menu")
                    {    
                        interaction.CallbackQuery.Data = "1";
                        comparator.ComparasignFindAllInfo(interaction, interaction.Module[0]);

                        try
                        {
                            await telegram_bot.AnswerCallbackQuery(callbackQueryId: callback.Id);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[ERROR] не получилось ответить на Callback от {callback.From} {ex.Message}");
                        }
                    }
                }
                else
                {
                    telegram.SendUserLog("not new interaction", "not new interaction", null, interaction.CallbackQuery);
                }
            }
        }

        static bool CanProcessCallback(Interactions interaction)
        {
            if(CallbackInteractions.Count != 0)
            {
                int i = 0;
                int j = 0;
                foreach(Interactions PreviousInteration in CallbackInteractions)
                {
                    if(PreviousInteration.Message.Id != interaction.CallbackQuery.Message.Id)
                    {

                    }
                    if(PreviousInteration.Message.Id == interaction.CallbackQuery.Message.Id)
                    {
                        if(PreviousInteration.PreviousFrom == interaction.From)
                        {
                            i++;
                            j++;
                        }
                        if(PreviousInteration.PreviousFrom != interaction.From)
                        {
                            return false;
                        }
                    }
                }
                if(i == 0 && j != 0)
                {
                    telegram.SendUserLog("not new interaction", "not new interaction", null, interaction.CallbackQuery);
                    return false;
                }
                if(i == 0 && j == 0)
                {
                    telegram.SendUserLog("not yours interaction", "not new interaction", null, interaction.CallbackQuery);
                    return false;
                }
                if(i != 0 && j != 0)
                {
                    return true;
                }
            }
            return false;
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
                    telegram_bot.SendMessage(update.Id, "Потерян контекст или кто то нажал кнопки за вас");
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
