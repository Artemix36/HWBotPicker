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

namespace HW_picker_bot
{
    class Program
    {
        static bool to_work = true;
        static int[] rate = new int[5];
        static int counter;
        static Message messageForCallback = null;
        static MessagePool<ContextOfMsg> MsgPool = new MessagePool<ContextOfMsg>();

        static void Main(string[] args)
        {
            Console.WriteLine(")                    )           (   (             )     (     \n( /( (  (         (  ( /(   *   )   )\\ ))\\ )  (    ( /(     )\\ )\n)\\()))\\))(   '  ( )\\ )\\())` )  /(  (()/(()/(  )\\   )\\())(  (()/(  \n((_)\\((_)()\\ )   )((_|(_)\\  ( )(_))  /(_))(_)|((_)|((_)\\ )\\  /(_)) \n_((_)(())\\_)() ((_)_  ((_)(_(_())  (_))(_)) )\\___|_ ((_|(_)(_))\n| || \\ \\((_)/ /  | _ )/ _ \\|_   _|  | _ \\_ _((/ __| |/ /| __| _ \\  \n| __ |\\ \\/\\/ /   | _ \\ (_) | | |    |  _/| | | (__  ' < | _||   \n|_||_| \\_/\\_/    |___/\\___/  |_|    |_| |___| \\___|_|\\_\\|___|_|_\")");
            Thread StartListening = new Thread(Start_to_listen);
            StartListening.Start();

            Console.Read();
        }

        static void Start_to_listen() //поток прослушивания сообщения
        {
            Console.WriteLine("[INF] New thread started. Starting bot");
            // string path = @"C:\token.txt";
            string path = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/token.txt";
            try
            {
                string[] token = System.IO.File.ReadAllLines(path);
                TelegramBotClient telegram_bot = new TelegramBotClient(token[0]);
                ReceiverOptions receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = new[]
                    {
                UpdateType.Message,
                UpdateType.CallbackQuery
                },
                    ThrowPendingUpdates = true,
                };

                Program Program = new Program();

                while (Program.to_work)
                {
                    if (Program.to_work == true)
                    {
                        telegram_bot.StartReceiving(Update, Handle_errors, receiverOptions);
                        Console.WriteLine("[INF] SUCCESS. Bot got token and started listening");
                        Console.Read();
                    }
                    else
                    {
                        Console.WriteLine("Завершение работы");
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            }

        private static Task Handle_errors(ITelegramBotClient telegram_bot, Exception exception, CancellationToken token)
        {
            string ErrorMessage = exception.ToString();
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        async static Task Update(ITelegramBotClient telegram_bot, Update update, CancellationToken token) //при получении сообщения
        {

            TGAPI telegram = new TGAPI();
 
            try
            {
                if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is not null)
                {
                    var callback = update.CallbackQuery;
                    ContextOfMsg messageWorker = MsgPool.getObj(update);
                    Console.WriteLine($"{telegram.getCallbackName(callback.From).Item1} | {telegram.getCallbackName(callback.From).Item2}");

                    if (messageWorker.isCallBackFromSameGuy(update))
                    {
                        Thread CheckNewMessage = new Thread(() => messageWorker.ParseCallback(telegram_bot, update, token));
                        CheckNewMessage.Start();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Другой пользователь использует меню оценки");
                    }
                }
                if (update.Type == UpdateType.Message && update.Message is not null)
                {
                        var message = update.Message;
                        if (message.Text == null) return;
                        Console.WriteLine($"{telegram.getName(message).Item1} | {telegram.getName(message).Item2}");

                        Thread CheckNewMessage = new Thread(async () => await ParseMessage(telegram_bot, message, update, token));
                        CheckNewMessage.Start();
                        return;
                }
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        async static Task ParseMessage(ITelegramBotClient telegram_bot, Message message, Update update, CancellationToken token)
        {
            Program Program = new Program();
            Compare comparator = new Compare();
            TGAPI telegram = new TGAPI();
            
            if(message.Text is not null && message.From is not null)
            {
                if (message.Text.ToLower().Contains("миронов"))
                {
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "@ReversFlash25 купи 12су за 45к и в доставку!");
                    return;
                }

                if (message.Text.ToLower().Contains("фролов"))
                {
                    telegram.sendMessage(telegram_bot, "document", message.Chat.Id, document: "https://tenor.com/qX1eCt0OjDO.gif");
                    return;
                }

                if (message.Text.ToLower().Contains("остановить работу сейчас же"))
                {
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "Вырубаюсь");
                    Program.to_work = false;
                    return;
                }

                if (message.Text.ToLower().Contains("добавить ссылку") || message.Text.ToLower().Contains("добавь ссылку") || message.Text.ToLower().Contains("добавить сравнение") || message.Text.ToLower().Contains("добавь сравнение"))
                {
                    comparator.comparasing_photo_write(telegram_bot, message);
                    return;
                }

                if (message.Text.ToLower().Contains("покажи сравнения"))
                {
                    comparator.comparasing_photo_read(telegram_bot, message, $"{message.From.FirstName} {message.From.LastName} | {message.From.Username}");
                    return;
                }

                if (message.Text.ToLower().Contains("добавь отзыв на") || message.Text.ToLower().Contains("добавить отзыв на"))
                {
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

                if (message.Text.ToLower().Contains("покажи сравнение"))
                {
                    comparator.comparasing_find(telegram_bot, message, $"{message.From.FirstName} {message.From.LastName} | {message.From.Username}");
                    return;
                }

            }
        }

        static void ParseCallback(ITelegramBotClient telegram_bot, Update update, CancellationToken token)
        {
            Program progr = new Program();

            string[] buttons = new string[] { "Общая оценка", "Система", "Камера", "Батарея", "Экран" };
            var callback = update.CallbackQuery;
            if(callback  is not null && callback.Data is not null)
            {
                string data = callback.Data.ToString();
                Console.WriteLine(counter);

                for (int i = 0; i < 5; i++)
                {
                    if (data.Contains(buttons[i]))
                    {
                        data = data.Replace(buttons[i] + ":", "");
                        int.TryParse(data, out rate[i]);
                        counter++;
                    }
                }

                if (counter == 5) {
                    counter = 0;
                    messageForCallback = null;
                }
                else
                {
                    telegram_bot.SendTextMessageAsync(update.Id, "Потерян контекст или кто то нажал кнопки за вас");
                }
            }
        }

    }

    class ContextOfMsg
    {
        static public int[] rate = new int[5];
        static public int counter { get; set; }
        static public Message messageForCallback {  get; set; }
        static public Update update {  get; set; }

        static ContextOfMsg Obj = new ContextOfMsg();

        public bool isCallBackFromSameGuy(Update update2)
        {
            if(update is not null && update.Message is not null && update2 is not null && update2.CallbackQuery is not null){
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
        public void ParseCallback(ITelegramBotClient telegram_bot, Update update, CancellationToken token)
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
                    messageForCallback = null;
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