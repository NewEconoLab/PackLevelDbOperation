using Newtonsoft.Json.Linq;

namespace PackLevelDbOperation
{
    public class Setting
    {
        public string Conn_Track { get; private set; }
        public string DataBase_Track { get; private set; }
        public string Coll_Track { get; private set; }
        public int SleepTime { get; private set; }

        public Setting()
        {
            JObject json = JObject.Parse(System.IO.File.ReadAllText("config.json"));
            Conn_Track = (string)json["Conn_Track"];
            DataBase_Track = (string)json["DataBase_Track"];
            Coll_Track = (string)json["Coll_Track"];
            SleepTime = (int)json["sleepTime"];
        }
    }
}
