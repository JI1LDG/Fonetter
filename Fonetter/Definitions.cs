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
		public int ContentsNum { get { return Timelines.Count - startIdx; } }
		public GridMode Mode { get; set; }
		public List<TimeLine> Timelines { get; private set; }
		public int startIdx { get; set; }
		public int cacheRow { get; set; }
		public int cacheCol { get; set; }

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
			
			var mc = status.Source.GetDataFromLink();
			ViaUri = mc[0].Groups["url"].Value;
			ViaName = mc[0].Groups["text"].Value;
		}
	}

	public class RetweetData : TweetData {
		public long RtingId { get; set; }
		public string RtingScreenName { get; set; }
		public string RtingUserName { get; set; }
		public DateTime RtingCreatedTime { get; set; }

		public RetweetData(Status status) : base(status.RetweetedStatus) {
			var id = TweetId;
			TweetId = status.Id;
			RtingId = id;
			RtingScreenName = status.User.ScreenName;
			RtingUserName = status.User.Name;
			RtingCreatedTime = status.CreatedAt.LocalDateTime;
		}
	}

	public enum StreamStatus {
		Wait, Streaming, Stop,
	}

	public class AccountData {
		private UserResponse user;
		public string IconUri { get { return user.ProfileImageUrl; } }
		public string ScreenName { get { return user.ScreenName; } }
		public long UserId { get { return (long)user.Id; } }

		public AccountData(UserResponse userResponse) {
			this.user = userResponse;
		}
	}
}
