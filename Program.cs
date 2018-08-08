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

        // Hold boundary conditions for longtitude and latitude
        // Don't need to calculate more than once
        private static int _integralMaxLat;
        private static int _fractionalMaxLat;
        private static int _integralMinLat;
        private static int _fractionalMinLat; 
        private static int _integralMaxLong;
        private static int _fractionalMaxLong;  
        private static int _integralMinLong;
        private static int _fractionalMinLong;

        // Longitude and Latitude boundaries within which to create event locations
        // Example rectangle that describes the bulk of England without hitting sea
        // Bottom left 51.010299, -3.114624 (Taunton)
        // Bottom right 51.083686, -0.145569 (Mid Sussex)
        // Top left 53.810382, -3.048706 (Blackpool)
        // Top right 53.745462, -0.346069 (Hull)
        // Use these as default if not supplied in args
        private static decimal _maxLat = 53.810382m;
        private static decimal _minLat = 51.010299m;
        private static decimal _maxLong = -0.145569m;
        private static decimal _minLong = -3.048706m;
        
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

            SetLocationBoundaries(_maxLat, _minLat, _maxLong, _minLong);

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

        private static (decimal longtitude, decimal latitude) GetAlarmLocation()
        {
            Random latRandom = new Random(Environment.TickCount);
            int latIntegral = latRandom.Next(_integralMinLat, _integralMaxLat + 1);
            int latFractional = latRandom.Next(_fractionalMinLat, _fractionalMaxLat + 1);
            decimal latitude = latIntegral + (latFractional / 1000000m);

            Random longRandom = new Random(Environment.TickCount);
            int longIntegral = longRandom.Next(_integralMinLong, _integralMaxLong + 1);
            int longFractional = latRandom.Next(_fractionalMinLong, _fractionalMaxLong + 1);
            decimal longtitude = longIntegral + (longFractional / 1000000m);

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

        private static void SetLocationBoundaries(decimal maxLat, decimal minLat, decimal maxLong, decimal minLong)
        {
            // Break the coordinates into integral and fractional components
            // So that each part can be randomly created within the right
            // boundaries
            _integralMaxLat = (int) maxLat;
            decimal decFractionalMaxLat = maxLat - _integralMaxLat;
            _fractionalMaxLat = (int) (decFractionalMaxLat * GetMultiplyer(decFractionalMaxLat));
            
            _integralMinLat = (int) minLat;
            decimal decFractionalMinLat = minLat - _integralMinLat;
            _fractionalMinLat = (int) (decFractionalMinLat * GetMultiplyer(decFractionalMinLat));
            
            _integralMaxLong = (int)maxLong;
            decimal decFractionalMaxLong = maxLong - _integralMaxLong;
            _fractionalMaxLong = (int) (decFractionalMaxLong * GetMultiplyer(decFractionalMaxLong));
            
            _integralMinLong = (int)minLong;
            decimal decFractionalMinLong = minLong - _integralMinLong;
            _fractionalMinLong = (int) (decFractionalMinLong * GetMultiplyer(decFractionalMinLong)); 

            // Deal with negative Longtitudes, so that when getting random number the min and max work
            if (_fractionalMaxLong < 0 && _fractionalMinLong < 0)
            {
                // Swap them
                int tmpMax = _fractionalMaxLong;
                int tmpMin = _fractionalMinLong;

                _fractionalMaxLong = tmpMin;
                _fractionalMinLong = tmpMax;
            } 

            // Deal with negative Latitudes, so that when getting random number the min and max work
            if (_fractionalMaxLat < 0 && _fractionalMinLat < 0)
            {
                // Swap them
                int tmpMax = _fractionalMaxLat;
                int tmpMin = _fractionalMinLat;

                _fractionalMaxLat = tmpMin;
                _fractionalMinLat = tmpMax;
            } 
        }

        private static int GetMultiplyer(decimal value)
        {
            int factor;
            
            switch (value.ToString().Length)
            {
                case 1:
                    factor = 10;
                    break; 
                case 2:
                    factor = 100;
                    break; 
                case 3:
                    factor = 1000;
                    break;  
                case 4:
                    factor = 10000;
                    break;
                case 5:
                    factor = 100000;
                    break;
                default:
                    factor = 1000000;
                    break;
            }

            return factor;
        }
    }
}
