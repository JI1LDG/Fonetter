using CoreTweet;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
	public partial class TimeLine : UserControl {
		private ObservableCollection<TweetDisp> tw;
		private double currVal, max = -1;
		int currCount;
		private ScrollBar scrollBar;
		public string TlName { get; set; }
		private Dictionary<TweetDisp, double> heights;
		private DateTime nowTime;
		private double removeHeight;

		public TimeLine(ObservableCollection<TweetDisp> home) {
			InitializeComponent();
			lbTl.ItemsSource = home;
			scrollBar = GetScrollBar();
			scrollBar.SizeChanged += ScrollBar_SizeChanged;
			scrollBar.LayoutUpdated += ScrollBar_LayoutUpdated;
			tw = home;
			tw.CollectionChanged += Tw_CollectionChanged;

			max = -1;
			heights = new Dictionary<Fonetter.TweetDisp, double>();
		}

		private void ScrollBar_LayoutUpdated(object sender, EventArgs e) {
			var sb = GetScrollBar();
			var sv = GetScrollViewer();
			if(currCount != lbTl.Items.Count) {
				currCount = lbTl.Items.Count;
				if(max == -1) {
					max = sb.Maximum;
					currVal = sb.Value;
				}

				if(sb.Value == 0.0f || max == sb.Maximum) {
					max = sb.Maximum;
				} else if(max < sb.Maximum) {
					var cacheMax = max;
					max = sb.Maximum;
					sv.ScrollToVerticalOffset(currVal + sb.Maximum - cacheMax);
				} else {
					max = sb.Maximum;
					sv.ScrollToVerticalOffset(currVal - removeHeight);
				}
			}
			currVal = sb.Value;
			if(tw != null && tw.Count > 0) {
				int nowCnt = 0;
				double chkVal = 0.0f;
				while(chkVal < currVal) {
					chkVal += tw[nowCnt++].ActualHeight;
				}
				if(tw.Count <= nowCnt) return;
				else nowTime = (tw[nowCnt].data is RetweetData) ? (tw[nowCnt].data as RetweetData).RtingCreatedTime : tw[nowCnt].data.CreatedTime;
			}
		}

		private void Tw_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			var sb = GetScrollBar();
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach(TweetDisp t in e.NewItems) {
						heights[t] = t.ActualHeight;
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					removeHeight = 0;
					foreach(TweetDisp t in e.OldItems) {
						DateTime thistime;
						if(t.data is RetweetData) thistime = (t.data as RetweetData).RtingCreatedTime;
						else thistime = t.data.CreatedTime;
						if(thistime < nowTime) continue;
						removeHeight += t.ActualHeight;
					}
					break;
			}
		}

		private void ScrollBar_SizeChanged(object sender, SizeChangedEventArgs e) {
			var sb = GetScrollBar();
			currVal = sb.Value;
			max = sb.Maximum;
		}

		private Visual GetDescendantByType(Visual element, Type type) {
			if(element == null) return null;
			if(element.GetType() == type) {
				return element;
			}

			Visual foundElement = null;
			if(element is FrameworkElement) (element as FrameworkElement).ApplyTemplate();
			for(int i = 0;i < VisualTreeHelper.GetChildrenCount(element); i++) {
				var vis = VisualTreeHelper.GetChild(element, i) as Visual;
				foundElement = GetDescendantByType(vis, type);
				if(foundElement != null) break;
			}

			return foundElement;
		}

		private ScrollViewer GetScrollViewer() {
			return GetDescendantByType(lbTl, typeof(ScrollViewer)) as ScrollViewer;
		}

		private ScrollBar GetScrollBar() {
			return GetDescendantByType(GetScrollViewer(), typeof(ScrollBar)) as ScrollBar;
		}
	}
}
