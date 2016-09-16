using System;
using System.Windows;
using System.Windows.Controls;
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
			setFor.RowDefinitions.Clear();
			for(int i = 0; i < row; i++) {
				setFor.RowDefinitions.Add(new RowDefinition());
			}
			setFor.ColumnDefinitions.Clear();
			for(int i = 0; i < column; i++) {
				setFor.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}
	}
}
