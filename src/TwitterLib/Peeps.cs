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
    /// Peep collection class
    /// </summary>
    public class Peeps : List<Peep>
    {
        // Partial Twitter API
        private const string getFriendsURI = "http://twitter.com/statuses/friends/{0}.xml?page={1}";

        /// <summary>
        /// Return list of Peeps followed by a specified person
        /// </summary>
        /// <param name="ScreenName">The Twitter username, e.g. spsexton</param>
        /// <returns></returns>
        public static Peeps PeopleFollowedBy(string ScreenName, out int RateLimit, out int LimitRemaining)
        {
            if ((ScreenName == null) || (ScreenName == ""))
                throw new ArgumentException("PeopleFollowedBy: Invalid ScreenName");

            int nPageNum = 1;
            int userCount = 0;      // # users read on last call

            int rateLimit = 0;          // Max # API calls per hour
            int limitRemaining = 0;     // # API calls remaining

            XDocument docFriends;
            Peeps peeps = new Peeps();

            // Retrieve people I'm following, 100 people at a time
            // (each call to Twitter API results in one "page" of results--up to 100 users)
            try
            {
                do
                {
                    // Example of constituting XDocument directly from the URI
                    // docFriends = XDocument.Load(string.Format(getFriendsURI, ScreenName, nPageNum));

                    // Manually create an HTTP request, so that we can pull information out of the
                    // headers in the response.  (Then later constitute the XDocument).
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(string.Format(getFriendsURI, ScreenName, nPageNum));
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    TwitterUtility.GetInfoFromResponse(resp, out rateLimit, out limitRemaining);
                    XmlReader reader = XmlReader.Create(resp.GetResponseStream());
                    docFriends = XDocument.Load(reader);

                    IEnumerable<XElement> users = docFriends.Elements("users").Elements("user");

                    userCount = users.Count();
                    if (userCount > 0)
                    {
                        List<Peep> nextPage = (from user in users
                                               orderby (string)user.Element("screen_name")
                                               select new Peep
                                               {
                                                   ID = (int)user.Element("id"),
                                                   ScreenName = (string)user.Element("screen_name"),
                                                   Name = (string)user.Element("name"),
                                                   Location = (string)user.Element("location"),
                                                   Description = (string)user.Element("description"),
                                                   ProfileImageURL = TwitterUtility.UriFromString((string)user.Element("profile_image_url")),
                                                   URL = TwitterUtility.UriFromString((string)user.Element("url")),
                                                   NumFollowers = (int)user.Element("followers_count"),
                                                   LastUpdateDateTime = TwitterUtility.SafeUpdateDateTime(user.Element("status")),
                                                   LastUpdateText = TwitterUtility.SafeUpdateText(user.Element("status"))
                                               }).ToList();

                        peeps.AddRange(nextPage);
                    }
                    nPageNum++;
                } while (userCount > 0);
            }
            catch (WebException xcp)
            {
                throw new ApplicationException(
                    string.Format("Twitter rate limit exceeded, max of {0}/hr allowed. Remaining = {1}",
                        rateLimit,
                        limitRemaining),
                    xcp);
            }
            finally
            {
                RateLimit = rateLimit;
                LimitRemaining = limitRemaining;
            }

            return peeps;
        }
    }
}
