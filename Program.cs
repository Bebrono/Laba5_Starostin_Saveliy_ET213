using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace laba5_Starostin_Saveliy_ET213
{
    internal class Program
    {
        public class Station
        {
            public string Name { get; set; }
            public List<Route> Routes { get; set; } = new List<Route>();
        }

        public class Route
        {
            public string RouteName { get; set; }
            public List<Station> Stations { get; set; } // Список станций в пути
            public List<int> TravelTimes { get; set; } // Список времени в пути между двумя соседними станциями 
            public int Start { get; set; }
            public int End { get; set; }
            public int Interval { get; set; }
        }

        public class Schedule
        {
            private Dictionary<string, Station> stations = new Dictionary<string, Station>();

            public void AddStation(string stationName)
            {
                if (!stations.ContainsKey(stationName))
                {
                    stations[stationName] = new Station { Name = stationName };
                }
            }

            public void AddRoute(string routeName, List<string> stationNames, List<int> travelTimes, int start, int end, int interval)
            {
                var route = new Route
                {
                    RouteName = routeName,
                    Stations = stationNames.Select(name => stations[name]).ToList(),
                    TravelTimes = travelTimes,
                    Start = start,
                    End = end,
                    Interval = interval
                };
                foreach (var station in route.Stations)
                {
                    station.Routes.Add(route);
                }
            }

            public List<string> GetDepartures(string stationName, int currentTime)
            {
                var result = new List<string>();

                if (!stations.ContainsKey(stationName))
                {
                    result.Add("Такой станции нет!");
                    return result;
                }

                var station = stations[stationName];

                foreach (var route in station.Routes)
                {
                    int stationIndex = route.Stations.IndexOf(station);
                    int[] nextDepartures = GetArrival(route, stationIndex, currentTime);
                    if (stationIndex != (route.Stations.Count - 1))
                    {
                        int minToArrive = nextDepartures[0] - currentTime;
                        result.Add($"{route.RouteName}, маршрутка с конечной точкой в {route.Stations[route.Stations.Count - 1].Name}, приедет через {minToArrive} мин.");
                    }
                    if (stationIndex != 0)
                    {
                        int minutesToArrive = nextDepartures[1] - currentTime;
                        result.Add($"{route.RouteName}, маршрутка с конечной точкой в {route.Stations[0].Name}, приедет через {minutesToArrive} мин.");
                    }
                }

                return result;
            }

            private int[] GetArrival(Route route, int stationIndex, int currTime)
            {
                int[] arriveTimes = new int[2] { -1, -1 };
                int timeToStation = 0;

                for (int i = 0; i < stationIndex; i++)
                {
                    timeToStation += route.TravelTimes[i];
                }

                int reverseTimeToStation = route.TravelTimes.Sum() - timeToStation;

                if ((currTime < route.Start + timeToStation) && (currTime > route.End + timeToStation))
                {
                    arriveTimes[0] = route.Start + timeToStation;
                }
                else
                {
                    int timeStart = currTime - route.Start;
                    int intervalsPass = timeStart / route.Interval;
                    int nextDeparture = route.Start + (intervalsPass + 1) * route.Interval;

                    while (currTime <= (nextDeparture - route.Interval + timeToStation))
                    {
                        nextDeparture -= route.Interval;
                    }
                    arriveTimes[0] = nextDeparture + timeToStation;
                }
                if ((currTime < route.Start + reverseTimeToStation) && (currTime > route.End + reverseTimeToStation))
                {
                    arriveTimes[1] = route.Start + reverseTimeToStation;
                }
                else
                {
                    int timeStart = currTime - route.Start;
                    int intervalsPass = timeStart / route.Interval;
                    int nextDeparture = route.Start + (intervalsPass + 1) * route.Interval;

                    nextDeparture = route.Start + (intervalsPass + 1) * route.Interval;

                    while (currTime <= (nextDeparture - route.Interval + reverseTimeToStation))
                    {
                        nextDeparture -= route.Interval;
                    }
                    arriveTimes[1] = nextDeparture + reverseTimeToStation;
                }
                return arriveTimes;

            }
        }

        static void Main(string[] args)
        {
            using (StreamReader reader = new StreamReader("../../schedule.txt"))
            {
                string line;
                Schedule schedule = new Schedule();

                while ((line = reader.ReadLine()) != null)
                {
                    string[] alls = line.Split(' ');
                    string routeName = alls[0];
                    string[] parts = alls[1].Split(':');

                    int firstDeparture = int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
                    parts = alls[2].Split(':');
                    int lastDeparture = int.Parse(parts[0]) * 60 + int.Parse(parts[1]);

                    int interval = int.Parse(alls[3]);

                    List<string> stationNames = new List<string>();
                    List<int> travelTimes = new List<int>();

                    for (int i = 4; i < alls.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            stationNames.Add(alls[i]);
                            schedule.AddStation(alls[i]);
                        }
                        else
                        {
                            travelTimes.Add(int.Parse(alls[i]));
                        }
                    }
                    schedule.AddRoute(routeName, stationNames, travelTimes, firstDeparture, lastDeparture, interval);
                }

                string enteredStation = "some";

                while (!string.IsNullOrEmpty(enteredStation))
                {
                    Console.Write("Введите станцию: ");
                    enteredStation = Console.ReadLine();

                    if (!string.IsNullOrEmpty(enteredStation))
                    {
                        Console.Write("Текущее время: ");
                        string timeInput = Console.ReadLine();
                        string[] timeParts = timeInput.Split(':');

                        int hours = int.Parse(timeParts[0]);
                        int mins = int.Parse(timeParts[1]);
                        int timeInMins = hours * 60 + mins;

                        Console.WriteLine("Расписание:");
                        List<string> result = schedule.GetDepartures(enteredStation, timeInMins);

                        foreach (var x in result)
                        {
                            Console.WriteLine(x);
                        }
                    }
                }
            }
        }
    }
}
