using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.extensions
{
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Attempts to parse a date time offset using the specified format string.
        /// </summary>
        /// <returns> Returns the result if parsed successfully.  Otherwise returns null. </returns>
        public static DateTime? ParseDateTime(this string dateTimeString, string formatString)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(dateTimeString, out dateTime))
            {
                if (!DateTime.TryParseExact(dateTimeString, formatString, null, DateTimeStyles.None, out dateTime))
                    return null;
            }

            return dateTime;
        }

    }
}
