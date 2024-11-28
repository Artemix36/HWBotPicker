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
    internal class MessageSending
    {
        public async void AllInfoAboutComparasingCallback(Comparasign[] phoneComparisons, Message Message) //Подробная информация о сравнении по сообщению
        {
            try
            {
                ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                comparasignPagesButtons.CreateOneCompButtons(phoneComparisons);
                var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                if (phoneComparisons[0].Phone1.Specs.CameraSpec != string.Empty && phoneComparisons[0].Phone2.Specs.CameraSpec != string.Empty)
                {
                    Answer answer = new Answer();
                    string text = answer.OneCompMessage(phoneComparisons);

                    await TGAPI.telegram_bot.SendMessage(Message.Chat.Id, text, parseMode: ParseMode.Html, replyMarkup: comp_buttons);
                }
                else
                {
                    await TGAPI.telegram_bot.SendMessage(Message.Chat.Id, "Найденные сравнения:", parseMode: ParseMode.Html, replyMarkup: comp_buttons);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при изменении ответа: {ex.Message} {ex.Data}");
            }
        }

        public async void AllComparasignsByOnePhoneCallback(Comparasign[] phoneComparisons, Message Message) //Показать все сравнения по телефону
        {
            try
            {
                ComparasignPagesButtons comparasignPagesButtons = new ComparasignPagesButtons();
                comparasignPagesButtons.CreateAllComparasignsButtons(phoneComparisons, null);
                var comp_buttons = new InlineKeyboardMarkup(comparasignPagesButtons.ComparasignButtons.Select(a => a.ToArray()).ToArray());

                if (phoneComparisons.Length <= 1)
                {
                    Answer answer = new Answer();
                    string text = answer.OneCompMessage(phoneComparisons);

                    await TGAPI.telegram_bot.SendMessage(Message.Chat.Id, text, parseMode: ParseMode.Html, replyMarkup: comp_buttons);
                }
                else
                {
                    await TGAPI.telegram_bot.SendMessage(Message.Chat.Id, "Найденные сравнения:", parseMode: ParseMode.Html, replyMarkup: comp_buttons);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при изменении ответа: {ex.Message} {ex.Data}");
            }
        }
    
    }

}