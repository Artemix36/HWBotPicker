using HardWarePickerBot;

namespace HWpicker_bot
{
    internal class Phone
    {
        public string Manufacturer {get; set;} = string.Empty;
        public string Model {get; set;} = string.Empty;
        public string CameraSpec {get; set;} = string.Empty;

        public async Task GetCameraSpec()//Получение харак-к камер для телефона
        {
            if(this.IsPhoneWritten())
            {
                DB_HTTP_worker db = new DB_HTTP_worker();
                string ResultFromDB = await db.GetCameraSpec($"{this.Manufacturer} {this.Model}");
                if(ResultFromDB is not null)
                {
                    Console.WriteLine($"[INFO] Найдены хар-ки камер для {this.Manufacturer} {this.Model} в базе данных {ResultFromDB}. Результат записан в объект.");
                    this.CameraSpec = ResultFromDB;
                    return;
                }
                else
                {
                    SpecWriter_HTTP specWriter = new SpecWriter_HTTP();
                    string ResultFromGSM = await specWriter.FindAndWriteSpecs($"{this.Manufacturer} {this.Model}");
                    this.CameraSpec = ResultFromGSM;
                    return;
                }
            }
        }
        
        public bool IsPhoneWritten()
        {
            if(this.Model != string.Empty && this.Manufacturer != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}