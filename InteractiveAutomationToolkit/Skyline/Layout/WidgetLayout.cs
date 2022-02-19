﻿namespace Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit
{
	using System;

	/// <summary>
	/// Used to define the position of a widget in a grid layout.
	/// </summary>
	public class WidgetLayout
	{
		private int column;
		private int columnSpan;
		private Margin margin;
		private int row;
		private int rowSpan;

		/// <summary>
		/// Initializes a new instance of the <see cref="WidgetLayout"/> class.
		/// </summary>
		/// <param name="fromRow">Row index of top-left cell.</param>
		/// <param name="fromColumn">Column index of the top-left cell.</param>
		/// <param name="rowSpan">Number of vertical cells the widget spans across.</param>
		/// <param name="columnSpan">Number of horizontal cells the widget spans across.</param>
		/// <param name="horizontalAlignment">Horizontal alignment of the widget.</param>
		/// <param name="verticalAlignment">Vertical alignment of the widget.</param>
		public WidgetLayout(
			int fromRow,
			int fromColumn,
			int rowSpan,
			int columnSpan,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Top)
		{
			Row = fromRow;
			Column = fromColumn;
			RowSpan = rowSpan;
			ColumnSpan = columnSpan;
			HorizontalAlignment = horizontalAlignment;
			VerticalAlignment = verticalAlignment;
			Margin = new Margin();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WidgetLayout"/> class.
		/// </summary>
		/// <param name="row">Row index of the cell where the widget is placed.</param>
		/// <param name="column">Column index of the cell where the widget is placed.</param>
		/// <param name="horizontalAlignment">Horizontal alignment of the widget.</param>
		/// <param name="verticalAlignment">Vertical alignment of the widget.</param>
		public WidgetLayout(
			int row,
			int column,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Top) : this(
			row,
			column,
			1,
			1,
			horizontalAlignment,
			verticalAlignment)
		{
		}

		/// <summary>
		///     Gets or sets the column location of the widget on the grid.
		/// </summary>
		public int Column
		{
			get
			{
				return column;
			}

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				column = value;
			}
		}

		/// <summary>
		///     Gets or sets how many columns the widget spans on the grid.
		/// </summary>
		public int ColumnSpan
		{
			get
			{
				return columnSpan;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				columnSpan = value;
			}
		}

		/// <summary>
		///     Gets or sets the horizontal alignment of the widget.
		/// </summary>
		public HorizontalAlignment HorizontalAlignment { get; set; }

		/// <summary>
		///     Gets or sets the margin around the widget.
		/// </summary>
		/// <exception cref="ArgumentNullException">When the value is null.</exception>
		public Margin Margin
		{
			get
			{
				return margin;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				margin = value;
			}
		}

		/// <summary>
		///     Gets or sets the row location of the widget on the grid.
		/// </summary>
		public int Row
		{
			get
			{
				return row;
			}

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				row = value;
			}
		}

		/// <summary>
		///     Gets or sets how many rows the widget spans on the grid.
		/// </summary>
		public int RowSpan
		{
			get
			{
				return rowSpan;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				rowSpan = value;
			}
		}

		/// <summary>
		///     Gets or sets the vertical alignment of the widget.
		/// </summary>
		public VerticalAlignment VerticalAlignment { get; set; }

		/// <summary>
		///		Determines if the current layout overlaps with the specified other layout.
		/// </summary>
		/// <param name="other">The other layout to compare the current layout to.</param>
		/// <returns>Whether the layout overlaps with <paramref name="other"/>.</returns>
		public bool Overlaps(WidgetLayout other)
		{
			// https://stackoverflow.com/a/20925869
			bool rowsOverlap = row + rowSpan > other.row && other.row + other.rowSpan > row;
			bool columnsOverlap = column + columnSpan > other.column && other.column + other.columnSpan > column;

			return rowsOverlap && columnsOverlap;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			WidgetLayout other = obj as WidgetLayout;
			if (other == null) return false;

			bool rowMatch = Row.Equals(other.Row);
			bool columnMatch = Column.Equals(other.Column);
			bool rowSpanMatch = RowSpan.Equals(other.RowSpan);
			bool columnSpanMatch = ColumnSpan.Equals(other.ColumnSpan);
			bool horizontalAlignmentMatch = HorizontalAlignment.Equals(other.HorizontalAlignment);
			bool verticalAlignmentMatch = VerticalAlignment.Equals(other.VerticalAlignment);

			bool rowParamsMatch = rowMatch && rowSpanMatch;
			bool columnParamsMatch = columnMatch && columnSpanMatch;
			bool alignmentParamsMatch = horizontalAlignmentMatch && verticalAlignmentMatch;

			return rowParamsMatch && columnParamsMatch && alignmentParamsMatch;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Row ^ Column ^ RowSpan ^ ColumnSpan ^ (int)HorizontalAlignment ^ (int)VerticalAlignment;
		}
	}
}
