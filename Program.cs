using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Booking
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public Booking(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date");

        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public IEnumerable<DateTime> GetAllDates()
    {
        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}

public class BookingAnalyzer
{
    public static Dictionary<int, int> CountBookingsPerDay(List<Booking> bookings)
    {
        var dailyCounts = new Dictionary<int, int>();

        foreach (var booking in bookings)
        {
            foreach (var date in booking.GetAllDates())
            {
                if (!dailyCounts.ContainsKey(date.Day))
                {
                    dailyCounts[date.Day] = 0;
                }
                dailyCounts[date.Day]++;
            }
        }

        return dailyCounts;
    }

    public static List<Booking> MergeOverlappingBookings(List<Booking> bookings)
    {
        if (bookings.Count == 0)
            return new List<Booking>();

        var sortedBookings = bookings.OrderBy(b => b.StartDate).ToList();
        var mergedBookings = new List<Booking> { sortedBookings[0] };

        for (int i = 1; i < sortedBookings.Count; i++)
        {
            var lastMerged = mergedBookings.Last();
            var current = sortedBookings[i];

            if (current.StartDate <= lastMerged.EndDate.AddDays(1))
            {
                var newEndDate = lastMerged.EndDate > current.EndDate ? lastMerged.EndDate : current.EndDate;
                mergedBookings[mergedBookings.Count - 1] = new Booking(lastMerged.StartDate, newEndDate);
            }
            else
            {
                mergedBookings.Add(current);
            }
        }

        return mergedBookings;
    }

    public static void PrintDailyCounts(Dictionary<int, int> dailyCounts, int month, int year)
    {
        Console.WriteLine($"Booking counts for {new DateTime(year, month, 1):MMMM yyyy}:");
        Console.WriteLine("Day | Count");
        Console.WriteLine("----|------");

        int daysInMonth = DateTime.DaysInMonth(year, month);
        for (int day = 1; day <= daysInMonth; day++)
        {
            int count = dailyCounts.TryGetValue(day, out var c) ? c : 0;
            Console.WriteLine($"{day,3} | {count}");
        }
    }
}
namespace Date_Range_Aggregator__Bookings_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var bookings = new List<Booking>();
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter booking range (MM/DD- MM/DD) or 'done' to finish:");
                    string input = Console.ReadLine();

                    if (input.ToLower() == "done")
                        break;

                    var parts = input.Split('-');
                    if (parts.Length != 2)
                        throw new FormatException("Use format MM/DD - MM/DD");

                    var startParts = parts[0].Trim().Split('/');
                    var endParts = parts[1].Trim().Split('/');

                    var startDate = new DateTime(currentYear, int.Parse(startParts[0]), int.Parse(startParts[1]));
                    var endDate = new DateTime(currentYear, int.Parse(endParts[0]), int.Parse(endParts[1]));

                    bookings.Add(new Booking(startDate, endDate));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}. Please try again.");
                }
            }

            if (bookings.Count > 0)
            {
                var dailyCounts = BookingAnalyzer.CountBookingsPerDay(bookings);
                BookingAnalyzer.PrintDailyCounts(dailyCounts, currentMonth, currentYear);

                var mergedBookings = BookingAnalyzer.MergeOverlappingBookings(bookings);
                Console.WriteLine("\nMerged Booking Ranges:");
                foreach (var booking in mergedBookings)
                {
                    Console.WriteLine($"{booking.StartDate:MM/dd} - {booking.EndDate:MM/dd}");
                }

                var mergedCounts = BookingAnalyzer.CountBookingsPerDay(mergedBookings);
                Console.WriteLine("\nDaily Counts (Merged View - No Overlaps):");
                BookingAnalyzer.PrintDailyCounts(mergedCounts, currentMonth, currentYear);
            }
            else
            {
                Console.WriteLine("No bookings entered.");
            }
        }
    }
}
