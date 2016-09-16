using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private Accounts ac;

		public MainWindow() {
			InitializeComponent();
			tlGrid = new Fonetter.TimeLineGrid() { MinHeight = 150, MinWidth = 300, MaxColumns = 4, MaxRows = 3, Mode = GridMode.Left, };
			ac = new Accounts();
			tlGrid.AddTimeLine(new Fonetter.TimeLine(ac.Tweets));

			grd_Adjust(grd.RenderSize);
			PutUis();
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

			if(tlGrid.ContentsNum <= cols && tlGrid.ContentsNum <= tlGrid.MaxColumns) {
				cols = tlGrid.ContentsNum;
				rows = 1;
			} else if(tlGrid.ContentsNum <= tlGrid.MaxRows * tlGrid.MaxColumns) {
				for(int i = 2; i <= rows; i++) {
					var mc = Math.Ceiling((double)tlGrid.ContentsNum / (double)i);
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

			grd.AddCell((int)rows, (int)cols);
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
					var tl = tlGrid.Timelines[r * w + c];
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
			grd_Adjust(e.NewSize);
			PutUis();
		}

		private void miTweet_Click(object sender, RoutedEventArgs e) {
			if(gUpdate.Height == 0) {
				gUpdate.Height = double.NaN;
				miTweet.Header = "TweetClose";
			} else {
				gUpdate.Height = 0;
				miTweet.Header = "TweetOpen";
			}
		}

		private void miReflesh_Click(object sender, RoutedEventArgs e) {
			ac.StopStreaming();
			Console.WriteLine("Wait!");
			System.Threading.Thread.Sleep(10 * 1000);
			Console.WriteLine("Restart!");
			ac.StartStreaming();
		}
	}
}
