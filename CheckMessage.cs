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
using TelegramApi;
using System.Runtime.CompilerServices;

namespace HardWarePickerBot
{
    public class CheckMessage
    {
        static public string[] startWords = { "pixel", "iphone", "huawei", "vivo", "xiaomi", "oppo", "oppo find" , "oneplus", "samsung", "nothing", "samsung galaxy", "samsung galaxy note"};
        public string FixPhoneNameToUpper(string name)//Исправление написания регистра имени телефона
        {
            string[] words = name.Split(' ');
            string answer = "";
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
            for(int i = 0; i<words.Length; i++)
            {
                answer = answer + words[i] + " ";
            }
            return answer.Trim(' ');
        }
        public string CompareLevDistance(string name)//исправление имен телефонов если есть опечатки
        {
            TGAPI tg = new TGAPI();
            string answer = "";
            try
            {
                string[] words = name.Split(" ");
                for(int h = 0; h<words.Length; h++)
                {
                    for(int i = 0; i<startWords.Length; i++)
                    {
                        if(tg.levDistance(words[h], startWords[i]) > 0 && tg.levDistance(words[h], startWords[i]) < 35)
                        {
                            answer = $"{startWords[i]} ";
                            for(int j = 1; j<words.Length; j++)
                            {
                                answer = answer + $"{words[j]} ";
                            }
                            Console.WriteLine($"[INFO] Заменена опечатка {words[h]} -> {startWords[i]}. Отдано: {answer}");
                            return answer.Trim(' ');
                        };
                    }
                }
                return name.Trim(' ');
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] ошибка при попытке исправления опечатки {ex.Message} {ex.StackTrace} {ex.ToString()}");
                return name;
            }
        }
        public (string, string, string, string) GetAddComparasignName(string? msg)//получение имени сравнения при добавлении
        {
            if(msg is not null)
            {
                string patternEasy = @"^(добавить ссылку|добавь ссылку|добавить сравнение|добавь сравнение)\s+(.+?)\s+vs\s+(.+?)(?=https?:\/\/(?:photos\.google\.com\/(?:share|album)\/|photos\.app\.goo\.gl\/))";
                Regex regexEasy = new Regex(patternEasy);
                Match MatchEasy = regexEasy.Match(msg.ToLower());

                if (MatchEasy.Success)
                {
                    string name1 = CompareLevDistance(MatchEasy.Groups[2].Value.Trim(' ')).Trim(' ');
                    string name2 = CompareLevDistance(MatchEasy.Groups[3].Value.Trim(' ')).Trim(' ');
                    
                    string pattern = $@"^({string.Join("|", startWords)})\s((?:[a-zA-Z]?\d{{1,2}})(?:\s?[a-zA-Z\s]{{0,7}}))?$";
                    Regex regex = new Regex(pattern);
                    Match Name1Match = regex.Match(name1.ToLower());
                    Match Name2Match = regex.Match(name2.ToLower());

                    if (regex.IsMatch(name1.ToLower()) && regex.IsMatch(name2.ToLower()))
                    {
                        return (FixPhoneNameToUpper(Name1Match.Groups[1].Value.Trim(' ')), FixPhoneNameToUpper(Name1Match.Groups[2].Value.Trim(' ')), FixPhoneNameToUpper(Name2Match.Groups[1].Value.Trim(' ')), FixPhoneNameToUpper(Name2Match.Groups[2].Value.Trim(' ')));
                    }
                    else
                    {
                        Console.WriteLine($"REGEX: name1 is {regex.IsMatch(name1.ToLower())} name2 is {regex.IsMatch(name2.ToLower())}");
                        return (string.Empty, string.Empty, string.Empty, string.Empty);
                    }
                }
                else
                {
                    return ("error", string.Empty, string.Empty, string.Empty);
                }
            }
            Console.WriteLine("[ERROR] message was nulls");
            return (string.Empty, string.Empty, string.Empty, string.Empty);
        }
        public (string, string) GetManufacturerAndModel(string name)//Найти производителя и модель
        {
            string pattern = $@"^({string.Join("|", startWords)})(\s([a-zA-Z]?\d{{1,2}})(\s?[a-zA-Z\s]{{0,7}}))?$";
            Regex regex = new Regex(pattern);
            Match Name1Match = regex.Match(name.ToLower());

            
            if (regex.IsMatch(name.ToLower()))
            {
                return (Name1Match.Groups[1].Value.Trim(' '), Name1Match.Groups[2].Value.Trim(' '));
            }
            else
            {
                return (string.Empty, string.Empty);
            }
        }
        public (string, string) ParseRequestName(string text)//Найти два или одно полное имя телефона
        {
            string ProcessedText = text.ToLower().Replace("покажи сравнение ","");
            if(ProcessedText.Contains("vs"))
            {
                string pattern = $@"^({string.Join("|", startWords)})(\s([a-zA-Z]?\d{{1,2}})(\s?[a-zA-Z\s]{{0,7}}))?$";
                Regex regex = new Regex(pattern);
                string[] words = ProcessedText.Split(" vs ");
                if(regex.IsMatch(words[0].Trim(' ')) && regex.IsMatch(words[1].Trim(' ')))
                {
                    return (words[0], words[1]);
                }
                else
                {
                    return (string.Empty, string.Empty);
                }
            }
            if(!ProcessedText.Contains("vs"))
            {
                string pattern = $@"^({string.Join("|", startWords)})(\s([a-zA-Z]?\d{{1,2}})(\s?[a-zA-Z\s]{{0,7}}))?$";
                Regex regex = new Regex(pattern);
                if(regex.IsMatch(ProcessedText.Trim(' ')))
                {
                    return (ProcessedText, string.Empty);
                }
                else
                {
                    return (string.Empty, string.Empty);
                }
            }
            return (string.Empty, string.Empty);
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
        
        
        //Секция с отзывами на телефоны пока что не переработана #TO BE DONE
        public string GetReviewName(string msg) //получение имени отзыва
        {
            string pattern = $@"^({string.Join("|", startWords)})(\s+\d{{1,2}}\s?[\sa-zA-Z]{{0,10}})?$";
            
            return string.Empty;
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
