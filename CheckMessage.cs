using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using HWpicker_bot;
using System.Text.RegularExpressions;
using Mysqlx;
using System.Xml.Linq;

namespace HardWarePickerBot
{
    public class CheckMessage
     {
        static string[] startWords = { "pixel", "iphone", "huawei", "vivo", "xiaomi", "oppo", "oneplus", "samsung", "nothing" };
        public string FixPhoneNameToUpper(string name)
        {
            string[] words = name.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                char[] letters = words[i].ToCharArray();
                if (words[i].ToLower() != "iphone") 
                { 
                letters[0] = char.ToUpper(letters[0]);
                }
                else
                {
                letters[1] = char.ToUpper(letters[1]);
                }
                words[i] = new string(letters);
            }
            string answer = null;
            for(int i = 0; i<words.Length; i++)
            {
                answer = answer + words[i] + " ";
            }
            return answer;
        }
        public (string, string) GetAddComparasignName(string msg) //получение имени сравнения при добавлении
        {
            string patternEasy = @"^(добавить ссылку|добавь ссылку|добавить сравнение|добавь сравнение)\s+(.+?)\s+vs\s+(.+?)(?=https?:\/\/(?:photos\.google\.com\/(?:share|album)\/|photos\.app\.goo\.gl\/))";
            Regex regexEasy = new Regex(patternEasy);
            Match MatchEasy = regexEasy.Match(msg.ToLower());

            if (MatchEasy.Success)
            {
                string name1 = MatchEasy.Groups[2].Value.Trim(' ');
                string name2 = MatchEasy.Groups[3].Value.Trim(' ');

                string pattern = $@"^({string.Join("|", startWords)})(\s([a-zA-Z]?\d{{1,2}})(\s?[a-zA-Z\s]{{0,7}}))?$";
                Regex regex = new Regex(pattern);

                if (regex.IsMatch(name1.ToLower()) && regex.IsMatch(name2.ToLower()))
                {
                    return (name1, name2);
                }
                else
                {
                    Console.WriteLine($"REGEX: name1 is {regex.IsMatch(name1.ToLower())} name2 is {regex.IsMatch(name2.ToLower())}");
                    return (null, null);
                }
            }
            else
            {
                return ("error", null);
            }

        }
        public (string, string) GetFindComparasignName(string msg) //получение имени сравнения при поиске
        {
            string patternEasy = @"^покажи сравнение\s+([a-zA-Z0-9\s]{1,20})(?:\s+vs\s+([a-zA-Z0-9\s]{1,15}))?$";
            string pattern = $@"^({string.Join("|", startWords)})(\s([a-zA-Z]?\d{{1,2}})(\s?[a-zA-Z\s]{{0,7}}))?$";
            Regex regex = new Regex(pattern);
            Regex regexEasy = new Regex(patternEasy);
            Match MatchEasy = regexEasy.Match(msg.ToLower());

            if (MatchEasy.Groups[1].Success && MatchEasy.Groups[2].Success) 
            {
                if (regex.IsMatch(MatchEasy.Groups[1].Value.ToLower()) && regex.IsMatch(MatchEasy.Groups[2].Value.ToLower()))
                {
                    return (MatchEasy.Groups[1].Value, MatchEasy.Groups[2].Value);
                }
                else
                {
                    Console.WriteLine("[INFO] Phone names are written badly!");
                    return (null, null);
                }
            }
            else if(regex.IsMatch(MatchEasy.Groups[1].Value.ToLower()))
            {
                return(MatchEasy.Groups[1].Value, null);
            }
            else
            {
                return (null, null);
            }

        } 
        public string GetReviewName(string msg) //получение имени отзыва
        {
            string pattern = $@"^({string.Join("|", startWords)})(\s+\d{{1,2}}\s?[\sa-zA-Z]{{0,10}})?$";
            
            return null;
        }
        public string GetLink(string msg) //Получение ссылки на сравнение
        {
            string patternLink = $@"^*https?:\/\/(?:photos\.google\.com\/(?:share|album)\/|photos\.app\.goo\.gl\/)[\w\-]+(?:\/?\?[\w=&amp;]*)?";
            Regex regexLink = new Regex(patternLink);
            Match Match = regexLink.Match(msg);
            if (Match.Success)
            {
                return Match.Value;
            }
            else
            {
                return "error";
            }
        }
        internal string GetDescription(string[] msg) //to be refactored
        {
                string description = null;

                for (int i = 0; i < msg.Length; i++)
                {
                    msg[i] = msg[i].Replace("\n", "");
                }

                int wordCounter = 0;
            try
            {
                for (int i = 0; i < msg.Length; i++)
                {

                    if (msg[i] == "Описание:")
                    {
                        i++;
                        while (!msg[i].Contains(";"))
                        {
                            description = description + msg[i] + " ";
                            i++;
                            wordCounter++;
                        }
                        msg[i] = msg[i].Trim(';');
                        description = description + msg[i] + " ";
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Пусто {e.Message}");
                return null;
            }
                if (wordCounter < 10 && wordCounter>0)
                {
                    return description;
                }
                else
                {       
                    return null;
                }
        }
        internal (string,string,string,string,string,string,string) GetSpecs(string[] msg) //to be refactored
        {
            string CPU = null;
            string screen = null;
            string Cameras = null;
            string Battery = null;
            string Reviews = null;
            string MidPrice = null;
            string description = GetDescription(msg);

            try
            {
                for (int i = 0; i < msg.Length; i++)
                {
                    if (msg[i].Contains("Процессор:"))
                    {
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            CPU = CPU + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        CPU = CPU + msg[i];
                    }
                    if (msg[i].Contains("Экран:"))
                    {
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            screen = screen + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        screen = screen + msg[i];
                    }
                    if (msg[i].Contains("Камера:"))
                    {
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            Cameras = Cameras + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        Cameras = Cameras + msg[i];
                    }
                    if (msg[i].Contains("Батарея:"))
                    {
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            Battery = Battery + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        Battery = Battery + msg[i];
                    }
                    if (msg[i].Contains("Отзывы:"))
                    {
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            Reviews = Reviews + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        Reviews = Reviews + msg[i];
                    }
                    if (msg[i].Contains("Средняя") && msg[i+1].Contains("цена:"))
                    {
                        i++;
                        i++;
                        while (!msg[i].Contains(';'))
                        {
                            MidPrice = MidPrice + msg[i] + " ";
                            i++;
                        }
                        msg[i] = msg[i].Trim(new char[] { ' ', ';' });
                        MidPrice = MidPrice + msg[i];
                    }
                }
            }
            catch {
                Console.WriteLine("Неверно заполнено");
                return (null, null, null, null, null, null, null);
            }

            return (CPU, screen,Cameras, Battery ,Reviews, MidPrice, description);
        }
        internal int GetRate(string[] msg) //to be reviewed
        {
            int rate = 0;

            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i].Length >= 1 && int.TryParse(msg[i], out rate))
                {
                    if (rate >= 1 && rate <= 5)
                    {
                        return rate;
                    }
                }
            }
            return 0;
        }
     }
}
