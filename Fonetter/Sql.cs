using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;

namespace Fonetter {
	class Sql {
		public static void CheckDb() {
			var db_table = new List<string>();
			var tableList = new List<string>() { "ConsumerKey", "User", "Selection" };
			
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var com = con.CreateCommand()) {
					com.CommandText = "select * from sqlite_master where type='table'";
					using(var reader = com.ExecuteReader()) {
						while(reader.Read()) {
							db_table.Add(reader["tbl_name"].ToString());
						}
					}
				}
				con.Close();
			}

			foreach(var dt in db_table) {
				tableList.Remove(dt);
			}
			if(tableList.Count == 0) Console.WriteLine("Success: DbCheck");
			else {
				CreateKeyTable(tableList.ToArray());
			}
		}

		public static void CreateKeyTable(string[] list) {
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var com = con.CreateCommand()) {
					foreach(var l in list) {
						switch(l) {
							case "ConsumerKey":
								com.CommandText = "create table ConsumerKey(Key TEXT, Secret TEXT, Name TEXT)";
								break;
							case "User":
								com.CommandText = "create table User(Id TEXT, ScreenName TEXT, ConsumerKey TEXT, AccessToken TEXT, AccessSecret TEXT)";
								break;
							case "Selection":
								com.CommandText = "create table Selecton(Id TEXT, ConsumerKey TEXT)";
								break;
						}
						com.ExecuteNonQuery();
					}
				}
				con.Close();
			}
		}

		public static ObservableCollection<Consumers> LoadConsumers() {
			var list = new ObservableCollection<Consumers>();
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var com = con.CreateCommand()) {
					com.CommandText = "select * from ConsumerKey";
					using(var reader = com.ExecuteReader()) {
						while(reader.Read()) {
							list.Add(new Consumers(reader["Key"].ToString(), reader["Secret"].ToString(), reader["Name"].ToString()));
						}
					}
				}
				con.Close();
			}
			return list;
		}

		public static void UpdateConsumers(Consumers[] consumer) {
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var sqlt = con.BeginTransaction()) {
					using(var com = con.CreateCommand()) {
						com.CommandText = "delete from ConsumerKey";
						com.ExecuteNonQuery();

						foreach(var c in consumer) {
							com.CommandText = "insert into ConsumerKey (Key,Secret,Name) values('" + c.Key + "', '" + c.Secret + "', '" + c.Name + "')";
							com.ExecuteNonQuery();
						}
					}
					sqlt.Commit();
				}
				con.Close();
			}
		}

		public static ObservableCollection<Users> LoadUsers() {
			var list = new ObservableCollection<Users>();
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var com = con.CreateCommand()) {
					com.CommandText = "select * from User";
					using(var reader = com.ExecuteReader()) {
						while(reader.Read()) {
							long id = long.Parse(reader["id"].ToString());
							if(list.Any(x => x.Id == id)){
								list.First(x => x.Id == id).Key.Add(new Keys(reader["ConsumerKey"].ToString(), reader["AccessToken"].ToString(), reader["AccessSecret"].ToString()));
							} else {
								list.Add(new Users(id, reader["ScreenName"].ToString(), reader["ConsumerKey"].ToString(), reader["AccessToken"].ToString(), reader["AccessSecret"].ToString()));
							}
						}
					}
				}
				con.Close();
			}
			return list;
		}

		public static void UpdateUsers(List<Users> user) {
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var sqlt = con.BeginTransaction()) {
					using(var com = con.CreateCommand()) {
						com.CommandText = "delete from User";
						com.ExecuteNonQuery();

						foreach(var c in user) {
							foreach(var k in c.Key) {
								com.CommandText = "insert into User (Id,ScreenName,ConsumerKey,AccessToken,AccessSecret) values('" + c.Id + "', '" + c.ScreenName + "', '" + k.ConsumerKey + "', '" + k.AccessToken + "', '" + k.AccessSecret + "')";
								com.ExecuteNonQuery();
							}
						}
					}
					sqlt.Commit();
				}
				con.Close();
			}
		}

		public static ObservableCollection<Selections> LoadSelections() {
			var list = new ObservableCollection<Selections>();
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var com = con.CreateCommand()) {
					com.CommandText = "select * from Selection";
					using(var reader = com.ExecuteReader()) {
						while(reader.Read()) {
							list.Add(new Selections(long.Parse(reader["Id"].ToString()), reader["ConsumerKey"].ToString()));
						}
					}
				}
				con.Close();
			}
			return list;
		}

		public static void UpdateSelections(Selections[] select) {
			using(var con = new SQLiteConnection("Data Source=Keys.db")) {
				con.Open();
				using(var sqlt = con.BeginTransaction()) {
					using(var com = con.CreateCommand()) {
						com.CommandText = "delete from Selection";
						com.ExecuteNonQuery();

						foreach(var c in select) {
							com.CommandText = "insert into Selection (Id,ConsumerKey) values('" + c.Id + "', '" + c.ConsumerKey + "')";
							com.ExecuteNonQuery();
						}
					}
					sqlt.Commit();
				}
				con.Close();
			}
		}
	}
}
