using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TwitterLib
{
    /// <summary>
    /// Various global utility methods for Twitter library
    /// </summary>
    public class TwitterUtility
    {
        /// <summary>
        /// Convert a string to a valid Uri object (or null)
        /// </summary>
        /// <param name="sUri">String represent Uri, e.g. http://blahblah.com</param>
        /// <returns></returns>
        public static Uri UriFromString(string sUri)
        {
            return ((sUri != null) && (sUri.Length > 0)) ? new Uri(sUri) : null;
        }

        /// <summary>
        /// Pull a couple fields out of the header--specifically, the Twitter API rate limit info.
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="rateLimit"></param>
        /// <param name="limitRemaining"></param>
        public static void GetInfoFromResponse(WebResponse resp, out int rateLimit, out int limitRemaining)
        {
            rateLimit = 0;
            limitRemaining = 0;

            for (int i = 0; i < resp.Headers.Keys.Count; i++)
            {
                string s = resp.Headers.GetKey(i);
                if (s == "X-RateLimit-Limit")
                {
                    rateLimit = int.Parse(resp.Headers.GetValues(i).First());
                }
                if (s == "X-RateLimit-Remaining")
                {
                    limitRemaining = int.Parse(resp.Headers.GetValues(i).First());
                }
            }
        }

        /// <summary>
        /// Parse twitter date string into .NET DateTime
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static DateTime ParseTwitterDate(string dateString)
        {
            return DateTime.ParseExact(dateString, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return a valid DateTime for status.created_at and handle the case
        /// of the status element not being present.
        /// </summary>
        /// <param name="user">Represents status element (child of user element)</param>
        /// <returns></returns>
        public static DateTime SafeUpdateDateTime(XElement user)
        {
            DateTime creAt = new DateTime();        // Default constructor is 1/1/0001 12AM

            if (user != null)
            {
                XElement elemCreAt = user.Element("created_at");
                if (elemCreAt != null)
                {
                    creAt = ParseTwitterDate((string)elemCreAt);
                }
            }

            return creAt;
        }

        /// <summary>
        /// Return a valid update text string, whether or not the <status> element
        /// was present.
        /// </summary>
        /// <param name="user">Represents status element (child of user element)</param>
        /// <returns></returns>
        public static string SafeUpdateText(XElement user)
        {
            string sText = "";

            if (user != null)
            {
                XElement elemText = user.Element("text");
                if (elemText != null)
                {
                    sText = (string)elemText;
                }
            }

            return sText;
        }
    }
}
