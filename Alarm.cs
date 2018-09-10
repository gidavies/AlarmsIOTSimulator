namespace alarms
{
    public class Alarm
    {
        public int deviceId {get; set; }
        public string status { get; set; }
        public decimal longitude { get; set; }
        public decimal latitude { get; set; }
        public string image { get; set; }
        public string name {get; set;}
        public string text {get; set;}
    }
}
