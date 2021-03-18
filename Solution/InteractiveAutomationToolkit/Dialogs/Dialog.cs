﻿namespace Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;

	/// <summary>
	///     A dialog represents a single window that can be shown.
	///     Widgets can be shown on the window by adding them to the dialog.
	///     The dialog uses a grid to layout its widgets.
	/// </summary>
	public abstract class Dialog
	{
		private const string Auto = "auto";
		private const string Stretch = "*";

		private readonly Dictionary<Widget, IWidgetLayout> widgetLayouts = new Dictionary<Widget, IWidgetLayout>();
		private readonly List<string> columnDefinitions = new List<string>();
		private readonly List<string> rowDefinitions = new List<string>();

		private int height;
		private int maxHeight;
		private int maxWidth;
		private int minHeight;
		private int minWidth;
		private int width;
		private int rowCount;
		private int columnCount;
		private bool isEnabled = true;

		protected Dialog(Engine engine)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			Engine = engine;
			width = -1;
			height = -1;
			MaxHeight = Int32.MaxValue;
			MinHeight = 1;
			MaxWidth = Int32.MaxValue;
			MinWidth = 1;
			RowCount = 0;
			ColumnCount = 0;
			Title = "Dialog";
			AllowOverlappingWidgets = false;
		}

		/// <summary>
		/// Gets or sets a value indicating whether overlapping widgets are allowed or not.
		/// Can be used in case you want to add multiple widgets to the same cell in the dialog.
		/// You can use the Margin property on the widgets to place them apart.
		/// </summary>
		public bool AllowOverlappingWidgets { get; set; }

		/// <summary>
		///     Event triggers when the back button of the dialog is pressed.
		/// </summary>
		public event EventHandler<EventArgs> Back;

		/// <summary>
		///     Event triggers when the forward button of the dialog is pressed.
		/// </summary>
		public event EventHandler<EventArgs> Forward;

		/// <summary>
		///     Event triggers when there is any user interaction.
		/// </summary>
		public event EventHandler<EventArgs> Interacted;

		/// <summary>
		///     Gets the number of columns of the grid layout.
		/// </summary>
		public int ColumnCount
		{
			get
			{
				return columnCount;
			}

			private set
			{
				if (this.columnCount < value)
				{
					this.columnDefinitions.AddRange(Enumerable.Repeat(Auto, value - this.columnCount));
					this.columnCount = value;
				}
				else if (this.columnCount > value)
				{
					for (int i = this.columnCount - 1; i > value; i--)
					{
						this.columnDefinitions.RemoveAt(i);
					}

					this.columnCount = value;
				}
				else
				{
					// nothing to do
				}
			}
		}

		/// <summary>
		///     Gets the link to the SLAutomation process.
		/// </summary>
		public Engine Engine { get; private set; }

		/// <summary>
		///     Gets or sets the fixed height (in pixels) of the dialog.
		/// </summary>
		/// <remarks>
		///     The user will still be able to resize the window,
		///     but scrollbars will appear immediately.
		///     <see cref="MinHeight" /> should be used instead as it has a more desired effect.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">When the value is smaller than 1.</exception>
		public int Height
		{
			get
			{
				return height;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				height = value;
			}
		}

		/// <summary>
		///     Gets or sets the maximum height (in pixels) of the dialog.
		/// </summary>
		/// <remarks>
		///     The user will still be able to resize the window past this limit.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">When value is smaller than 1.</exception>
		public int MaxHeight
		{
			get
			{
				return maxHeight;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				maxHeight = value;
			}
		}

		/// <summary>
		///     Gets or sets the maximum width (in pixels) of the dialog.
		/// </summary>
		/// <remarks>
		///     The user will still be able to resize the window past this limit.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">When value is smaller than 1.</exception>
		public int MaxWidth
		{
			get
			{
				return maxWidth;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				maxWidth = value;
			}
		}

		/// <summary>
		///     Gets or sets the minimum height (in pixels) of the dialog.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When value is smaller than 1.</exception>
		public int MinHeight
		{
			get
			{
				return minHeight;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				minHeight = value;
			}
		}

		/// <summary>
		///     Gets or sets the minimum width (in pixels) of the dialog.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When value is smaller than 1.</exception>
		public int MinWidth
		{
			get
			{
				return minWidth;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				minWidth = value;
			}
		}

		/// <summary>
		///     Gets the number of rows in the grid layout.
		/// </summary>
		public int RowCount
		{
			get
			{
				return rowCount;
			}

			private set
			{
				if (this.rowCount < value)
				{
					this.rowDefinitions.AddRange(Enumerable.Repeat(Auto, value - this.rowCount));
					this.rowCount = value;
				}
				else if (this.rowCount > value)
				{
					for (int i = this.rowCount - 1; i > value; i--)
					{
						this.rowDefinitions.RemoveAt(i);
					}

					this.rowCount = value;
				}
				else
				{
					// nothing to do
				}
			}
		}

		/// <summary>
		///		Gets or sets a value indicating whether the Interactive Widgets within the dialog are enabled or not.
		/// </summary>
		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}

			set
			{
				isEnabled = value;
				foreach (Widget widget in Widgets)
				{
					InteractiveWidget interactiveWidget = widget as InteractiveWidget;
					if (interactiveWidget != null && !(interactiveWidget is CollapseButton))
					{
						interactiveWidget.IsEnabled = isEnabled;
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the title at the top of the window.
		/// </summary>
		/// <remarks>Available from DataMiner 9.6.6 onwards.</remarks>
		public string Title { get; set; }

		/// <summary>
		///     Gets widgets that are added to the dialog.
		/// </summary>
		public IEnumerable<Widget> Widgets
		{
			get
			{
				return widgetLayouts.Keys;
			}
		}

		/// <summary>
		///     Gets or sets the fixed width (in pixels) of the dialog.
		/// </summary>
		/// <remarks>
		///     The user will still be able to resize the window,
		///     but scrollbars will appear immediately.
		///     <see cref="MinWidth" /> should be used instead as it has a more desired effect.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">When the value is smaller than 1.</exception>
		public int Width
		{
			get
			{
				return width;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				width = value;
			}
		}

		/// <summary>
		///     Adds a widget to the dialog.
		/// </summary>
		/// <param name="widget">Widget to add to the dialog.</param>
		/// <param name="widgetLayout">Location of the widget on the grid layout.</param>
		/// <returns>The dialog.</returns>
		/// <exception cref="ArgumentNullException">When the widget is null.</exception>
		/// <exception cref="ArgumentException">When the widget is already added to the dialog.</exception>
		/// <exception cref="ArgumentException">When the widget overlaps with another widget.</exception>
		public Dialog AddWidget(Widget widget, IWidgetLayout widgetLayout)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			if (widgetLayouts.ContainsKey(widget))
			{
				throw new ArgumentException("Widget is already added to the dialog");
			}

			widgetLayouts.Add(widget, widgetLayout);

			HashSet<int> rowsInUse;
			HashSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);

			return this;
		}

		/// <summary>
		///     Adds a widget to the dialog.
		/// </summary>
		/// <param name="widget">Widget to add to the dialog.</param>
		/// <param name="row">Row location of widget on the grid.</param>
		/// <param name="column">Column location of the widget on the grid.</param>
		/// <param name="horizontalAlignment">Horizontal alignment of the widget.</param>
		/// <param name="verticalAlignment">Vertical alignment of the widget.</param>
		/// <returns>The dialog.</returns>
		/// <exception cref="ArgumentNullException">When the widget is null.</exception>
		/// <exception cref="ArgumentException">When the location is out of bounds of the grid.</exception>
		/// <exception cref="ArgumentException">When the widget is already added to the dialog.</exception>
		public Dialog AddWidget(
			Widget widget,
			int row,
			int column,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Center)
		{
			AddWidget(widget, new WidgetLayout(row, column, horizontalAlignment, verticalAlignment));
			return this;
		}

		/// <summary>
		///     Adds a widget to the dialog.
		/// </summary>
		/// <param name="widget">Widget to add to the dialog.</param>
		/// <param name="fromRow">Row location of widget on the grid.</param>
		/// <param name="fromColumn">Column location of the widget on the grid.</param>
		/// <param name="rowSpan">Number of rows the widget will use.</param>
		/// <param name="colSpan">Number of columns the widget will use.</param>
		/// <param name="horizontalAlignment">Horizontal alignment of the widget.</param>
		/// <param name="verticalAlignment">Vertical alignment of the widget.</param>
		/// <returns>The dialog.</returns>
		/// <exception cref="ArgumentNullException">When the widget is null.</exception>
		/// <exception cref="ArgumentException">When the location is out of bounds of the grid.</exception>
		/// <exception cref="ArgumentException">When the widget is already added to the dialog.</exception>
		public Dialog AddWidget(
			Widget widget,
			int fromRow,
			int fromColumn,
			int rowSpan,
			int colSpan,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Center)
		{
			AddWidget(
				widget,
				new WidgetLayout(fromRow, fromColumn, rowSpan, colSpan, horizontalAlignment, verticalAlignment));
			return this;
		}

		/// <summary>
		///     Gets the layout of the widget in the dialog.
		/// </summary>
		/// <param name="widget">A widget that is part of the dialog.</param>
		/// <returns>The widget layout in the dialog.</returns>
		/// <exception cref="NullReferenceException">When the widget is null.</exception>
		/// <exception cref="ArgumentException">When the widget is not part of the dialog.</exception>
		public IWidgetLayout GetWidgetLayout(Widget widget)
		{
			CheckWidgetExists(widget);
			return widgetLayouts[widget];
		}

		/// <summary>
		///     Removes a widget from the dialog.
		/// </summary>
		/// <param name="widget">Widget to remove.</param>
		/// <exception cref="ArgumentNullException">When the widget is null.</exception>
		public void RemoveWidget(Widget widget)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			widgetLayouts.Remove(widget);

			HashSet<int> rowsInUse;
			HashSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);
		}

		/// <summary>
		/// Adds the widgets from the section to the Dialog.
		/// </summary>
		/// <param name="section">Section to be added to the Dialog.</param>
		/// <param name="layout">Left top position of the Section within the Dialog.</param>
		/// <returns>Updated Dialog.</returns>
		public Dialog AddSection(Section section, SectionLayout layout)
		{
			foreach(Widget widget in section.Widgets)
			{
				IWidgetLayout widgetLayout = section.GetWidgetLayout(widget);
				AddWidget(
					widget,
					new WidgetLayout(
						widgetLayout.Row + layout.Row,
						widgetLayout.Column + layout.Column,
						widgetLayout.RowSpan,
						widgetLayout.ColumnSpan,
						widgetLayout.HorizontalAlignment,
						widgetLayout.VerticalAlignment));
			}

			return this;
		}

		/// <summary>
		/// Adds the widgets from the section to the Dialog.
		/// </summary>
		/// <param name="section">Section to be added to the Dialog.</param>
		/// <param name="fromRow">Row in the Dialog where the Section should be added.</param>
		/// <param name="fromColumn">Column in the Dialog where the Section should be added.</param>
		/// <returns>Updated Dialog.</returns>
		public Dialog AddSection(Section section, int fromRow, int fromColumn)
		{
			return AddSection(section, new SectionLayout(fromRow, fromColumn));
		}

		/// <summary>
		///     Apply a fixed width (in pixels) to a column.
		/// </summary>
		/// <param name="column">The index of the column in the grid.</param>
		/// <param name="columnWidth">The width of the column.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the column index does not exist.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the column width is smaller than 0.</exception>
		public void SetColumnWidth(int column, int columnWidth)
		{
			if ((column < 0) || (column >= ColumnCount))
			{
				throw new ArgumentOutOfRangeException("column");
			}

			if (columnWidth < 0)
			{
				throw new ArgumentOutOfRangeException("columnWidth");
			}

			columnDefinitions[column] = columnWidth.ToString();
		}

		/// <summary>
		///     The width of the column will be automatically adapted to the widest dialog box item in that column.
		/// </summary>
		/// <param name="column">The index of the column in the grid.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the column index does not exist.</exception>
		public void SetColumnWidthAuto(int column)
		{
			if ((column < 0) || (column >= ColumnCount))
			{
				throw new ArgumentOutOfRangeException("column");
			}

			columnDefinitions[column] = Auto;
		}

		/// <summary>
		///     The column will have the largest possible width, depending on the width of the other columns.
		/// </summary>
		/// <param name="column">The index of the column in the grid.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the column index does not exist.</exception>
		public void SetColumnWidthStretch(int column)
		{
			if ((column < 0) || (column >= ColumnCount))
			{
				throw new ArgumentOutOfRangeException("column");
			}

			columnDefinitions[column] = Stretch;
		}

		/// <summary>
		///     Apply a fixed height (in pixels) to a row.
		/// </summary>
		/// <param name="row">The index of the row in the grid.</param>
		/// <param name="rowHeight">The height of the column.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the row index does not exist.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the row height is smaller than 0.</exception>
		public void SetRowHeight(int row, int rowHeight)
		{
			if ((row < 0) || (row >= RowCount))
			{
				throw new ArgumentOutOfRangeException("row");
			}

			if (rowHeight <= 0)
			{
				throw new ArgumentOutOfRangeException("rowHeight");
			}

			rowDefinitions[row] = rowHeight.ToString();
		}

		/// <summary>
		///     The height of the row will be automatically adapted to the highest dialog box item in that row.
		/// </summary>
		/// <param name="row">The index of the row in the grid.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the row index does not exist.</exception>
		public void SetRowHeightAuto(int row)
		{
			if ((row < 0) || (row >= RowCount))
			{
				throw new ArgumentOutOfRangeException("row");
			}

			rowDefinitions[row] = Auto;
		}

		/// <summary>
		///     The row will have the largest possible height, depending on the height of the other rows.
		/// </summary>
		/// <param name="row">The index of the row in the grid.</param>
		/// <exception cref="ArgumentOutOfRangeException">When the row index does not exist.</exception>
		public void SetRowHeightStretch(int row)
		{
			if ((row < 0) || (row >= RowCount))
			{
				throw new ArgumentOutOfRangeException("row");
			}

			rowDefinitions[row] = Stretch;
		}

		/// <summary>
		///     Sets the layout of the widget in the dialog.
		/// </summary>
		/// <param name="widget">A widget that is part of the dialog.</param>
		/// <param name="widgetLayout">The layout to apply on the widget.</param>
		/// <exception cref="NullReferenceException">When widget is null.</exception>
		/// <exception cref="ArgumentException">When the widget is not part of the dialog.</exception>
		/// <exception cref="NullReferenceException">When widgetLayout is null.</exception>
		/// <exception cref="ArgumentException">When widgetLayout is out of bounds of the dialog grid.</exception>
		public void SetWidgetLayout(Widget widget, IWidgetLayout widgetLayout)
		{
			CheckWidgetExists(widget);
			widgetLayouts[widget] = widgetLayout;
		}

		/// <summary>
		///     Shows the dialog window.
		///     Also loads changes and triggers events when <paramref name="requireResponse" /> is <c>true</c>.
		/// </summary>
		/// <param name="requireResponse">If the dialog expects user interaction.</param>
		/// <remarks>Should only be used when you create your own event loop.</remarks>
		public void Show(bool requireResponse = true)
		{
			UIBuilder uib = Build();
			uib.RequireResponse = requireResponse;

			UIResults uir = Engine.ShowUI(uib);

			if (requireResponse)
			{
				LoadChanges(uir);
				RaiseResultEvents(uir);
			}
		}

		/// <summary>
		/// Removes all Widgets from the Section.
		/// </summary>
		public void Clear()
		{
			widgetLayouts.Clear();
			RowCount = 0;
			ColumnCount = 0;
		}

		private static string AlignmentToUiString(HorizontalAlignment horizontalAlignment)
		{
			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Center:
					return "Center";
				case HorizontalAlignment.Left:
					return "Left";
				case HorizontalAlignment.Right:
					return "Right";
				case HorizontalAlignment.Stretch:
					return "Stretch";
				default:
					throw new InvalidEnumArgumentException(
						"horizontalAlignment",
						(int)horizontalAlignment,
						typeof(HorizontalAlignment));
			}
		}

		private static string AlignmentToUiString(VerticalAlignment verticalAlignment)
		{
			switch (verticalAlignment)
			{
				case VerticalAlignment.Center:
					return "Center";
				case VerticalAlignment.Top:
					return "Top";
				case VerticalAlignment.Bottom:
					return "Bottom";
				case VerticalAlignment.Stretch:
					return "Stretch";
				default:
					throw new InvalidEnumArgumentException(
						"verticalAlignment",
						(int)verticalAlignment,
						typeof(VerticalAlignment));
			}
		}

		/// <summary>
		/// Checks if any visible widgets in the Dialog overlap.
		/// </summary>
		/// <exception cref="OverlappingWidgetsException">Thrown when two visible widgets overlap with each other.</exception>
		private void CheckIfVisibleWidgetsOverlap()
		{
			if (AllowOverlappingWidgets) return;

			foreach(Widget widget in widgetLayouts.Keys)
			{
				if (!widget.IsVisible) continue;

				IWidgetLayout widgetLayout = widgetLayouts[widget];
				for (int column = widgetLayout.Column; column < widgetLayout.Column + widgetLayout.ColumnSpan; column++)
				{
					for (int row = widgetLayout.Row; row < widgetLayout.Row + widgetLayout.RowSpan; row++)
					{
						foreach(Widget otherWidget in widgetLayouts.Keys)
						{
							if (!otherWidget.IsVisible || widget.Equals(otherWidget)) continue;

							IWidgetLayout otherWidgetLayout = widgetLayouts[otherWidget];
							if (column >= otherWidgetLayout.Column && column < otherWidgetLayout.Column + otherWidgetLayout.ColumnSpan && row >= otherWidgetLayout.Row && row < otherWidgetLayout.Row + otherWidgetLayout.RowSpan)
							{
								throw new OverlappingWidgetsException(String.Format("The widget overlaps with another widget in the Dialog on Row {0}, Column {1}, RowSpan {2}, ColumnSpan {3}", widgetLayout.Row, widgetLayout.Column, widgetLayout.RowSpan, widgetLayout.ColumnSpan));
							}
						}
					}
				}
			}
		}

		private UIBuilder Build()
		{
			// Check rows and columns in use
			HashSet<int> rowsInUse;
			HashSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);

			// Check if visible widgets overlap and throw exception if this is the case
			CheckIfVisibleWidgetsOverlap();

			// Initialize UI Builder
			var uiBuilder = new UIBuilder
			{
				Height = Height,
				MinHeight = MinHeight,
				Width = Width,
				MinWidth = MinWidth,
				RowDefs = String.Join(";", rowDefinitions),
				ColumnDefs = String.Join(";", columnDefinitions),
				Title = Title
			};

			List<int> orderedRowsInUse = rowsInUse.OrderBy(x => x).ToList();
			List<int> orderedColumnsInUse = columnsInUse.OrderBy(x => x).ToList();

			KeyValuePair<Widget, IWidgetLayout> defaultKeyValuePair = default(KeyValuePair<Widget, IWidgetLayout>);
			for (int rowIndex = 0; rowIndex < orderedRowsInUse.Count; rowIndex++)
			{
				for (int columnIndex = 0; columnIndex < orderedColumnsInUse.Count; columnIndex++)
				{
					foreach (KeyValuePair<Widget, IWidgetLayout> keyValuePair in widgetLayouts.Where(x => x.Key.IsVisible && x.Key.Type != UIBlockType.Undefined && x.Value.Row.Equals(orderedRowsInUse[rowIndex]) && x.Value.Column.Equals(orderedColumnsInUse[columnIndex])))
					{
						if (keyValuePair.Equals(defaultKeyValuePair)) continue;

						// Can be removed once we retrieve all collapsed states from the UI
						TreeView treeView = keyValuePair.Key as TreeView;
						if (treeView != null) treeView.UpdateItemCache();

						UIBlockDefinition widgetBlockDefinition = keyValuePair.Key.BlockDefinition;
						IWidgetLayout widgetLayout = keyValuePair.Value;

						widgetBlockDefinition.Column = columnIndex;
						widgetBlockDefinition.ColumnSpan = widgetLayout.ColumnSpan;
						widgetBlockDefinition.Row = rowIndex;
						widgetBlockDefinition.RowSpan = widgetLayout.RowSpan;
						widgetBlockDefinition.HorizontalAlignment = AlignmentToUiString(widgetLayout.HorizontalAlignment);
						widgetBlockDefinition.VerticalAlignment = AlignmentToUiString(widgetLayout.VerticalAlignment);
						widgetBlockDefinition.Margin = widgetLayout.Margin.ToString();

						uiBuilder.AppendBlock(widgetBlockDefinition);
					}
				}
			}

			return uiBuilder;
		}

		/// <summary>
		/// Used to retrieve the rows and columns that are being used and updates the RowCount and ColumnCount properties based on the Widgets added to the dialog.
		/// </summary>
		/// <param name="rowsInUse">Collection containing the rows that are defined by the Widgets in the Dialog.</param>
		/// <param name="columnsInUse">Collection containing the columns that are defined by the Widgets in the Dialog.</param>
		private void FillRowsAndColumnsInUse(out HashSet<int> rowsInUse, out HashSet<int> columnsInUse)
		{
			rowsInUse = new HashSet<int>();
			columnsInUse = new HashSet<int>();
			foreach (KeyValuePair<Widget, IWidgetLayout> keyValuePair in this.widgetLayouts)
			{
				if (keyValuePair.Key.IsVisible && keyValuePair.Key.Type != UIBlockType.Undefined)
				{
					for (int i = keyValuePair.Value.Row; i < keyValuePair.Value.Row + keyValuePair.Value.RowSpan; i++)
					{
						rowsInUse.Add(i);
					}

					for (int i = keyValuePair.Value.Column; i < keyValuePair.Value.Column + keyValuePair.Value.ColumnSpan; i++)
					{
						columnsInUse.Add(i);
					}
				}
			}

			this.RowCount = rowsInUse.Count;
			this.ColumnCount = columnsInUse.Count;
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		private void CheckWidgetExists(Widget widget)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			if (!widgetLayouts.ContainsKey(widget))
			{
				throw new ArgumentException("Widget is not part of this dialog");
			}
		}

		private void LoadChanges(UIResults uir)
		{
			foreach (InteractiveWidget interactiveWidget in Widgets.OfType<InteractiveWidget>())
			{
				if (interactiveWidget.IsVisible)
				{
					interactiveWidget.LoadResult(uir);
				}
			}
		}

		private void RaiseResultEvents(UIResults uir)
		{
			if (Interacted != null)
			{
				Interacted(this, EventArgs.Empty);
			}

			if (uir.WasBack() && (Back != null))
			{
				Back(this, EventArgs.Empty);
				return;
			}

			if (uir.WasForward() && (Forward != null))
			{
				Forward(this, EventArgs.Empty);
				return;
			}

			// ToList is necessary to prevent InvalidOperationException when adding or removing widgets from a event handler.
			List<InteractiveWidget> intractableWidgets = Widgets.OfType<InteractiveWidget>()
				.Where(widget => widget.WantsOnChange).ToList();

			foreach (InteractiveWidget intractable in intractableWidgets)
			{
				intractable.RaiseResultEvents();
			}
		}
	}
}
