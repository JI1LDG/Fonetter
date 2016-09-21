using CoreTweet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		private TimeLineGrid tlGrid;
		private List<Accounts> ac;
		private ReplyControl replyUi;
		private int accountNowSelected {
			get {
				return replyUi.SelectedUser;
			}
			set {
				replyUi.SelectedUser = value;
			}
		}
		private Accounts nowAccount { get { return ac[accountNowSelected]; } }
		private int startIdx;
		

		public MainWindow() {
			Sql.CheckDb();
			var au = new Authorization();
			if(au.AuthedToken == null || au.AuthedToken.Length == 0) au.ShowDialog();

			InitializeComponent();
			tlGrid = new Fonetter.TimeLineGrid() { MinHeight = 150, MinWidth = 300, MaxColumns = 4, MaxRows = 3, Mode = GridMode.Left, startIdx = 0, };
			ac = new List<Accounts>();
			replyUi = new ReplyControl(tbTweet, imgAccount, tbReplyText, btnClearReply);
			foreach(var aat in au.AuthedToken) {
				var a = new Accounts(aat, ac, replyUi);
				if(a.Data == null) continue;
				ac.Add(a);
				tlGrid.AddTimeLine(new TimeLine(a.Tweets) { TlName = "TL: @" + a.Data.ScreenName, });
			}

			if(ac.Count > 0) {
				imgAccount.AsyncLoad(ac[0].Data.IconUri);
			}

			grd_Adjust(grd.RenderSize);
		}

		~MainWindow() {
			if(ac != null) {

			}
		}

		private void grd_Adjust(Size gdSize) {
			double rows = gdSize.Height / tlGrid.MinHeight;
			if(rows < 2.0f) rows = 1.0f;
			else rows = Math.Floor(rows);

			double cols = gdSize.Width / tlGrid.MinWidth;
			if(cols < 2.0f) cols = 1.0f;
			else cols = Math.Floor(cols);

			int cn = tlGrid.Timelines.Count - startIdx;

			if(cn <= cols && cn <= tlGrid.MaxColumns) {
				cols = cn;
				rows = 1;
			} else if(cn <= tlGrid.MaxRows * tlGrid.MaxColumns) {
				for(int i = 2; i <= rows; i++) {
					var mc = Math.Ceiling((double)cn / (double)i);
					if(mc <= cols && mc <= tlGrid.MaxColumns) {
						cols = mc;
						rows = i;
						break;
					}
				}
				if(rows > tlGrid.MaxRows) rows = tlGrid.MaxRows;
				if(cols > tlGrid.MaxColumns) cols = tlGrid.MaxColumns;
			} else {
				if(rows > tlGrid.MaxRows) rows = tlGrid.MaxRows;
				if(cols > tlGrid.MaxColumns) cols = tlGrid.MaxColumns;
			}

			if(tlGrid.cacheRow != (int)rows || tlGrid.cacheCol != (int)cols || startIdx != tlGrid.startIdx) {
				tlGrid.startIdx = startIdx;
				grd.AddCell((int)rows, (int)cols);
				tlGrid.cacheRow = (int)rows;
				tlGrid.cacheCol = (int)cols;
				PutUis();
			}
		}

		private void PutUis() {
			grd.Children.Clear();
			int h = grd.RowDefinitions.Count;
			int w = grd.ColumnDefinitions.Count;

			int ex_c = h * w - tlGrid.ContentsNum;
			int ex_h = h - 2;
			int rspan = 1, cspan = 1;
			int alph_c = 0;

			for(int r = 0; r < h; r++) {
				for(int c = 0; c < w && tlGrid.ContentsNum > r * w + c; c++) {
					var tl = tlGrid.Timelines[r * w + c + tlGrid.startIdx];
					if(tlGrid.ContentsNum >= h * w) {
					} else {
						rspan = 1; alph_c = 0;
						if(tlGrid.Mode.HasFlag(GridMode.Right) && r == ex_h && w - c <= ex_c) {
							rspan = 2;
						} else if(tlGrid.Mode.HasFlag(GridMode.Left)) {
							if(r == ex_h && c < ex_c) rspan = 2;
							else if(r == ex_h + 1) alph_c = ex_c;
						}
					}
					grd.SetUI(tl, r, c + alph_c, rspan, cspan);
				}
			}
		}

		private void grd_SizeChanged(object sender, SizeChangedEventArgs e) {
			if(tlGrid.Timelines.Count == 0) return;
			startIdx %= tlGrid.Timelines.Count;
			tlGrid.startIdx = startIdx;
			grd_Adjust(e.NewSize);
		}

		private void miTweet_Click(object sender, RoutedEventArgs e) {
			if(gUpdate.Height == 0) {
				gUpdate.Height = double.NaN;
			} else {
				gUpdate.Height = 0;
			}
		}

		private void miReflesh_Click(object sender, RoutedEventArgs e) {
			foreach(var a in ac) {
				a.StopStreaming();
				Console.WriteLine("Wait!");
				System.Threading.Thread.Sleep(10 * 1000);
				Console.WriteLine("Restart!");
				a.StartStreaming();
			}
		}

		private void btnAccount_Click(object sender, RoutedEventArgs e) {
			if(ac.Count <= 0) return;
			accountNowSelected++;
			if(accountNowSelected / ac.Count >= 1.0f) accountNowSelected %= ac.Count;
			imgAccount.AsyncLoad(ac[accountNowSelected].Data.IconUri);
		}

		private void btnUpdate_Click(object sender, RoutedEventArgs e) {
			btnUpdate.IsEnabled = false;
			string error;
			if(replyUi.ReplyFor == 0) {
				error = nowAccount.UpdateStatus(tbTweet.Text);
			} else {
				error = nowAccount.UpdateStatus(tbTweet.Text, replyUi.ReplyFor);
			}
			if(error == ExceptionCheck.Ok.ToString()) {
			} else if(error == ExceptionCheck.Duplicate.ToString()) {
				MessageBox.Show("ツイートが重複しています。", "Error");
			} else {
				MessageBox.Show("ツイートに失敗しました。 (" + error + ")", "Error");
			}
			btnUpdate.IsEnabled = true;

			tbTweet.Text = "";
			tbReplyText.Text = "";
			replyUi.ReplyFor = 0;
		}

		private void tbTweet_TextChanged(object sender, TextChangedEventArgs e) {
			var lastLen = 140 - tbTweet.Text.LetterLen();
			lbLetter.Content = lastLen;
			if((lastLen < 0 || lastLen == 140) || tlGrid.Timelines.Count == 0) btnUpdate.IsEnabled = false;
			else btnUpdate.IsEnabled = true;
		}

		private void miSelection_Click(object sender, RoutedEventArgs e) {
			if(tlGrid.Timelines.Count == 0) return;
			startIdx++;
			startIdx %= tlGrid.Timelines.Count;
			grd_Adjust(grd.RenderSize);
		}

		private void btnClearReply_Click(object sender, RoutedEventArgs e) {
			btnClearReply.IsEnabled = false;
			tbReplyText.Text = "";
			replyUi.ReplyFor = 0;
		}

		private void miAuth_Click(object sender, RoutedEventArgs e) {
			var au = new Authorization();
			var newac = new List<Accounts>();
			au.ShowDialog();
			if(au.AuthedToken == null || au.AuthedToken.Count() == 0) Environment.Exit(0);
			foreach(var a in ac) {
				var token = au.AuthedToken.FirstOrDefault(x => x.UserId == a.Data.UserId);
				a.StopStreaming();
				if(token == null) {
				} else {
					var na = new Accounts(token, newac, replyUi, a.Tweets);
					newac.Add(na);
					tlGrid.AddTimeLine(new TimeLine(na.Tweets) { TlName = "TL: @" + na.Data.ScreenName, });
				}
			}
			ac = newac;

			if(ac.Count > 0) {
				imgAccount.AsyncLoad(ac[0].Data.IconUri);
			}

			grd_Adjust(grd.RenderSize);
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			ac = null;
		}

		private void Window_Closed(object sender, EventArgs e) {
			Environment.Exit(0);
		}
	}
}
