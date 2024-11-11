using YamlDotNet.Serialization;

namespace YAMLvarsReader
{
    public class BotVars
    {
        public string TGtoken {get; set;}  = "Not Found";
        public string? GSMarenaBotToken {get; set;}  = "Not Found";
        public string? DBBaseURL {get; set;} = "Not Found";
        public string? GSMarenaBotUrl {get; set;}  = "Not Found";
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
                return vars;
            }
        }

    }

}