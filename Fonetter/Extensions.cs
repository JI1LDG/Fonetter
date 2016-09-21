using CoreTweet;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fonetter {
	static partial class Extensions {
		public static MatchCollection GetDataFromLink(this string html) {
			string href = @"<a\s+[^>]*href\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|" + @"(?<url>[^\s>]+))[^>]*>(?<text>.*?)</a>";
			return Regex.Matches(html, href, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		public static TweetData Convert(this Status status) {
			if(status.RetweetedStatus == null) return new Fonetter.TweetData(status);
			else return new RetweetData(status);
		}

		public static int LetterLen(this string str) {
			return str.Length - str.Count(x => x == '\r');
		}
	}
}
