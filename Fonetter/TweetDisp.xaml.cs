using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fonetter {
	/// <summary>
	/// TimeLine.xaml の相互作用ロジック
	/// </summary>
	public partial class TweetDisp : UserControl {
		public TweetData data { get; private set; }
		private List<Accounts> accounts;
		private AccountData accountData;

		public TweetDisp(TweetData tweetData, AccountData ad, List<Accounts> accountList) {
			InitializeComponent();
			data = tweetData;
			accountData = ad;
			accounts = accountList;
			sndRun.Text = "@" + tweetData.ScreenName;
			undRun.Text = tweetData.UserName;
			TweetText.Text = tweetData.Text;
			viaRun.Text = "via " + tweetData.ViaName;
			viaHyperLink.NavigateUri = new Uri(tweetData.ViaUri);
			lbDate.Text = (tweetData.CreatedTime.Year % 100).ToString("00") + "/" + (tweetData.CreatedTime.Month).ToString("00") + "/" + (tweetData.CreatedTime.Day).ToString("00");
			lbTime.Text = (tweetData.CreatedTime.Hour).ToString("00") + ":" + (tweetData.CreatedTime.Minute).ToString("00") + ":" + (tweetData.CreatedTime.Second).ToString("00");
			imgIcon.AsyncLoad(tweetData.IconUri);
			ChangeRtStat(data.IsRetweeted);

			if(tweetData is RetweetData) {
				var rd = tweetData as RetweetData;
				unsRun.Text = "@" + rd.RtingScreenName;
				dateRun.Text = (rd.RtingCreatedTime.Year % 100).ToString("00") + "/" + (rd.RtingCreatedTime.Month).ToString("00") + "/" + (rd.RtingCreatedTime.Day).ToString("00");
				timeRun.Text = (rd.RtingCreatedTime.Hour).ToString("00") + ":" + (rd.RtingCreatedTime.Minute).ToString("00") + ":" + (rd.RtingCreatedTime.Second).ToString("00");
				if(rd.RtingScreenName == accountData.ScreenName) {
					ChangeRtStat(true);
				}
			} else wpStatus.Height = 0;
		}

		public void ChangeRtStat(bool stat) {
			if(stat) {
				data.IsRetweeted = true;
				btRetweet.Content = " Rted ";
				btRetweet.FontWeight = FontWeights.Bold;
			} else {
				data.IsRetweeted = false;
				btRetweet.Content = " Rt ";
				btRetweet.FontWeight = FontWeights.Normal;
			}
		}

		private void UserHL_Click(object sender, RoutedEventArgs e) {

		}

		private void UriHL_Click(object sender, RoutedEventArgs e) {
			var hl = (Hyperlink)sender;
			System.Diagnostics.Process.Start(hl.NavigateUri.AbsoluteUri);
		}

		private void btRetweet_Click(object sender, RoutedEventArgs e) {
			var chk = accounts.FirstOrDefault(x => x.Data.ScreenName == accountData.ScreenName);
			if(chk == null) MessageBox.Show("RTに失敗しました。");
			else {
				if(!data.IsRetweeted) {
					if(MessageBox.Show("以下のツイートを@" + accountData.ScreenName + "よりRTしますか?\r\n" + data.Text, "Information", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
					var r = chk.Retweeting(data.TweetId);
					if(r == "ok") {
					} else if(r == "already") {
						MessageBox.Show("既にRTされています。", "Error");
					} else {
						MessageBox.Show("RTに失敗しました。 (" + r + ")", "Error");
						return;
					}
					ChangeRtStat(true);
				} else {
					if(MessageBox.Show("以下の@" + accountData.ScreenName + "によるRTを解除しますか?\r\n" + data.Text, "Information", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
					var tid = (data is RetweetData) ? (data as RetweetData).RtingId : data.TweetId;
					string d = chk.DestroyWithRetweet(tid);
					if(d == "ok") {
						ChangeRtStat(false);
					} else {
						MessageBox.Show("RTされていません。 (" + d + ")", "Error");
					}
				}
			}
		}
	}
}
