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

		public TimeLine(ObservableCollection<TweetDisp> home) {
			InitializeComponent();
			lbTl.ItemsSource = home;
			var scrollViewer = GetDescendantByType(lbTl, typeof(ScrollViewer)) as ScrollViewer;
			var scrollBar = GetDescendantByType(scrollViewer, typeof(ScrollBar)) as ScrollBar;
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
	}
}
