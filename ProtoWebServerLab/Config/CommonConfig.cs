namespace ProtoWebServerLab.Config
{
    public class Address
    {
        public string ip { get; set; } = string.Empty;
        public int port { get; set; }
        public string url { get; set; } = string.Empty;
        public int db_index { get; set; }
    }
}
