namespace Core.Infra;

public static class DateHelper
{
    public static DateOnly DateOfNthWeekOfTheMonthDate(int year, int month, int nthWeek, DayOfWeek dayOfWeek)
    {
        var date = new DateOnly(year, month, 1);

        var n = 0;

        while (true)
        {
            if (date.DayOfWeek == dayOfWeek)
            {
                n++;
                if (n == nthWeek)
                    return date;
            }
            date = date.AddDays(1);
            if (date.Year > year) throw new Exception($"Did not find {nthWeek}th {dayOfWeek} of {month}-{year}");
        }

        return date;
    }

}