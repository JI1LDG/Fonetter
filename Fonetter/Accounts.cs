using CoreTweet;
using CoreTweet.Streaming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Fonetter {
	public class Accounts {
		public List<Accounts> AccountList;
		public ObservableCollection<TweetDisp> Tweets { get; private set; }
		public StreamStatus StreamState { get; private set; }
		public AccountData Data { get; private set; }
		private Tokens token;
		private IConnectableObservable<StreamingMessage> stream;
		private IDisposable disposable;

		public Accounts(Tokens tokens, List<Accounts> accounts) {
			AccountList = accounts;
			Tweets = new ObservableCollection<TweetDisp>();
			StreamState = StreamStatus.Stop;
			token = tokens;
				
			var ur = token.Account.VerifyCredentials();
			token.ScreenName = ur.ScreenName;
			token.UserId = (long)ur.Id;
			Data = new AccountData(ur);

			StartStreaming();
		}

		private async void GetLastTweets() {
			if(Tweets.Count == 0) {
				foreach(var status in await token.Statuses.HomeTimelineAsync(count => 50)) {
					Tweets.Add(new TweetDisp(status.Convert(), Data, AccountList));
				}
			} else {
				int i = 0;
				foreach(var status in await token.Statuses.HomeTimelineAsync(since_id => Tweets.Max(x => x.data.TweetId).ToString())) {
					if(Tweets.Any(x => x.data.TweetId == status.Id)) break;
					Tweets.Insert(i++, new Fonetter.TweetDisp(status.Convert(), Data, AccountList));
				}
			}
		}

		public void StartStreaming() {
			stream = token.Streaming.UserAsObservable().Publish();
			Console.WriteLine("Start");

			Action<Status> StatusOnNextAction = (message) => {
				StreamState = StreamStatus.Streaming;
				Tweets.Insert(0, new TweetDisp(message.Convert(), Data, AccountList));
				Console.WriteLine("Create: " + message.Text);
			};
			Action<DeleteMessage> DeleteOnNextAction = (message) => {
				var del = Tweets.Where(x => x.data.TweetId == message.Id || ((x.data is RetweetData) && (x.data as RetweetData).RtingId == message.Id)).ToArray();
				if(del.Count() > 0) { foreach(var d in del) { Tweets.Remove(d); } }
				Console.WriteLine("Remove: " + message.Id);
			};
			Action<Exception> OnExceptionAction = (message) => {
				StreamState = StreamStatus.Wait;
				StopStreaming();
				StartStreaming();
			};

			stream.OfType<StatusMessage>().Select(x => x.Status).ObserveOn(SynchronizationContext.Current).Subscribe(onNext: x => StatusOnNextAction(x), onError: err => OnExceptionAction(err));
			stream.OfType<DeleteMessage>().ObserveOn(SynchronizationContext.Current).Subscribe(onNext: x => DeleteOnNextAction(x), onError: err => OnExceptionAction(err));
			GetLastTweets();
			disposable = stream.Connect();
		}

		public void StopStreaming() {
			StreamState = StreamStatus.Stop;
			disposable.Dispose();
			Console.WriteLine("Stop");
		}

		public async Task<StatusResponse> UpdateStatusAsync(string Text) {
			return await token.Statuses.UpdateAsync(status => Text);
		}

		public string DestroyStatus(long TweetId) {
			try {
				token.Statuses.Destroy(id => TweetId);
				return "ok";
			} catch(TwitterException e) {
				return e.Message;
			}
		}

		public string DestroyWithRetweet(long TweetId) {
			long idUR;
			try {
				var sr = token.Statuses.Show(id => TweetId, include_my_retweet => "true");
				var i = sr.CurrentUserRetweet;
				idUR = (long)i;
			} catch(TwitterException e) {
				return e.Message;
			} catch(Exception e2) {
				return e2.Message;
			}
			return DestroyStatus(idUR);
		}

		public string Retweeting(long TweetId) {
			try {
				token.Statuses.Retweet(id => TweetId);
				return "ok";
			} catch (CoreTweet.TwitterException e) {
				if(e.Message == "You have already retweeted this tweet.") {
					return "already";
				}
				return e.Message;
			}
		}
	}
}
