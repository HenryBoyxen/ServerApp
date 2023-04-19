using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleGUI.Classes
{
    public static class DateTimeExtension
    {
        public static DateTime GetMondayOfWeek(DateTime dateTime)
        {
            while (dateTime.DayOfWeek != DayOfWeek.Monday)
            {
                dateTime = dateTime.AddDays(-1);
            }
            return dateTime;
        }

        public static int GetDaysBetweenDates(DateTime dt1, DateTime dt2)
        {
            TimeSpan timeSpan = (dt1 < dt2) ? (dt2 - dt1) : (dt1 - dt2);
            return (int)Math.Ceiling(timeSpan.TotalDays);
        }
    }
}
