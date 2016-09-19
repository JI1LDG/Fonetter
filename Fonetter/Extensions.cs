using CoreTweet;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fonetter {
	static class Extensions {
		public static void SetUI(this Grid setFor, UIElement ui, int row, int column, int rowspan = 1, int colspan = 1) {
			Grid.SetRow(ui, row);
			Grid.SetColumn(ui, column);
			Grid.SetRowSpan(ui, rowspan);
			Grid.SetColumnSpan(ui, colspan);

			setFor.Children.Add(ui);
		}

		public static void AddCell(this Grid setFor, int row, int column) {
			int r = setFor.RowDefinitions.Count;
			if(r < row) {
				for(int i = r; i < row; i++) {
					setFor.RowDefinitions.Add(new RowDefinition());
				}
			} else if(r > row) {
				setFor.RowDefinitions.RemoveRange(row - 1, r - row);
			}
			int c = setFor.ColumnDefinitions.Count;
			if(c < column) {
				for(int i = c; i < column; i++) {
					setFor.ColumnDefinitions.Add(new ColumnDefinition());
				}
			} else if(c > column) {
				setFor.ColumnDefinitions.RemoveRange(column, c - column);
			}
		}

		public static void AsyncLoad(this Image imgFor, string imgUri, bool highQuality = false) {
			if(highQuality) imgUri = imgUri.Replace("_normal", "");
			var iconSrc = new BitmapImage(new Uri(imgUri));
			imgFor.Source = iconSrc;
			iconSrc.DownloadCompleted += new EventHandler((object sender, EventArgs e) => {
				RenderOptions.SetBitmapScalingMode(imgFor, BitmapScalingMode.Fant);
			});
		}

		public static MatchCollection GetDataFromLink(this string html) {
			string href = @"<a\s+[^>]*href\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|" + @"(?<url>[^\s>]+))[^>]*>(?<text>.*?)</a>";
			return Regex.Matches(html, href, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		public static TweetData Convert(this Status status) {
			if(status.RetweetedStatus == null) return new Fonetter.TweetData(status);
			else return new RetweetData(status);
		}

		public static int LetterLen(this string str) {
			return str.Length - str.Count(x => x == '\r');
		}
	}
}
