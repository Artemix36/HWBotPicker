using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using HWPickerClassesLibrary;

namespace HW_picker_bot
{
    abstract class InlineKeyBoardBuilder
    {
        public List<List<InlineKeyboardButton>> ComparasignButtons = new List<List<InlineKeyboardButton>>();
        public abstract List<List<InlineKeyboardButton>> CreateAllComparasignsButtons(Comparasign[] phoneComparisons, int? page_now);

        public abstract List<List<InlineKeyboardButton>> CreateOneCompButtons(Comparasign[] phoneComparisons);

        public abstract List<List<InlineKeyboardButton>> CrateComparasignsAllInfo(Comparasign[] phoneComparisons);
    }
    class ComparasignPagesButtons: InlineKeyBoardBuilder
    {
        public override List<List<InlineKeyboardButton>> CreateAllComparasignsButtons(Comparasign[] phoneComparisons, int? page_now)
        {
            for (int i = 0; i <= phoneComparisons.Length - 1; i++)
            {
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                    row.Add(InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}", $"{phoneComparisons[i].CompareLink}"));
                    row.Add(InlineKeyboardButton.WithCallbackData($"Добавлено by: @{phoneComparisons[i].AddedBy}", $"[{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}"));
                this.ComparasignButtons.Add(row);
            }

            if (page_now is not null)
            {

                if (phoneComparisons.Length == 5)
                {
                    List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>();
                    row2.Add(InlineKeyboardButton.WithCallbackData($"Следующая страница >>", $"page:{page_now + 1}"));
                    this.ComparasignButtons.Add(row2);
                }

                if (page_now > 1)
                {
                    List<InlineKeyboardButton> row3 = new List<InlineKeyboardButton>();
                    row3.Add(InlineKeyboardButton.WithCallbackData($"Предыдущая страница <<", $"page:{page_now - 1}"));
                    this.ComparasignButtons.Add(row3);
                }

                return this.ComparasignButtons;
            }

            return this.ComparasignButtons;
        }

        public override List<List<InlineKeyboardButton>> CreateOneCompButtons(Comparasign[] phoneComparisons)
        {
            for (int i = 0; i <= phoneComparisons.Length - 1; i++)
            {
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                row.Add(InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}", $"{phoneComparisons[i].CompareLink}"));
                this.ComparasignButtons.Add(row);
                List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>();
                row2.Add(InlineKeyboardButton.WithCallbackData($"Все сравнения {phoneComparisons[i].Phone1.Manufacturer}", $"[{phoneComparisons[i].Phone1.Manufacturer}"));
                row2.Add(InlineKeyboardButton.WithCallbackData($"Все сравнения {phoneComparisons[i].Phone2.Manufacturer}", $"[{phoneComparisons[i].Phone2.Manufacturer}"));
                this.ComparasignButtons.Add(row2);
            }
            return this.ComparasignButtons;
        }

        public override List<List<InlineKeyboardButton>> CrateComparasignsAllInfo(Comparasign[] phoneComparisons)
        {
            for (int i = 0; i <= phoneComparisons.Length - 1; i++)
            {
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                row.Add(InlineKeyboardButton.WithUrl($"{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}", $"{phoneComparisons[i].CompareLink}"));
                row.Add(InlineKeyboardButton.WithCallbackData($"Добавлено by: @{phoneComparisons[i].AddedBy}", $"[{phoneComparisons[i].Phone1.Manufacturer} {phoneComparisons[i].Phone1.Model} vs {phoneComparisons[i].Phone2.Manufacturer} {phoneComparisons[i].Phone2.Model}"));
                this.ComparasignButtons.Add(row);
            }
            return this.ComparasignButtons;
        }

    }

}