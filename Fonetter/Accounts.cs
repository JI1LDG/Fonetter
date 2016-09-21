using CoreTweet;
using CoreTweet.Streaming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Fonetter {
	public class Accounts {
		public List<Accounts> AccountList;
		public ObservableCollection<TweetDisp> Tweets { get; private set; }
		public StreamStatus StreamState { get; private set; }
		public AccountData Data { get; private set; }
		private Tokens token;
		private IConnectableObservable<StreamingMessage> stream;
		private IDisposable disposable;
		private ReplyControl updateBox;

		public Accounts(Tokens tokens, List<Accounts> accounts, ReplyControl tbox, ObservableCollection<TweetDisp> tmpTweets = null) {
			AccountList = accounts;
			if(tmpTweets == null) Tweets = new ObservableCollection<TweetDisp>();
			else Tweets = tmpTweets;
			StreamState = StreamStatus.Stop;
			token = tokens;
			updateBox = tbox;

			UserResponse ur;
			try {
				ur = token.Account.VerifyCredentials();
			} catch(Exception e) {
				if(e is SocketException || e is WebException) {
					Console.WriteLine("Failed to Connect (" + e.Message + ")");
				} else {
					MessageBox.Show("Unknown Error occured. (" + e.Message + ")");
				}
				return;
			}
			token.ScreenName = ur.ScreenName;
			token.UserId = (long)ur.Id;
			Data = new AccountData(ur);

			StartStreaming();
		}

		~Accounts() {
			StopStreaming();
		}

		private async void GetLastTweets() {
			if(Tweets.Count == 0) {
				foreach(var status in await token.Statuses.HomeTimelineAsync(count => 50)) {
					Tweets.Add(new TweetDisp(status.Convert(), Data, AccountList, updateBox));
				}
			} else {
				int i = 0;
				foreach(var status in await token.Statuses.HomeTimelineAsync(since_id => Tweets.Max(x => x.data.TweetId).ToString())) {
					if(Tweets.Any(x => x.data.TweetId == status.Id)) break;
					Tweets.Insert(i++, new Fonetter.TweetDisp(status.Convert(), Data, AccountList, updateBox));
				}
			}
		}

		public void StartStreaming() {
			stream = token.Streaming.UserAsObservable().Publish();
			Console.WriteLine("Start");

			Action<Status> StatusOnNextAction = (message) => {
				StreamState = StreamStatus.Streaming;
				Tweets.Insert(0, new TweetDisp(message.Convert(), Data, AccountList, updateBox));
				Console.WriteLine("Create: " + message.Text);
			};
			Action<DeleteMessage> DeleteOnNextAction = (message) => {
				var del = Tweets.Where(x => x.data.TweetId == message.Id || ((x.data is RetweetData) && (x.data as RetweetData).RtingId == message.Id)).ToArray();
				if(del.Count() > 0) {
					foreach(var d in del) {
						Tweets.Remove(d);
					}
				}
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

		public string UpdateStatus(string Text) {
			try {
				token.Statuses.Update(status => Text);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}

		public string UpdateStatus(string Text, long ReplyTo) {
			try {
				token.Statuses.Update(status => Text, in_reply_to_status_id => ReplyTo);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}

		public string DestroyStatus(long TweetId) {
			try {
				token.Statuses.Destroy(id => TweetId);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}

		public string RetweetStatus(long TweetId) {
			try {
				token.Statuses.Retweet(id => TweetId);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}

		public string DestroyRetweet(long TweetId) {
			long idUR;
			try {
				var sr = token.Statuses.Show(id => TweetId, include_my_retweet => "true");
				var i = sr.CurrentUserRetweet;
				idUR = (long)i;
			} catch(Exception e) {
				return e.DetectException();
			}
			return DestroyStatus(idUR);
		}

		public string FavoriteStatus(long TweetId) {
			try {
				token.Favorites.Create(id => TweetId);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}

		public string DestroyFavorite(long TweetId) {
			try {
				token.Favorites.Destroy(id => TweetId);
				return ExceptionCheck.Ok.ToString();
			} catch(Exception e) {
				return e.DetectException();
			}
		}
	}
}
