using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HW_picker_bot;
using HWpicker_bot;
using HWPickerClassesLibrary;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ZstdSharp.Unsafe;
using static HWpicker_bot.Compare;

namespace TelegramApi
{
    public class Interactions
    {
        public Message? Message {get; set;}
        public CallbackQuery? CallbackQuery {get; set;}
        public string From {get; set;} = string.Empty;
        public string[] Module { get; } = { "all_comp", "one_comp", "all_by_one_comp" };
    }

    internal class TGAPI
    {
        public static string? StartupMessage {get; set;} = string.Empty;
        public static string? ComparasignModuleMessage {get; set;} = string.Empty;
        public static ITelegramBotClient telegram_bot {get; set;}
        
        public void ComparasignPagesSend(Comparasign[] phoneComparisons, Message message, int page_now)//для отправки всех сравнений НОВОЕ
        {
            try
            {
                ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                comparasignPagesButtons.CreateAllComparasignsButtons(phoneComparisons, page_now);
                var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                sendMessage(telegram_bot, "text", message.Chat.Id, text: $"Найденные сравнения:", reply: message.MessageId, buttons: comp_buttons);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при отправке всех сравнений {ex.Message}");
            }
        }

        public void SendDataTable(Comparasign[] phoneComparisons, Message message) //отправить найденные сравнения
        {
            try
            {
                ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                comparasignPagesButtons.CreateOneCompButtons(phoneComparisons);
                var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());
                Answer answer = new Answer();
                string text = answer.OneCompMessage(phoneComparisons);

                if (phoneComparisons.Length <= 1 && phoneComparisons[0].Phone1.Specs.CameraSpec != string.Empty && phoneComparisons[0].Phone2.Specs.CameraSpec != string.Empty)
                {

                    sendMessage(telegram_bot, "text", message.Chat.Id, text: text, reply: message.MessageId, buttons: comp_buttons);
                }
                else
                {
                    sendMessage(telegram_bot, "text", message.Chat.Id, text: $"Найденные сравнения:", reply: message.MessageId, buttons: comp_buttons);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при отправке ответа: {ex.Message} {ex.Data}");
            }
        }
        public void SenAddTable(ITelegramBotClient telegram_bot, Comparasign phoneComparisons, Message message, string answer) //отправить добавленное сравнение
        {
            List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();
            comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithUrl($"{phoneComparisons.Phone1.Manufacturer} {phoneComparisons.Phone1.Model} vs {phoneComparisons.Phone2.Manufacturer} {phoneComparisons.Phone2.Model}", $"{phoneComparisons.CompareLink}")});
            comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData($"Добавлено by: @{phoneComparisons.AddedBy}", $"{phoneComparisons.Phone1.Manufacturer} {phoneComparisons.Phone1.Model} vs {phoneComparisons.Phone2.Manufacturer} {phoneComparisons.Phone2.Model}")});
            var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());

            sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[SUCCESS]</blockquote><b>{answer.Replace("[SUCCESS] ", "")}</b>", reply: message.MessageId, buttons: comp_buttons);
        }
        public void SendMainMenu(ITelegramBotClient telegram_bot, Message message) //отправить основное меню в котором содержатся все модули
        {
            try
            {
                List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();
                comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData($"Пользовательские сравнения камер смартфонов", "/comparasign")});
                var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());

                sendMessage(telegram_bot, "text", message.Chat.Id, text: StartupMessage, reply: message.MessageId, buttons: comp_buttons);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Не получилось отправить меню {ex.Message}");
            }
        }
        public void SendComparasignMenu(Message message) //отправить меню сравнений
        {
            try
            {
                List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();
                comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData($"Показать все сравнения", "comp main menu")});
                var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());

                sendMessage(telegram_bot, "text", message.Chat.Id, text: ComparasignModuleMessage, reply: message.Id, buttons: comp_buttons);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Не получилось отправить меню {ex.Message}");
            }
        }
        public void SendComparasignMenu(CallbackQuery callbackQuery) //отправить меню сравнений
        {
            try
            {
                List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();
                comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData($"Показать все сравнения", "comp main menu")});
                var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());

                sendMessage(telegram_bot, "text", callbackQuery.Message.Chat.Id, text: ComparasignModuleMessage, reply: callbackQuery.Message.Id, buttons: comp_buttons);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Не получилось отправить меню {ex.Message}");
            }
        }
        
       //Делал Адам
        public async void sendMessage(ITelegramBotClient bot, string type, long peer_id, int? reply = null, string? text = null, string? photo = null, string? document = null, InlineKeyboardMarkup? buttons = null)
        {
            if (type == "text" && text is not null)
            {
                await bot.SendMessage(chatId: peer_id, text: text, parseMode: ParseMode.Html, reply, replyMarkup: buttons);
                return;
            }
        }
        public (string, long) getName(Message message)
        {
            string name = string.Empty;
            long id = 0;
            var user = message.From;

            if (user is not null && user.FirstName != null)
            {
                name = user.FirstName;
                id = user.Id;
            }
            else
            {
                name = "Имя не найдено";
            }
            return (name, id);
        }
        public (string, long) getCallbackName(User user)
        {
            string name = string.Empty;
            long id = 0;

            if (user.FirstName != null)
            {
                name = user.FirstName;
                id = user.Id;
            }
            else
            {
                name = "Имя не найдено";
            }
            return (name, id);
        }
        public void SendUserLog(string answer, string module, Comparasign phoneComparison, Message message)
        {
            Compare compare = new Compare();
            if(module == "write_comp")
            {
                if(answer.Contains("[INPUT ERROR]"))
                {
                    sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[INPUT ERROR]</blockquote><b>\nПравила добавления сравнений:</b>\n-[phone1] vs [phone2] [link]\n-Разрешенные наименования моделей: pixel, iphone, huawei, vivo, xiaomi, oppo, oneplus, samsung, nothing\n-Может быть ваш ник и тэг занимают больше 30 символов?", reply: message.MessageId);
                }
                if(answer.Contains("[SUCCESS]") && phoneComparison is not null)
                {
                    SenAddTable(telegram_bot, phoneComparison, message, answer);
                }
                if(answer.Contains("[ERROR]"))
                {
                    sendMessage(telegram_bot, "text",message.Chat.Id, text: $"<blockquote>[USER ERROR]</blockquote><b>{answer.Replace("[ERROR] ", "")}</b>", reply: message.MessageId);
                }
            }
            if(module == "read_all_comp")
            {
                if(answer.Contains("ERROR"))
                {
                    sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Сравнения не найдены</b>", reply: message.MessageId);
                }
            }
            if(module == "read_comp")
            {
                if(answer.Contains("ERROR"))
                {
                    Console.WriteLine(answer);
                    sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Не найдены подобные сравнения</b>", reply: message.MessageId);
                }
                if(answer.Contains("BAD NAMES"))
                {
                    sendMessage(telegram_bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Неверно введено имя сравнения или телефонов! Проверьте синтаксис!</b>", reply: message.MessageId);
                }
            }
        }
        public void SendUserLog(string answer, string module, Comparasign phoneComparison, CallbackQuery callback)
        {
            Compare compare = new Compare();

            if(module == "read_all_comp")
            {
                if(answer.Contains("ERROR"))
                {
                    sendMessage(telegram_bot, "text", callback.Message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Сравнения не найдены</b>", reply: callback.Message.Id);
                }
            }
            if(module == "read_comp")
            {
                if(answer.Contains("ERROR"))
                {
                    Console.WriteLine(answer);
                    sendMessage(telegram_bot, "text", callback.Message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Не найдены подобные сравнения</b>", reply: callback.Message.Id);
                }
                if(answer.Contains("BAD NAMES"))
                {
                    sendMessage(telegram_bot, "text", callback.Message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Неверно введено имя сравнения или телефонов! Проверьте синтаксис!</b>", reply: callback.Message.Id);
                }
            }
        }
        public int levDistance(String sRow, String sCol) // Нахождение разницы между нужной строкой и инпутом
        {
            int RowLen = sRow.Length;
            int ColLen = sCol.Length;
            int RowIdx;
            int ColIdx;
            char Row_i;
            char Col_j;
            int cost;

            if (Math.Max(sRow.Length, sCol.Length) > Math.Pow(2, 31))
                throw (new Exception("\nMaximum string length in Levenshtein.iLD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sRow.Length, sCol.Length) + "."));

            if (RowLen == 0) return ColLen;

            if (ColLen == 0) return RowLen;

            int[] v0 = new int[RowLen + 1];
            int[] v1 = new int[RowLen + 1];
            int[] vTmp;

            for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
            {
                v0[RowIdx] = RowIdx;
            }

            for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
            {
                v1[0] = ColIdx;
                Col_j = sCol[ColIdx - 1];

                for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                {
                    Row_i = sRow[RowIdx - 1];

                    if (Row_i == Col_j)
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }

                    int m_min = v0[RowIdx] + 1;
                    int b = v1[RowIdx - 1] + 1;
                    int c = v0[RowIdx - 1] + cost;

                    if (b < m_min) m_min = b;
                    if (c < m_min) m_min = c;

                    v1[RowIdx] = m_min;
                }

                vTmp = v0;
                v0 = v1;
                v1 = vTmp;

            }

            int max = System.Math.Max(RowLen, ColLen);
            int result = ((100 * v0[RowLen]) / max);

            return result;
        }
    }
}
