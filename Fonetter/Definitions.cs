using CoreTweet;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fonetter {
	public class TimeLineGrid {
		public double MinHeight { get; set; }
		public double MinWidth { get; set; }
		public int MaxRows { get; set; }
		public int MaxColumns { get; set; }
		public int ContentsNum { get { return Timelines.Count; } }
		public GridMode Mode { get; set; }
		public List<TimeLine> Timelines { get; private set; }

		public TimeLineGrid() {
			Timelines = new List<TimeLine>();
		}

		public void AddTimeLine(TimeLine tl) {
			Timelines.Add(tl);
		}
	}

	public enum GridMode {
		Left = 0x01, Right = 0x02,
	}

	public class TweetData {
		public long TweetId { get; set; }
		public string ScreenName { get; set; }
		public string UserName { get; set; }
		public string Text { get; set; }
		public string IconUri { get; set; }
		public DateTime CreatedTime { get; set; } //status.CreatedAt.LocalDateTime
		public bool IsRetweeted { get; set; }
		public bool IsFavorited { get; set; }
		public string ViaUri { get; set; }
		public string ViaName { get; set; }

		public TweetData(Status status) {
			TweetId = status.Id;
			ScreenName = status.User.ScreenName;
			UserName = status.User.Name;
			Text = status.Text;
			IconUri = status.User.ProfileImageUrl;
			CreatedTime = status.CreatedAt.LocalDateTime;
			IsRetweeted = status.IsRetweeted == true ? true : false;
			IsFavorited = status.IsFavorited == true ? true : false;
			
			string href = @"<a\s+[^>]*href\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|" + @"(?<url>[^\s>]+))[^>]*>(?<text>.*?)</a>";
			MatchCollection mc = Regex.Matches(status.Source, href, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			ViaUri = mc[0].Groups["url"].Value;
			ViaName = mc[0].Groups["text"].Value;
		}
	}

	public enum StreamStatus {
		Wait, Streaming, Stop,
	}
}
