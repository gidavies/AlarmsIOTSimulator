using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace alarms
{
    class IOTSimulator
    {
        // Future params?
        // TL, TR, BL, BR for long, lat?
        // Status weighting?
        // Alarm image weighting?
        private static readonly HttpClient _client = new HttpClient();

        // Event Grid
        private static string _eventTopicEndpoint = null;
        private static string _eventTopicResource = null;
        private static string _eventAegSasKey = null;

        // Speed of event publishing, ms between each event
        private static int _eventInterval = 1000;

        // Images    
        private static string _falseAlarmImageURL = null;
        private static string _trueAlarmImageURL = null;
        
        // IOTSimulator 
        static void Main(string[] args)
        {
            string usageOutput = "Usage: dotnet run <EventTopicURL> <EventResourcePath> <EventKey> <FalseImageURL> <TrueImageURL> <EventInterval (int in ms)>";
            
            if (args.Length < 6)
            {
                System.Console.WriteLine("Please enter arguments.");
                System.Console.WriteLine(usageOutput);
                return;
            }

            _eventTopicEndpoint = args[0];
            _eventTopicResource = args[1];
            _eventAegSasKey = args[2];
            _falseAlarmImageURL = args[3];
            _trueAlarmImageURL = args[4];
            
            bool test = int.TryParse(args[5], out _eventInterval);
            if (test == false)
            {
                System.Console.WriteLine("Please enter an int for the milliseconds between event publishing.");
                System.Console.WriteLine(usageOutput);
                return;
            }

            Console.Write("Alarms will be sent every " + _eventInterval + "ms.");

            SimulateAlarms().Wait();
        }

        private static async Task SimulateAlarms()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("aeg-sas-key", _eventAegSasKey);

            while(true)
            {
                try
                {    
                    // Create a new alarm
                    var location = GetAlarmLocation();
                    Alarm reading = new Alarm {
                        status = GetAlarmStatus(), 
                        longtitude = location.longtitude, 
                        latitude = location.latitude, 
                        image = GetAlarmImage() };

                    // Create a new event
                    AlarmEvent alarmEvent = new AlarmEvent {
                        topic = _eventTopicResource,
                        subject = "Alarm", 
                        id = Guid.NewGuid().ToString(),
                        eventType = "recordInserted", 
                        eventTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFK"),
                        data = reading };

                    // Event Grid data is an array with one element
                    AlarmEvent[] alarmEvents = { alarmEvent };

                    // Post the data
                    HttpResponseMessage response = await _client.PostAsync(_eventTopicEndpoint, new JsonContent(alarmEvents));

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("\n" + reading.status + " alarm sent. Longtitude: " 
                        + reading.longtitude + " latitude: " + reading.latitude
                        + " image: " + reading.image);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending alarm:" + e.Message);
                }

                Thread.Sleep(_eventInterval);
            }
        }

        private static string GetAlarmStatus()
        {
            // Return (pseudo) random red, amber or green
            string alarmStatus = "green";
            Random random = new Random(Environment.TickCount);
            
            // Simplistic weighting to make the majority (8/10) green
            // 0 = red, 1 = amber, 2-9 = green
            int value = random.Next(10);

            switch (value)
            {
                case 0:
                    alarmStatus = "red";
                    break;
                case 1:
                    alarmStatus = "amber";
                    break;
                default:
                    alarmStatus = "green";
                    break;
            }

            return alarmStatus;
        }

        private static (double longtitude, double latitude) GetAlarmLocation()
        {
            // Return random latitude and longtitude (bound to an area?)

            // Rectangle that describes the bulk of the UK without hitting sea
            // Bottom left 51.010299, -3.114624 (Taunton)
            // Bottom right 51.083686, -0.145569 (Mid Sussex)
            // Top left 53.810382, -3.048706 (Blackpool)
            // Top right 53.745462, -0.346069 (Hull)
            // Therefore to set some easier boundaries approx:
            // BL: 51.0, -3.0
            // BR: 51.0, -0.0
            // TL: 54.0, -3.0
            // TR: 54.0, -0.0

            var maxLat = 54;
            var minLat = 51;
            var maxLong = -3;
            var minLong = 0;

            Random latRandom = new Random(Environment.TickCount);
            var latDecimal = latRandom.NextDouble();
            var latitude = latDecimal + latRandom.Next(minLat, maxLat);
            
            Random longRandom = new Random(Environment.TickCount);
            var longDecimal = longRandom.NextDouble();
            var longtitude = longDecimal + longRandom.Next(maxLong, minLong);

            return (longtitude, latitude);
        }
        
        private static string GetAlarmImage()
        {
            // Return either the good (e.g. cat) or bad (e.g. gang) image
            string alarmImage = null;
            Random random = new Random(Environment.TickCount);
            
            // Assumed 50/50 weighting?
            int value = random.Next(2);

            if (value == 0)
            {
                alarmImage = _trueAlarmImageURL;
            }
            else
            {
                alarmImage = _falseAlarmImageURL;
            }
            
            return alarmImage;
        }
    }
}
