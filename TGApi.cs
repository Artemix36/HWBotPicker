using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HWpicker_bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static HWpicker_bot.Compare;

namespace TelegramApi
{
    internal class TGAPI
    {
        public void sendDataTable(ITelegramBotClient telegram_bot, Comparasign[] phoneComparisons, Message message)
        {
            TGAPI telegram = new TGAPI();
            List<List<InlineKeyboardButton>> comp_array = new List<List<InlineKeyboardButton>>();
            for (int i = 0; i <= phoneComparisons.Length - 1; i++)
            {
                comp_array.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1Name} vs {phoneComparisons[i].Phone2Name}", $"{phoneComparisons[i].CompLink}"), });
            }
            var comp_buttons = new InlineKeyboardMarkup(comp_array.Select(a => a.ToArray()).ToArray());
            if (phoneComparisons.Length <= 1)
            {
                telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: $"Найденные сравнения от пользователя {phoneComparisons[0].AddedBy}:\n<blockquote><b><u>{phoneComparisons[0].Phone1Name}</u></b> - <i>{phoneComparisons[0].Phone1CameraSpec}</i></blockquote>\n\n<blockquote><b><u>{phoneComparisons[0].Phone2Name}</u></b> - <i>{phoneComparisons[0].Phone2CameraSpec}</i></blockquote>", reply: message.MessageId, buttons: comp_buttons);
            }
            else
            {
                telegram.sendMessage(telegram_bot, "text", message.Chat.Id, text: $"Найденные сравнения от пользователя {phoneComparisons[0].AddedBy}:", reply: message.MessageId, buttons: comp_buttons);
            }
        }

        public async void sendMessage(ITelegramBotClient bot, string type, long peer_id, int? reply = null, string text = null, string photo = null, string document = null, InlineKeyboardMarkup buttons = null)
        {
            if (type == "text")
            {
                await bot.SendTextMessageAsync(chatId: peer_id, text: text, parseMode: ParseMode.Html, replyToMessageId: reply, replyMarkup: buttons);
                return;
            }

            if (type == "document")
            {
                await bot.SendDocumentAsync(chatId: peer_id, caption: text, parseMode: ParseMode.Html, replyToMessageId: reply, document: InputFile.FromUri(document));
                return;
            }
        }

        public (string, long) getName(Message message)
        {
            string name = null;
            long id = 0;
            var user = message.From;

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

        public (string, long) getCallbackName(User user)
        {
            string name = null;
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

        public void SendUserLog(string answer, string module, ITelegramBotClient bot, Message message)
        {
            Compare compare = new Compare();
            if(module == "write_comp")
            {
                if(answer.Contains("[INPUT ERROR]"))
                {
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[INPUT ERROR]</blockquote><b>\nПравила добавления сравнений:</b>\n-[phone1] vs [phone2] [link]\n-Разрешенные наименования моделей: pixel, iphone, huawei, vivo, xiaomi, oppo, oneplus, samsung, nothing\n-Может быть ваш ник и тэг занимают больше 30 символов?", reply: message.MessageId);
                }
                if(answer.Contains("[SUCCESS]"))
                {
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[SUCCESS]</blockquote><b>{answer.Replace("[SUCCESS] ", "")}</b>", reply: message.MessageId);
                }
                if(answer.Contains("[ERROR]"))
                {
                    sendMessage(bot, "text",message.Chat.Id, text: $"<blockquote>[USER ERROR]</blockquote><b>{answer.Replace("[ERROR] ", "")}</b>", reply: message.MessageId);
                }
            }
            if(module == "read_all_comp")
            {
                if(answer.Contains("ERROR") && message.From.Username != "Artemix36")
                {
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Не найдены сравнения от этого пользователя!</b>\nЗапрос сравнений от создателя бота", reply: message.MessageId);
                    compare.comparasing_photo_read(bot, message, "Artemix36");
                }
                if(answer.Contains("ERROR") && message.From.Username == "Artemix36")
                {
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Сравнения не найдены</b>", reply: message.MessageId);
                }
            }
            if(module == "read_comp")
            {
                if(answer.Contains("ERROR") && message.From.Username != "Artemix36")
                {
                    Console.WriteLine(answer);
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Не найдены сравнения от этого пользователя!</b>\nЗапрос сравнений от создателя бота", reply: message.MessageId);
                    compare.comparasing_find(bot, message, "Artemix36");
                }
                if(answer.Contains("ERROR") && message.From.Username == "Artemix36")
                {
                    Console.WriteLine(answer);
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Не найдены подобные сравнения</b>", reply: message.MessageId);
                }
                if(answer.Contains("BAD NAMES"))
                {
                    sendMessage(bot, "text", message.Chat.Id, text: $"<blockquote>[ERROR]</blockquote><b>Неверно введено имя сравнения или телефонов! Проверьте синтаксис!</b>", reply: message.MessageId);
                }
            }
        }
        public int levDistance(String sRow, String sCol) // 0 - same strings, 100 - totally different
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
