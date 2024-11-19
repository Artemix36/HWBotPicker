using Google.Protobuf.WellKnownTypes;
using YamlDotNet.Serialization;

namespace YAMLvarsReader
{
    public class BotVars
    {
        public string TGtoken {get; set;}  = "Not Found";
        public string? GSMarenaBotToken {get; set;}  = "Not Found";
        public string? DBBaseURL {get; set;} = "Not Found";
        public string? GSMarenaBotUrl {get; set;}  = "Not Found";
        public TimeSpan Timeout {get; set;}
        public string? StarttupMessage {get; set;} = "Привет! Добро пожаловать в HardWarePickerBot. Список модулей:";
        public string? ComparasignModuleMessage {get; set;} = "Нажми на кнопку ниже чтобы посмотреть все добавленные сравнения. Напиши покажи сравнения {phone_name} чтобы найти сравнения конкретного телефона. Напиши добавить сравнение или добавить ссылку {phone1} vs {phone2} чтобы добавить свое сравнение";
    }

    public class YamlReader
    {
        public BotVars ReadVars()
        {   
            BotVars vars = new BotVars();
            string path = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/var.yaml";
            Console.WriteLine("[INF] Reading vars from var.yaml");
            try
            {
                string readedFile = System.IO.File.ReadAllText(path);
                var deserializer = new DeserializerBuilder().Build();
                var result = deserializer.Deserialize<Dictionary<string, object>>(readedFile);
                
                vars.TGtoken = (string)result["TGtoken"];
                vars.DBBaseURL = (string)result["DBBaseURL"];
                vars.GSMarenaBotToken = (string)result["GSMarenaBotToken"];
                vars.GSMarenaBotUrl = (string)result["GSMarenaBotUrl"];
                vars.Timeout = ParseCustomTimeSpan((string)result["Timeout"]);
                vars.StarttupMessage = ProcessNewLines((string)result["StartupMessage"]);
                vars.ComparasignModuleMessage = ProcessNewLines((string)result["ComparasignModuleMessage"]);
                return vars;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return vars;
            }
        }
        static string? ProcessNewLines(string input)
        {
            return input?.Replace(@"\n", Environment.NewLine);
        }
        static TimeSpan ParseCustomTimeSpan(string input)
        {
            // Удаляем скобки и разделители
            string[] parts = input.Trim('(', ')').Split(',');

            // Преобразуем каждую часть в целое число
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);

            // Создаём объект TimeSpan
            return new TimeSpan(hours, minutes, seconds);
        }

    }

}