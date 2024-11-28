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
                if(callbackQuery.Message is not null)
                {
                    ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                    comparasignPagesButtons.CreateOneCompButtons(phoneComparisons);
                    var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                    if (phoneComparisons.Length <= 1 && phoneComparisons[0].Phone1.Specs.CameraSpec != string.Empty && phoneComparisons[0].Phone2.Specs.CameraSpec != string.Empty)
                    {
                        Answer answer = new Answer();
                        string text = answer.OneCompMessage(phoneComparisons);

                        await TGAPI.telegram_bot.EditMessageText(callbackQuery.Message.Chat.Id ,callbackQuery.Message.Id, text, parseMode: ParseMode.Html);
                        await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                    }
                    else
                    {
                        await TGAPI.telegram_bot.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, text: $"Найденные сравнения:", parseMode: ParseMode.Html);
                        await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при изменении ответа: {ex.Message} {ex.Data}");
            }
        }

        public async void AllComparasignsByOnePhoneCallback(Comparasign[] phoneComparisons, CallbackQuery callbackQuery) //Показать все сравнения по телефону
        {
            try
            {
                if(callbackQuery.Message is not null)
                {
                    ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                    comparasignPagesButtons.CreateAllComparasignsButtons(phoneComparisons, null);
                    var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                    if (phoneComparisons.Length <= 1)
                    {
                        Answer answer = new Answer();
                        string text = answer.OneCompMessage(phoneComparisons);

                        await TGAPI.telegram_bot.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, text, parseMode: ParseMode.Html);
                        await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                    }
                    else
                    {
                        await TGAPI.telegram_bot.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, text: $"Найденные сравнения:", parseMode: ParseMode.Html);
                        await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при изменении ответа: {ex.Message} {ex.Data}");
            }
        }

        public async void ChangePageForComparasigns(Comparasign[] phoneComparisons, CallbackQuery callbackQuery) 
        {
            try
            {
                if(callbackQuery.Data is not null && callbackQuery.Message is not null)
                {
                    int page_num;
                    Int32.TryParse(callbackQuery.Data.Replace("page:", ""), out page_num);
                    ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                    comparasignPagesButtons.CreateAllComparasignsButtons(phoneComparisons, page_num);

                    var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                    await TGAPI.telegram_bot.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, text: $"Найденные сравнения:", parseMode: ParseMode.Html);
                    await TGAPI.telegram_bot.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, replyMarkup: comp_buttons);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при изменении ответа: {ex.Message} {ex.Data}");
            }
        }
    
    }

}