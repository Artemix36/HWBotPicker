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
    internal class CallBackEditing
    {
        public async void AllInfoAboutComparasingCallback(Comparasign[] phoneComparisons, CallbackQuery callbackQuery) //Подробная информация о сравнении по нажатию кнопки
        {
            try
            {
                List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();

                for (int i = 0; i <= phoneComparisons.Length - 1; i++)
                {
                    List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                    row.Add(InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}", $"{phoneComparisons[i].CompareLink}"));
                    comp_array.Add(row);
                    List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>();
                    row2.Add(InlineKeyboardButton.WithCallbackData($"Все сравнения {phoneComparisons[i].Phone1.Manufacturer}", $"[{phoneComparisons[i].Phone1.Manufacturer}"));
                    row2.Add(InlineKeyboardButton.WithCallbackData($"Все сравнения {phoneComparisons[i].Phone2.Manufacturer}", $"[{phoneComparisons[i].Phone2.Manufacturer}"));
                    comp_array.Add(row2);
                }

                var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());
                if (phoneComparisons.Length <= 1 && phoneComparisons[0].Phone1.Specs.CameraSpec != string.Empty && phoneComparisons[0].Phone2.Specs.CameraSpec != string.Empty)
                {
                    Answer answer = new Answer();
                    string text = answer.OneCompMessage(phoneComparisons);
                    await TGAPI.telegram_bot.EditMessageText(callbackQuery.ChatInstance ,callbackQuery.Message.Id, text, parseMode: ParseMode.Html);
                    await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.ChatInstance, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                }
                else
                {
                    await TGAPI.telegram_bot.EditMessageText(callbackQuery.ChatInstance, callbackQuery.Message.Id, text: $"Найденные сравнения:", parseMode: ParseMode.Html);
                    await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.ChatInstance, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при отправке ответа: {ex.Message} {ex.Data}");
            }
        }

        public async void AllComparasignsByOnePhoneCallback(Comparasign[] phoneComparisons, Message message, int replyID)
        {
            try
            {
                List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();

                for (int i = 0; i <= phoneComparisons.Length - 1; i++)
                {
                    List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                        row.Add(InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}", $"{phoneComparisons[i].CompareLink}"));
                        row.Add(InlineKeyboardButton.WithCallbackData($"Добавлено by: @{phoneComparisons[i].AddedBy}", $"[{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}"));
                    comp_array.Add(row);
                }
                var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());

                if (phoneComparisons.Length <= 1)
                {
                    Answer answer = new Answer();
                    string text = answer.OneCompMessage(phoneComparisons);

                    await TGAPI.telegram_bot.EditMessageText(message.Chat.Id, replyID, text, parseMode: ParseMode.Html);
                    await TGAPI.telegram_bot.EditMessageReplyMarkup(message.Chat.Id, replyID, replyMarkup: comp_buttons);
                }
                else
                {
                    await TGAPI.telegram_bot.EditMessageText(message.Chat.Id, replyID, text: $"Найденные сравнения:", parseMode: ParseMode.Html);
                    await TGAPI.telegram_bot.EditMessageReplyMarkup(message.Chat.Id, replyID, replyMarkup: comp_buttons);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при отправке ответа: {ex.Message} {ex.Data}");
            }
        }
    
    }

}