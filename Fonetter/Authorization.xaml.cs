using CoreTweet;
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
using System.Windows.Shapes;

namespace Fonetter {
	/// <summary>
	/// Authorization.xaml の相互作用ロジック
	/// </summary>
	public partial class Authorization : Window {
		private ObservableCollection<Consumers> cons;
		private string[] conNames { get { return cons.Select(x => x.Name).ToArray(); } }
		private OAuth.OAuthSession session;
		private ObservableCollection<Users> userCol;
		private ObservableCollection<Selections> selects;
		public Tokens[] AuthedToken {
			get {
				if(selects.Count == 0) return null;
				try {
					var tmp = selects.Select(x => Tokens.Create(x.ConsumerKey, cons.First(y => y.Key == x.ConsumerKey).Secret, userCol.First(y => y.Id == x.Id).Key.First(y => y.ConsumerKey == x.ConsumerKey).AccessToken, userCol.First(y => y.Id == x.Id).Key.First(y => y.ConsumerKey == x.ConsumerKey).AccessSecret)).ToList();
					foreach(var t in tmp) {
						var uz = t.Account.VerifyCredentials();
						t.UserId = (long)uz.Id;
					}
					return tmp.ToArray();
				}catch(Exception e) {
					return null;
				}
			}
		}

		public Authorization(int index = 0) {
			cons = Sql.LoadConsumers();
			userCol = Sql.LoadUsers();
			selects = Sql.LoadSelections();

			InitializeComponent();
			tabc.SelectedIndex = index;

			dgcData.ItemsSource = cons;
			UpdateConsumer();
			UpdateUser();
			UpdateSelection();
			cbuUser.SelectedIndex = 0;
			dguData.ItemsSource = selects;
		}

		private void btcReload_Click(object sender, RoutedEventArgs e) {
			cons = Sql.LoadConsumers();
			dgcData.ItemsSource = cons;
			UpdateConsumer();
		}

		private void btcUpdate_Click(object sender, RoutedEventArgs e) {
			UpdateConsumer();
		}

		private void btcAdd_Click(object sender, RoutedEventArgs e) {
			var tmp = new Consumers(tbcKey.Text, tbcSecret.Text, tbcName.Text);
			if(cons.Any(x => x.Key == tmp.Key || x.Name == tmp.Name)) {
				MessageBox.Show("重複しています。", "Error");
				return;
			}

			cons.Add(tmp);
			tbcKey.Text = tbcSecret.Text = tbcName.Text = "";
			UpdateConsumer();
		}

		private void btcEdit_Click(object sender, RoutedEventArgs e) {
			int si = dgcData.SelectedIndex;
			if(si == -1) {
				MessageBox.Show("選択してください。", "Information");
				return;
			}

			var tmp = cons[si];
			tbcName.Text = tmp.Name;
			tbcKey.Text = tmp.Key;
			tbcSecret.Text = tmp.Secret;
			cons.RemoveAt(si);
			UpdateConsumer();
		}

		private void UpdateConsumer() {
			Sql.UpdateConsumers(cons.ToArray());
			cbuConsumer.Items.Clear();
			foreach(var s in conNames) {
				cbuConsumer.Items.Add(s);
			}
		}

		private void UpdateUser() {
			Sql.UpdateUsers(userCol.ToList());
			cbuUser.Items.Clear();
			cbuUser.Items.Add("<New User>");
			foreach(var uc in userCol) {
				cbuUser.Items.Add(uc.ScreenName);
			}
		}

		private void UpdateSelection() {
			Sql.UpdateSelections(selects.ToArray());
			selects = Sql.LoadSelections();
			foreach(var s in selects) {
				var uf = userCol.FirstOrDefault(x => x.Id == s.Id);
				var cf = cons.First(x => x.Key == s.ConsumerKey);
				if(uf == null || uf == null) {
					selects.Remove(s);
					continue;
				} else {
					s.ScreenName = uf.ScreenName;
					s.Name = cf.Name;
				}
			}
			dguData.ItemsSource = selects;
		}

		private void btuExecute_Click(object sender, RoutedEventArgs e) {
			int si = cbuConsumer.SelectedIndex;
			if(si == -1) {
				MessageBox.Show("選択してください。", "Error");
				return;
			}
			UpdateConsumer();
			string select = cbuConsumer.Items[si].ToString();
			if(!conNames.Any(x => x == select)) {
				MessageBox.Show("不明なエラー。", "Error");
				return;
			}

			si = cbuUser.SelectedIndex;
			if(si == -1) {
				MessageBox.Show("選択してください。", "Error");
				return;
			}
			string u_select = cbuUser.Items[si].ToString();
			if(u_select != "<New User>" && userCol.Any(x => x.ScreenName == u_select)) {
				var uca = userCol.First(x => x.ScreenName == u_select);
				var ckey = cons.First(x => x.Name == select).Key;
				if(uca.Key.Any(x => x.ConsumerKey == ckey)) {
					if(selects.Any(x => x.Id == uca.Id)) {
						var sa = selects.First(x => x.Id == uca.Id);
						sa.ConsumerKey = ckey;
					} else {
						selects.Add(new Selections(uca.Id, ckey));
					}
					UpdateSelection();
					return;
				}
			}

			var tmp = cons.First(x => x.Name == select);
			try {
				session = OAuth.Authorize(tmp.Key, tmp.Secret);
				System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
			} catch(Exception te) {
				MessageBox.Show(te.Message, "Error");
			}
			btuConfirm.IsEnabled = true;
			btuExecute.IsEnabled = false;
		}

		private void btuConfirm_Click(object sender, RoutedEventArgs e) {
			int pars;
			if(tbuPin.Text == "" || !int.TryParse(tbuPin.Text, out pars)) {
				MessageBox.Show("PINが入力されていない、もしくは不正です。", "Error");
				return;
			}

			try {
				var token = session.GetTokens(tbuPin.Text);
				var uc = token.Account.VerifyCredentials();
				
				if(userCol.Any(x => x.Id == token.UserId)) {
					var ua = userCol.First(x => x.Id == token.UserId);
					ua.UpdateName(uc.ScreenName);
					if(ua.Key.Any(x => x.ConsumerKey == token.ConsumerKey)) {
						ua.Key.Remove(ua.Key.First(x => x.ConsumerKey == token.ConsumerKey));
					}
					ua.Key.Add(new Fonetter.Keys(token.ConsumerKey, token.AccessToken, token.AccessTokenSecret));
				} else {
					userCol.Add(new Users(token.UserId, uc.ScreenName, token.ConsumerKey, token.AccessToken, token.AccessTokenSecret));
				}
				if(selects.Any(x => x.Id == token.UserId)) {
					var sa = selects.First(x => x.Id == token.UserId);
					sa.ConsumerKey = token.ConsumerKey;
				} else {
					selects.Add(new Selections(token.UserId, token.ConsumerKey));
				}
				UpdateUser();
				UpdateSelection();
				tbuPin.Text = "";
			} catch(Exception te) {
				MessageBox.Show(te.Message, "Error");
			}
			btuConfirm.IsEnabled = false;
			btuExecute.IsEnabled = true;
		}

		private void cbuConsumer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(cbuConsumer.SelectedItem == null) return;
			btuExecute.IsEnabled = true;
		}
	}
}
