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
		public ObservableCollection<TweetDisp> Tweets { get; private set; }
		public StreamStatus streamStatus { get; private set; }
		private Tokens token;
		private IConnectableObservable<StreamingMessage> stream;
		private IDisposable disposable;

		public Accounts() {
			Tweets = new ObservableCollection<TweetDisp>();
			streamStatus = StreamStatus.Stop;

			using(var sr = new System.IO.StreamReader("Keys.txt", System.Text.Encoding.UTF8)) {
				var keys = new string[4];
				for(int i = 0;i < 4; i++) {
					keys[i] = sr.ReadLine();
				}
				token = Tokens.Create(keys[0], keys[1], keys[2], keys[3]);
			}

			StartStreaming();
		}

		private async void GetLastTweets() {
			if(Tweets.Count == 0) {
				foreach(var status in await token.Statuses.HomeTimelineAsync(count => 10)) {
					Tweets.Add(new TweetDisp(new TweetData(status)));
				}
			} else {
				int i = 0;
				foreach(var status in await token.Statuses.HomeTimelineAsync(since_id => Tweets.Max(x => x.data.TweetId).ToString())) {
					if(Tweets.Any(x => x.data.TweetId == status.Id)) break;
					Tweets.Insert(i++, new Fonetter.TweetDisp(new Fonetter.TweetData(status)));
				}
			}
		}

		public void StartStreaming() {
			stream = token.Streaming.UserAsObservable().Publish();
			Console.WriteLine("Start");

			Action<Status> OnNextAction = (message) => {
				streamStatus = StreamStatus.Streaming;
				Tweets.Insert(0, new TweetDisp(new TweetData(message)));
				Console.WriteLine("Create: " + message.Text);
			};
			Action<Exception> OnExceptionAction = (message) => { streamStatus = StreamStatus.Wait; };

			stream.Catch(stream.DelaySubscription(System.TimeSpan.FromSeconds(10)).Retry()).Repeat()
				.OfType<StatusMessage>().Select(x => x.Status).ObserveOn(SynchronizationContext.Current).Subscribe(onNext: x => OnNextAction(x), onError: err => OnExceptionAction(err));
			GetLastTweets();
			disposable = stream.Connect();
		}

		public void StopStreaming() {
			streamStatus = StreamStatus.Stop;
			disposable.Dispose();
			Console.WriteLine("Stop");
		}
	}
}
