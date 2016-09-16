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

		public TweetDisp(TweetData tweetData) {
			InitializeComponent();
			data = tweetData;
			wpStatus.Height = 0;
			sndRun.Text = "@" + tweetData.ScreenName;
			undRun.Text = tweetData.UserName;
			TweetText.Text = tweetData.Text;
			viaRun.Text = "via " + tweetData.ViaName;
			viaHyperLink.NavigateUri = new Uri(tweetData.ViaUri);
			lbDate.Text = (tweetData.CreatedTime.Year % 100).ToString("00") + "/" + (tweetData.CreatedTime.Month).ToString("00") + "/" + (tweetData.CreatedTime.Day).ToString("00");
			lbTime.Text = (tweetData.CreatedTime.Hour).ToString("00") + ":" + (tweetData.CreatedTime.Minute).ToString("00") + ":" + (tweetData.CreatedTime.Second).ToString("00");
			AsyncLoad(imgIcon, tweetData.IconUri);
		}

		public TweetDisp() {
			InitializeComponent();
		}

		private void UserHL_Click(object sender, RoutedEventArgs e) {

		}

		private void UriHL_Click(object sender, RoutedEventArgs e) {
			var hl = (Hyperlink)sender;
			System.Diagnostics.Process.Start(hl.NavigateUri.AbsoluteUri);
		}

		private void AsyncLoad(Image imgFor, string imgUri) {
			var iconSrc = new BitmapImage(new Uri(imgUri));
			imgFor.Source = iconSrc;
			iconSrc.DownloadCompleted += new EventHandler((object sender, EventArgs e) => {
				imgFor.Width = iconSrc.PixelWidth;
				imgFor.Height = iconSrc.PixelHeight;
			});
		}
	}
}
