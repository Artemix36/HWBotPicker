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

}