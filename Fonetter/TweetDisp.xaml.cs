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
		private ReplyControl replyUi;

		public TweetDisp(TweetData tweetData, AccountData ad, List<Accounts> accountList, ReplyControl tbox) {
			InitializeComponent();
			data = tweetData;
			accountData = ad;
			accounts = accountList;
			replyUi = tbox;
			sndRun.Text = "@" + tweetData.ScreenName;
			undRun.Text = tweetData.UserName;
			TweetText.Text = tweetData.Text;
			viaRun.Text = "via " + tweetData.ViaName;
			viaHyperLink.NavigateUri = new Uri(tweetData.ViaUri);
			lbDate.Text = (tweetData.CreatedTime.Year % 100).ToString("00") + "/" + (tweetData.CreatedTime.Month).ToString("00") + "/" + (tweetData.CreatedTime.Day).ToString("00");
			lbTime.Text = (tweetData.CreatedTime.Hour).ToString("00") + ":" + (tweetData.CreatedTime.Minute).ToString("00") + ":" + (tweetData.CreatedTime.Second).ToString("00");
			imgIcon.AsyncLoad(tweetData.IconUri);
			ChangeRtStat(data.IsRetweeted);
			ChangeFavStat(data.IsFavorited);

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

		private void ChangeRtStat(bool stat) {
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

		private void ChangeFavStat(bool stat) {
			if(stat) {
				data.IsFavorited = true;
				btFavorite.Content = " Fved ";
				btFavorite.FontWeight = FontWeights.Bold;
			} else {
				data.IsFavorited = false;
				btFavorite.Content = " Fv ";
				btFavorite.FontWeight = FontWeights.Normal;
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
			if(chk == null) MessageBox.Show("不正なアカウント: RTに失敗しました。");
			else {
				if(!data.IsRetweeted) {
					if(MessageBox.Show("以下のツイートを@" + accountData.ScreenName + "よりRTしますか?\r\n" + data.Text, "Information", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
					var r = chk.RetweetStatus(data.TweetId);
					if(r == ExceptionCheck.Ok.ToString()) {
					} else if(r == ExceptionCheck.Already.ToString()) {
						MessageBox.Show("既にRTされています。", "Error");
					} else if(r == ExceptionCheck.Nothing.ToString()) {
						MessageBox.Show("このツイートは存在しません。", "Error");
						return;
					} else if(r == ExceptionCheck.Blocked.ToString()) {
						MessageBox.Show("ブロックされているため、RTできません。", "Error");
						return;
					} else {
						MessageBox.Show("不明なエラー。 (" + r + ")", "Error");
						return;
					}
					ChangeRtStat(true);
				} else {
					if(MessageBox.Show("以下の@" + accountData.ScreenName + "によるRTを解除しますか?\r\n" + data.Text, "Information", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
					var tid = (data is RetweetData) ? (data as RetweetData).RtingId : data.TweetId;
					string d = chk.DestroyRetweet(tid);
					if(d == ExceptionCheck.Ok.ToString()) {
						ChangeRtStat(false);
					} else if(d == ExceptionCheck.Nothing.ToString()) {
						MessageBox.Show("このツイートは存在しません。", "Error");
					} else {
						MessageBox.Show("不明なエラー。 (" + d + ")", "Error");
					}
				}
			}
		}

		private void btFavorite_Click(object sender, RoutedEventArgs e) {
			var chk = accounts.FirstOrDefault(x => x.Data.ScreenName == accountData.ScreenName);
			if(chk == null) MessageBox.Show("不正なアカウント: お気に入り登録に失敗しました。");
			else {
				if(!data.IsFavorited) {
					if(MessageBox.Show("以下のツイートを@" + accountData.ScreenName + "よりお気に入り登録しますか?\r\n" + data.Text, "Information", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
					var f = chk.FavoriteStatus(data.TweetId);
					if(f == ExceptionCheck.Ok.ToString()) {
					} else if(f == ExceptionCheck.Already.ToString()) {
						MessageBox.Show("既にお気に入り登録されています。", "Error");
					} else if(f == ExceptionCheck.ProtectedUser.ToString()) {
						MessageBox.Show("プロテクトユーザのため、お気に入り登録できません。", "Error");
						return;
					} else if(f == ExceptionCheck.Blocked.ToString()) {
						MessageBox.Show("ブロックされているため、お気に入り登録できません。", "Error");
						return;
					} else if(f == ExceptionCheck.Nothing.ToString()) {
						MessageBox.Show("削除されているため、お気に入り登録できません。", "Error");
						return;
					} else {
						MessageBox.Show("不明なエラー。 (" + f + ")", "Error");
						return;
					}
					ChangeFavStat(true);
				} else {
					var d = chk.DestroyFavorite(data.TweetId);
					if(d == ExceptionCheck.Ok.ToString()) {
						ChangeRtStat(false);
					} else if(d == ExceptionCheck.Nothing.ToString()) {
						MessageBox.Show("このツイートは存在しません。", "Error");
					} else {
						MessageBox.Show("不明なエラー。 (" + d + ")", "Error");
					}
					ChangeFavStat(false);
				}
			}
		}

		private void btReply_Click(object sender, RoutedEventArgs e) {
			var chk = accounts.FirstOrDefault(x => x.Data.ScreenName == accountData.ScreenName);
			if(chk == null) {
				MessageBox.Show("不明なエラー。", "Error");
				return;
			}
			replyUi.Icon.AsyncLoad(accountData.IconUri);
			replyUi.SelectedUser = accounts.IndexOf(chk);
			string mentionFor = "@" + data.ScreenName + " ";
			if(data.MentionFor != null) {
				foreach(var dm in data.MentionFor) {
					mentionFor += "@" + dm + " ";
				}
			}
			replyUi.Text.Text = mentionFor + replyUi.Text.Text;
			replyUi.MentionText.Text = "To: @" + data.ScreenName + " >> " + data.Text;
			replyUi.MentionButton.IsEnabled = true;
			replyUi.ReplyFor = data.TweetId;
		}
	}
}
