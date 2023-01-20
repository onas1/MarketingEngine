using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace TicketsConsole
{

        public class MarketingEngine
        {
            private readonly List<Event> _events;
            private readonly IDictionary<string, City> _cities;
            private readonly IDictionary<Event, int> _cachedDistances;
        public record City(string Name, int X, int Y);


        public MarketingEngine(List<Event> events, Dictionary<string, City> cities)
            {
                _events = events;
                _cities = cities;
                _cachedDistances = new Dictionary<Event, int>();
            }


            private static void NotificationService(Customer customer, Event SpecificEvent)
            {

                Console.WriteLine($"{customer.Name} from {customer.City} event {SpecificEvent.Name} at {SpecificEvent.Date}");
            }


            #region Task 2 response
            public void SendCustomerNotifications(Customer customer, Event SpecificEvent)
            {
                if (customer.City == SpecificEvent.City) NotificationService(customer, SpecificEvent);
            }
      
            #endregion

            #region Response to Task 3
            public void SendCustomerBirthDayNotification(Customer customer)
            {
                var closestEvent = GetClosestEventToCustomerBirthday(customer.BirthDate);
                NotificationService(customer, closestEvent);
            }

            //This method gets the event closest to the customer's birthday.
            private Event GetClosestEventToCustomerBirthday(DateTime birthday)
            {
                if (birthday > DateTime.UtcNow) birthday.AddYears(1);
                return _events.OrderBy(x => Math.Abs((birthday - x.Date).TotalSeconds)).First();
            }
        #endregion

            #region Response to Task 4,5 and 6
        public void NotifyCustomersBasedOnTopFiveClosestEventDistanceWithOptimization(Customer customer)
        {
            var customerCityInfo = _cities.FirstOrDefault(c => c.Key.Trim().ToLower() == customer.City.Trim().ToLower());
            var closestEvents = _events
                .Select(e => new { Event = e, Distance = GetDistance(customerCityInfo.Value, e) })
                .OrderBy(e => e.Distance).Take(5).Select(e => e.Event);

            foreach (var item in closestEvents)
            {
                NotificationService(customer, item);
            }
        }

        private int GetDistance(City customerCity, Event @event)
        {
            // check if the distance is already in the cache and returns it else, we get the distance by making the api call.
            var specificEvent = _cachedDistances.FirstOrDefault(x => x.Key == @event);
            if (specificEvent.Key != null)
            {
                return specificEvent.Value;
            }

            //handling possible error
            try
            {
                var eventCityInfo = _cities.FirstOrDefault(x => x.Key.Trim().ToLower() == @event.City.Trim().ToLower()).Value;
                var distance = Math.Abs(customerCity.X - eventCityInfo.X) + Math.Abs(customerCity.Y - eventCityInfo.Y);
                _cachedDistances.Add(@event, distance);
                return distance;
            }

            
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Console.WriteLine("The event city was not found.");
                            break;
                        case HttpStatusCode.Forbidden:
                            Console.WriteLine("You are not authorized to access this information.");
                            break;
                        case HttpStatusCode.RequestTimeout:
                            Console.WriteLine("The request timed out. Please try again later.");
                            break;
                        default:
                            Console.WriteLine("An error occurred while processing the request.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("An error occurred while processing the request.");
                }
                throw;
            }
        }
        #endregion
        public void NotifyCustomersBasedOnTopFiveClosestEventDistanceWithoutOptimization(Customer customer)
            {
                var distances = new Dictionary<Event, int>();
                customer.City = customer.City.Trim().ToLower();
                var customerCityInfo = _cities.First(c => c.Key.Trim().ToLower() == customer.City);

                foreach (var e in _events)
                {
                    e.City = e.City.Trim().ToLower();
                    var eventCityInfo = _cities.FirstOrDefault(c => c.Key.Trim().ToLower() == e.City).Value;

                    var distance = Math.Abs(customerCityInfo.Value.X - eventCityInfo.X) + Math.Abs(customerCityInfo.Value.Y - eventCityInfo.Y);
                    distances.Add(e, distance);
                }

                var closestEvents = distances.OrderBy(x => x.Value).Take(5).Select(x => x.Key);
                foreach (var item in closestEvents)
                {
                    NotificationService(customer, item);
                }

            }

        }
    }

