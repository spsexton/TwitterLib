using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TwitterCount
{
    class FriendsRequest
    {
        private const string getFriendsURI = "http://twitter.com/statuses/friends/{0}.xml?page={1}";
        private const string userInfoURI = "http://twitter.com/users/show.xml?screen_name={0}";

        public static List<Peep> Test(string ScreenName)
        {
            if ((ScreenName == null) || (ScreenName == ""))
                throw new ArgumentException("Test: Invalid ScreenName");

            int nPageNum = 1;
            int userCount = 0;      // # users read on last call

            int rateLimit = 0;          // Max # API calls per hour
            int limitRemaining = 0;     // # API calls remaining

            XDocument docFriends;
            List<Peep> peeps = new List<Peep>();

            // Retrieve people I'm following, 100 people at a time 
            // (each call to Twitter API results in one "page" of results--up to 100 users)
            try
            {
                do
                {
                    // Example of constituting XDocument directly from the URI
                    // Note: Each invocation of XDocument.Load counts against the Twitter API, since it 
                    //   goes out to Twitter server and loads the page specified in the URI.
                    //                    docFriends = XDocument.Load(string.Format(getFriendsURI, ScreenName, nPageNum));

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

                // Now that we have a basic list of all people that we follow, we can 
                // do additional processing on each person, to get the information that
                // wasn't present in the getFriendsURI.   Specifically, for each person
                // that you are following, also get--# people they are following, and total 
                // # updates.
                //int nDebug = 0;
                //foreach (Peep p in peeps)
                //{
                //    XDocument docUserInfo = XDocument.Load(string.Format(userInfoURI, p.ScreenName));
                //    XElement user = docUserInfo.Element("user");

                //    p.NumFollowing = (int)user.Element("friends_count");
                //    p.NumUpdates = (int)user.Element("statuses_count");

                //    Debug.WriteLine(string.Format("Processed {0}", nDebug++));
                //}
            }
            catch (WebException xcp)
            {
                throw new ApplicationException(
                    string.Format("Twitter rate limit exceeded, max of {0}/hr allowed. Remaining = {1}", 
                        rateLimit, 
                        limitRemaining), 
                    xcp);
            }

            return peeps;
        }

    }
}
