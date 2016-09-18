using CoreTweet;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
		private ObservableCollection<TweetDisp> tw { get; set; }
		private double currVal, max = -1;
		private int currCnt = -1;
		private ScrollBar scrollBar;

		public TimeLine(ObservableCollection<TweetDisp> home) {
			InitializeComponent();
			lbTl.ItemsSource = home;
			scrollBar = GetScrollBar();
			scrollBar.LayoutUpdated += ScrollBar_LayoutUpdated;
			scrollBar.SizeChanged += ScrollBar_SizeChanged;
		}

		private void ScrollBar_SizeChanged(object sender, SizeChangedEventArgs e) {
			max = scrollBar.Maximum;
			currVal = scrollBar.Value;
		}

		private void ScrollBar_LayoutUpdated(object sender, EventArgs e) {
			if(currCnt != lbTl.Items.Count) {
				currCnt = lbTl.Items.Count;
				if(scrollBar != null) {
					if(max == -1) { // At First
						max = scrollBar.Maximum;
						currVal = scrollBar.Value;
					}

					if(max != scrollBar.Maximum) {
						var tagObj = GetScrollViewer();
						var cacheMax = max;
						max = scrollBar.Maximum;
						if(cacheMax < scrollBar.Maximum) {
							tagObj.ScrollToVerticalOffset(currVal + scrollBar.Maximum - cacheMax);
						} else {
							tagObj.ScrollToVerticalOffset(currVal + cacheMax - scrollBar.Maximum);
						}
					} else {
						currVal = scrollBar.Value;
					}
				}
			} else {
				if(scrollBar != null) {
					currVal = scrollBar.Value;
				}
			}
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
