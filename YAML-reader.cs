using YamlDotNet.Serialization;

namespace YAMLvarsReader
{
    public class BotVars
    {
        public string TGtoken {get; set;}
        public string? GSMarenaBotToken {get; set;}
        public string? DBBaseURL {get; set;}
        public string? GSMarenaBotUrl {get; set;}
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
                return vars;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                vars.TGtoken = "not found";
                vars.DBBaseURL = "not found";
                vars.GSMarenaBotToken = "not found";
                vars.GSMarenaBotUrl = "not found";
                return vars;
            }
        }

    }

}