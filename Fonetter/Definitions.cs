using CoreTweet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Fonetter {
	public class Consumers {
		public string Name { get; private set; }
		public string Key { get; private set; }
		public string Secret { get; private set; }

		public Consumers(string key, string secret, string name = "") {
			this.Key = key;
			this.Secret = secret;
			this.Name = name;
		}
	}

	public class Users {
		public long Id { get; private set; }
		public string ScreenName { get; private set; }
		public List<Keys> Key { get; set; }

		public Users(long id, string name, string consumer, string token, string secret) {
			this.Id = id;
			this.ScreenName = name;
			this.Key = new List<Keys>() { new Keys(consumer, token, secret) };
		}

		public void UpdateName(string name) {
			this.ScreenName = name;
		}
	}

	public class Keys {
		public string ConsumerKey { get; private set; }
		public string AccessToken { get; private set; }
		public string AccessSecret { get; private set; }

		public Keys(string consumer, string token, string secret) {
			this.ConsumerKey = consumer;
			this.AccessToken = token;
			this.AccessSecret = secret;
		}
	}

	public class Selections {
		public long Id { get; private set; }
		public string ScreenName { get; set; }
		public string ConsumerKey { get; set; }
		public string Name { get; set; }

		public Selections(long id, string key) {
			this.Id = id;
			this.ConsumerKey = key;
		}
	}

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
		public string[] MentionFor { get; set; }

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

			if(status.Entities.UserMentions != null && status.Entities.UserMentions.Length > 0) {
				var mf = new List<string>();
				foreach(var um in status.Entities.UserMentions) {
					mf.Add(um.ScreenName);
				}
				MentionFor = mf.ToArray();
			}
		}
	}

	public class RetweetData : TweetData {
		public long RtingId { get; set; } //RT元はTweetData::TweetId
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

	public enum ExceptionCheck {
		Ok, 
		Already, Blocked, ProtectedUser, Nothing, Duplicate,
	}

	public class ReplyControl {
		public TextBox Text;
		public Image Icon;
		public int SelectedUser;
		public TextBlock MentionText;
		public Button MentionButton;
		public long ReplyFor;

		public ReplyControl(TextBox tbox, Image ibox, TextBlock mbox, Button bbox) {
			Text = tbox;
			Icon = ibox;
			SelectedUser = 0;
			MentionText = mbox;
			MentionButton = bbox;
		}
	}
}
