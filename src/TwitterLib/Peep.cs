using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TwitterLib
{
    public class Peep
    {
        private const string userInfoURI = "http://twitter.com/users/show.xml?screen_name={0}";

        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public Uri ProfileImageURL { get; set; }
        public Uri URL { get; set; }
        public int ID { get; set; }

        public int NumFollowers { get; set; }
        public int NumFollowing { get; set; }
        public int NumUpdates { get; set; }

        public string LastUpdateText { get; set; }
        public DateTime LastUpdateDateTime { get; set; }

        public new string ToString()
        {
            return string.Format("{0} - {1}", ScreenName, Name);
        }

        /// <summary>
        /// Calculate NumFollowing & NumUpdates, since these two fields'
        /// data isn't available from an API call that gets a list of
        /// multiple users, but must be retrieve for each user individually.
        /// </summary>
        public void CalcAddlInfo()
        {
            if ((ScreenName == null) || (ScreenName == ""))
                throw new ArgumentException("CalcNumFollowing: Invalid ScreenName");

            int rateLimit = 0;          // Max # API calls per hour
            int limitRemaining = 0;     // # API calls remaining

            XDocument docUser;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(string.Format(userInfoURI, ScreenName));
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                TwitterUtility.GetInfoFromResponse(resp, out rateLimit, out limitRemaining);
                XmlReader reader = XmlReader.Create(resp.GetResponseStream());
                docUser = XDocument.Load(reader);

                XElement user = docUser.Element("user");

                NumFollowing = (int)user.Element("friends_count");
                NumUpdates = (int)user.Element("statuses_count");
            }
            catch (WebException xcp)
            {
                throw new ApplicationException(
                    string.Format("Twitter rate limit exceeded, max of {0}/hr allowed. Remaining = {1}",
                        rateLimit,
                        limitRemaining),
                    xcp);
            }

        }

        /// <summary>
        /// Variant that just takes screen name, rather than acting on existing
        /// instance.
        /// </summary>
        /// <param name="ScreenName"></param>
        public void CalcAddlInfo(string screenName)
        {
            Peep p = new Peep { ScreenName = screenName };
            p.CalcAddlInfo();
        }
    }
}
