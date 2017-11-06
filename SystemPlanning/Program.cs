using System;
using System.Numerics;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SystemPlanning
{

    static class Constants {
        public const double DistanceToMerope = 871.018424994378;

        public static SystemPoint Merope = new SystemPoint
        {
            Name = "Merope",
            Coordinates = new Vector3 { X = -78.59375f, Y = -149.625f, Z = -340.53125f }
        };

        public static SystemPoint Col70 = new SystemPoint
        {
            Name = "Col 70",
            Coordinates = new Vector3 { X = 687.0625f, Y = -362.53125f, Z = -697.0625f }
        };
    }

    class Data {
        public float DistanceWeight = 0.00f;
        public float SphereRadius = 0.00f;
    }

    class SystemPoint
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("coords")]
        public Vector3 Coordinates { get; set; }
    }

    class Program
    {
        static Data storage = new Data();

        static void Main(string[] args)
        {
            Console.Write("How many Meropes from Col70 do you want to go: ");
            storage.DistanceWeight = float.Parse(Console.ReadLine());

            Console.Write("How many lightyears for candidate sphere radius: ");
            storage.SphereRadius = float.Parse(Console.ReadLine());

            Vector3 newPoint = Vector3.Lerp(Constants.Col70.Coordinates, Constants.Merope.Coordinates, storage.DistanceWeight);
            float distance = Vector3.Distance(Constants.Col70.Coordinates, newPoint);

            Console.WriteLine("~~ Enter Q to exit! ~~");
            Console.WriteLine();

            var exit = false;
            while(!exit) {
                Console.Write($"System to find candidates through near '{storage.DistanceWeight} Merope' from Col70: ");
                var newSystem = Console.ReadLine();
                if (newSystem == "q" || newSystem == "Q") {
                    exit = true;
                    continue;
                }

                CalculatePoint(newSystem, distance);

                var key = Console.ReadKey().KeyChar;
                if (key == 'q' || key == 'Q') {
                    exit = true;
                }
            }
        }

        static async void CalculatePoint(string newSystem, float distance) {
            SystemPoint system = await GetSystem(newSystem);
            float newDistance = Vector3.Distance(Constants.Col70.Coordinates, system.Coordinates);
            GetData(Constants.Col70, system, Vector3.Lerp(Constants.Col70.Coordinates, system.Coordinates, (distance/newDistance)));
        }

        static async Task<SystemPoint> GetSystem(string newSystem)
        {
            newSystem = HttpUtility.UrlEncode(newSystem);
            string baseURL = $"https://www.edsm.net/api-v1/system?systemName={newSystem}&showCoordinates=1";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage res = await client.GetAsync(baseURL))
            using (HttpContent content = res.Content)
            {
                string data = await content.ReadAsStringAsync();
                if (data != null)
                {
                    return JsonConvert.DeserializeObject<SystemPoint>(data);
                }
            }
            throw new Exception("System does not exist");
        }

        static async void GetData(SystemPoint origin, SystemPoint destination, Vector3 center) {
            string baseURL = $"https://www.edsm.net/api-v1/sphere-systems?x={center.X}&y={center.Y}&z={center.Z}&radius={storage.SphereRadius}&showCoordinates=1";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage res = await client.GetAsync(baseURL))
            using (HttpContent content = res.Content)
            {
                string data = await content.ReadAsStringAsync();
                if (data != null)
                {
                    var systems = JsonConvert.DeserializeObject<List<SystemPoint>>(data);
                    Console.WriteLine($"-- Candidate systems at '{storage.DistanceWeight} Merope' through {destination.Name} in a {storage.SphereRadius}ly sphere --");
                    foreach(SystemPoint system in systems) {
                        float distance = Vector3.Distance(origin.Coordinates, system.Coordinates);
                        Console.WriteLine($"{system.Name} @ {system.Coordinates}: {distance}ly");
                    }
                }
            }
        }
    }
}
