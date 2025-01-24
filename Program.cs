using HWpicker_bot;
using TelegramApi;
using Telegram.Bot;
using Telegram.Bot.Types;
using HardWarePickerBot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using YAMLvarsReader;

namespace HW_picker_bot
{
    class Program
    {
        static int[] rate = new int[5];
        static ILogger logger;
        static private Compare comparator = new Compare();
        static private TGAPI telegram = new TGAPI();
        static internal List<Interactions> CallbackInteractions = new List<Interactions>();
        static void Main(string[] args)
        {
            Console.WriteLine(")                    )           (   (             )     (     \n( /( (  (         (  ( /(   *   )   )\\ ))\\ )  (    ( /(     )\\ )\n)\\()))\\))(   '  ( )\\ )\\())` )  /(  (()/(()/(  )\\   )\\())(  (()/(  \n((_)\\((_)()\\ )   )((_|(_)\\  ( )(_))  /(_))(_)|((_)|((_)\\ )\\  /(_)) \n_((_)(())\\_)() ((_)_  ((_)(_(_())  (_))(_)) )\\___|_ ((_|(_)(_))\n| || \\ \\((_)/ /  | _ )/ _ \\|_   _|  | _ \\_ _((/ __| |/ /| __| _ \\  \n| __ |\\ \\/\\/ /   | _ \\ (_) | | |    |  _/| | | (__  ' < | _||   \n|_||_| \\_/\\_/    |___/\\___/  |_|    |_| |___| \\___|_|\\_\\|___|_|_\")");
            Thread ConfListening = new Thread(async () => await ConfigureListener());
            ConfListening.Start();
            while(true)
            {
                Thread.Sleep(50000);
            }
        }

        static Task ConfigureListener() //поток прослушивания сообщения
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => 
            {
                builder.ClearProviders();
                builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
                builder.AddFilter("System", LogLevel.Debug).SetMinimumLevel(LogLevel.Information);
            });
            logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation($"Configuration started. Starting bot");   
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
                    logger.LogInformation($"Bot got token and DB base url: {DB_HTTP_worker.DBBaseURL}, bot started listening");
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
                logger.LogCritical(e.ToString());
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
                        logger.LogInformation($"[CLBCK UPDATE] {update.CallbackQuery.From.FirstName} | {update.CallbackQuery.From.Id}");
                        Thread CheckNewCallback= new Thread(async () => await ParseCallback(telegram_bot, update, callback));
                        CheckNewCallback.Start();
                        return;
                    }
                    if (update.Type == UpdateType.Message && update.Message is not null)
                    {
                        var message = update.Message;
                        if (message.Text == null) return;
                        logger.LogInformation($"[MSG UPDATE] {telegram.getName(message).Item1} | {telegram.getName(message).Item2}");

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
            CheckMessage checker = new CheckMessage();
            interaction.Message = message;
            if(message.Text is not null && message.From is not null)
            {
                interaction.From = $"{message.From.FirstName} {message.From.LastName} {message.From.Username}";
                string receivedText = message.Text.ToLower();

                if(receivedText == "/start" || receivedText == "/start@hw_picker_bot")
                {
                    logger.LogInformation($"Запрошено главное меню через {receivedText}");
                    telegram.SendMainMenu(telegram_bot, message);
                    return;
                }

                if(receivedText == "/comparasign" || receivedText == "/comparasign@hw_picker_bot")
                {
                    logger.LogInformation($"Запрошено меню сравнения через {receivedText}");
                    telegram.SendComparasignMenu(message);
                    return;
                }

                if (receivedText.Contains("миронов"))
                {
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "@ReversFlash25 купи 12су за 45к и в доставку!");
                    return;
                }

                if (receivedText.Contains("фролов"))
                {
                    telegram.sendMessage(telegram_bot, "document", message.Chat.Id, document: "https://tenor.com/qX1eCt0OjDO.gif");
                    return;
                }

                if (receivedText.Contains("остановить работу сейчас же"))
                {
                    telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "Вырубаюсь");
                    return;
                }

                if (receivedText.Contains("добавить ссылку") || receivedText.Contains("добавь ссылку") || receivedText.Contains("добавить сравнение") || receivedText.Contains("добавь сравнение"))
                {
                    logger.LogInformation($"Запрошено добавление ссылки {receivedText}");
                    comparator.AddNewComparasign(telegram_bot, message);
                    return;
                }

                if (receivedText == "покажи сравнения" || receivedText == "покажи мои сравнения")
                {
                    logger.LogInformation($"Запрошен показ сравнений");
                    comparator.ComparasignFindAllInfo(interaction, interaction.Module[0]);
                    return;
                }

                if (receivedText.Contains("покажи сравнение") || receivedText.Contains("покажи сравнения") || receivedText.Contains("покажи мои сравнения"))
                {
                    logger.LogInformation($"Запрошен показ сравнений");
                    comparator.ComparasignFindAllInfo(interaction, interaction.Module[2]);
                    return;
                }

                if(receivedText.Contains("/imei ") && receivedText != "/imei" && receivedText != "/imei ")
                {
                    logger.LogInformation($"Запрошено получение информации о Pixel по IMEI");
                    SpecWriter_HTTP specWriter = new SpecWriter_HTTP();
                    long IMEI = checker.GetIMEI(receivedText);
                    if(IMEI != 0)
                    {
                        string info = await specWriter.GetInfoByIMEI(IMEI);
                        if(info != string.Empty && info != "Неверный IMEI")
                        {
                            telegram.SendInfoByIMEI(info, IMEI , message);
                        }
                        if(info == "Неверный IMEI")
                        {
                            telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: info);
                        }
                        if(info == string.Empty)
                        {
                            telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "При обработке запроса произошла ошибка, пожалуйста, повторите операцию");
                        }
                    }
                    else
                    {
                        telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: "Проверьте правильность введенного IMEI");
                    }
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
                        logger.LogDebug("Someone tried to use old interation");
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

}
