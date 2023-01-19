using System;
using System.Collections.Generic;
using System.Linq;
/*
Let&#39;s say we&#39;re running a small entertainment business as a start-up. This means we&#39;re selling tickets to live events
on a website. An email campaign service is what we are going to make here. We&#39;re building a marketing engine that
will send notifications (emails, text messages) directly to the client and we&#39;ll add more features as we go.
Please, instead of debuging with breakpoints, debug with "Console.Writeline();" for each task because the Interview
will be in Coderpad and in that platform you cant do Breakpoints.
*/
namespace TicketsConsole
{
    internal partial class Program
    {
        public class MarketingEngine
        {
            private readonly List<Event> _events;
            private readonly IDictionary<string, City> _cities;
            private readonly IDictionary<Event, int> _cachedDistances;


            public MarketingEngine(List<Event> events, Dictionary<string, City> cities)
            {
                _events = events;
                _cities = cities;
                _cachedDistances = new Dictionary<Event, int>();
            }


            //This method gets the event closest to the customer's birthday.
            private Event GetClosestEventToCustomerBirthday(DateTime birthday)
            {
                if (birthday > DateTime.UtcNow) birthday.AddYears(1);
                return _events.OrderBy(x => Math.Abs((birthday - x.Date).TotalSeconds)).First();
            }
            private static void NotificationService(Customer customer, Event SpecificEvent)
            {
                
                Console.WriteLine($"{customer.Name} from {customer.City} event {SpecificEvent.Name} at {SpecificEvent.Date}");
            }


            public void SendCustomerNotifications(Customer customer, Event SpecificEvent) 
            {
                if (customer.City == SpecificEvent.City) NotificationService(customer, SpecificEvent);
            }
            public void SendCustomerBirthDayNotification(Customer customer)
            {
              var closestEvent = GetClosestEventToCustomerBirthday(customer.BirthDate);
                NotificationService(customer, closestEvent);
            }

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

            public void NotifyCustomersBasedOnTopFiveClosestEventDistanceWithOptimization(Customer customer)
            {
                var customerCityInfo = _cities.FirstOrDefault(c => c.Key.Trim().ToLower() == customer.City.Trim().ToLower());
                var closestEvents = _events
                    .Select(e => new { Event = e, Distance = GetDistance(customerCityInfo.Value, e) })
                    .OrderBy(e => e.Distance)
                    .Take(5)
                    .Select(e => e.Event);
                foreach (var item in closestEvents)
                {
                    NotificationService(customer, item);
                }
            }

            //This method get the distance between the customer's city and an event.
            private int GetDistance(City customerCity, Event @event)
            {
                // check if the distance is already in the cache and returns it else, we get the distance by making the api call.
                var specificEvent = _cachedDistances.FirstOrDefault(x => x.Key == @event);
                if (specificEvent.Key != null)
                {
                    return specificEvent.Value;
                }

                //gets the distance from calling the api , adds it to the cache and return the value.
                var eventCityInfo = _cities.FirstOrDefault(x => x.Key.Trim().ToLower() == @event.City.Trim().ToLower()).Value;
                var distance = Math.Abs(customerCity.X - eventCityInfo.X) + Math.Abs(customerCity.Y - eventCityInfo.Y);
                _cachedDistances.Add(@event, distance);
                return distance;
            }

        }
    }
}
