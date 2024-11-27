using HWPickerClassesLibrary;
using Telegram.Bot.Types;

namespace HW_picker_bot
{

    abstract class MessageBuilder
    {
        public string message {get; private set;} = string.Empty;
        public abstract string OneCompMessage(Comparasign[] phoneComparisons);

    }
    class Answer: MessageBuilder
    {
        public override string OneCompMessage(Comparasign[] phoneComparisons)
        {
            string answer = $"Найденное сравнение:\n<blockquote><b><u>{phoneComparisons[0].Phone1.Manufacturer} {phoneComparisons[0].Phone1.Model} </u></b>";
            
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU");

            if(phoneComparisons[0].Phone1.Specs.calculatedPrices.AvgPrice != 0)
            {
                var Price = phoneComparisons[0].Phone1.Specs.calculatedPrices.AvgPrice;
                answer = answer + $"\n(Средняя цена Авито ~ {Price.ToString("C", culture)} )";
            }
            if(phoneComparisons[0].Phone1.Specs.CameraSpec != string.Empty)
            {
                answer = answer + $" - <i>{phoneComparisons[0].Phone1.Specs.CameraSpec}</i></blockquote>\n\n";
            }

            answer = answer + $"<blockquote><b><u>{phoneComparisons[0].Phone2.Manufacturer} {phoneComparisons[0].Phone2.Model} </u></b>";

            if(phoneComparisons[0].Phone2.Specs.calculatedPrices.AvgPrice != 0)
            {
                var Price = phoneComparisons[0].Phone2.Specs.calculatedPrices.AvgPrice;
                answer = answer + $"\n(Средняя цена Авито ~ {Price.ToString("C", culture)} )";
            }
            if(phoneComparisons[0].Phone2.Specs.CameraSpec != string.Empty)
            {
                answer = answer + $" - <i>{phoneComparisons[0].Phone2.Specs.CameraSpec}</i></blockquote>\n\n";
            }

            return answer;
        }
    }

}