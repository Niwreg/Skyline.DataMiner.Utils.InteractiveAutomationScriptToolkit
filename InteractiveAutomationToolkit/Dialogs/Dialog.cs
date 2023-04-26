﻿namespace Skyline.DataMiner.Utils.InteractiveAutomationScript
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;

	/// <inheritdoc />
	public class Dialog<TPanel> : IDialog<TPanel> where TPanel : IPanel, new()
	{
		private const string Auto = "auto";
		private const string Stretch = "*";

		private readonly Dictionary<int, string> columnDefinitions = new Dictionary<int, string>();
		private readonly Dictionary<int, string> rowDefinitions = new Dictionary<int, string>();

		private int height;
		private int maxHeight;
		private int maxWidth;
		private int minHeight;
		private int minWidth;
		private int width;

		/// <summary>
		///     Initializes a new instance of the <see cref="Dialog{TPanel}" /> class.
		/// </summary>
		/// <param name="engine">Allows interaction with the DataMiner System.</param>
		public Dialog(IEngine engine)
		{
			Engine = engine ?? throw new ArgumentNullException(nameof(engine));
			width = -1;
			height = -1;
			MaxHeight = Int32.MaxValue;
			MinHeight = 1;
			MaxWidth = Int32.MaxValue;
			MinWidth = 1;
			Title = null;
			AllowOverlappingWidgets = false;
		}

		/// <inheritdoc />
		public event EventHandler<EventArgs> Back;

		/// <inheritdoc />
		public event EventHandler<EventArgs> Forward;

		/// <inheritdoc />
		public event EventHandler<EventArgs> Interacted;

		/// <inheritdoc/>
		IPanel IDialog.Panel => Panel;

		/// <inheritdoc/>
		public TPanel Panel { get; } = new TPanel();

		/// <inheritdoc />
		public IEngine Engine { get; }

		/// <inheritdoc />
		public bool AllowOverlappingWidgets { get; set; }

		/// <inheritdoc />
		public int Height
		{
			get => height;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				height = value;
			}
		}

		/// <inheritdoc />
		public int MaxHeight
		{
			get => maxHeight;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				maxHeight = value;
			}
		}

		/// <inheritdoc />
		public int MaxWidth
		{
			get => maxWidth;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				maxWidth = value;
			}
		}

		/// <inheritdoc />
		public int MinHeight
		{
			get => minHeight;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				minHeight = value;
			}
		}

		/// <inheritdoc />
		public int MinWidth
		{
			get => minWidth;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				minWidth = value;
			}
		}

		/// <inheritdoc />
		public string Title { get; set; }

		/// <inheritdoc />
		public int Width
		{
			get => width;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				width = value;
			}
		}

		/// <inheritdoc />
		public void SetColumnWidth(int column, int columnWidth)
		{
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(column));
			}

			if (columnWidth < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(columnWidth));
			}

			columnDefinitions[column] = columnWidth.ToString();
		}

		/// <inheritdoc />
		public void SetColumnWidthAuto(int column)
		{
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(column));
			}

			columnDefinitions[column] = Auto;
		}

		/// <inheritdoc />
		public void SetColumnWidthStretch(int column)
		{
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(column));
			}

			columnDefinitions[column] = Stretch;
		}

		/// <inheritdoc />
		public void SetRowHeight(int row, int rowHeight)
		{
			if (row < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(row));
			}

			if (rowHeight < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rowHeight));
			}

			rowDefinitions[row] = rowHeight.ToString();
		}

		/// <inheritdoc />
		public void SetRowHeightAuto(int row)
		{
			if (row < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(row));
			}

			rowDefinitions[row] = Auto;
		}

		/// <inheritdoc />
		public void SetRowHeightStretch(int row)
		{
			if (row < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(row));
			}

			rowDefinitions[row] = Stretch;
		}

		/// <inheritdoc/>
		public void ShowStatic(bool disabled)
		{
			IInteractiveWidget[] widgets = disabled
				? GetEnabledWidgets()
				: Array.Empty<IInteractiveWidget>();

			DisableWidgets(widgets);

			UIBuilder uib = Build();
			uib.RequireResponse = false;

			Engine.ShowUI(uib);
			Engine.KeepAlive();

			EnableWidgets(widgets);
		}

		/// <inheritdoc/>
		public void ShowInteractive()
		{
			UIBuilder uib = Build();
			uib.RequireResponse = true;

			UIResults uir = Engine.ShowUI(uib);
			Engine.KeepAlive();

			LoadChanges(uir);
			RaiseResultEvents(uir);
		}

		internal UIBuilder Build()
		{
			WidgetLocationPair[] visibleWidgetLocationPairs = Panel.GetWidgetLocationPairs()
				.Where(pair => pair.Widget.IsVisible)
				.ToArray();

			if (!AllowOverlappingWidgets)
			{
				CheckIfWidgetsOverlap(visibleWidgetLocationPairs);
			}

			// Initialize UI Builder
			var builder = new UIBuilder
			{
				Height = Height,
				MinHeight = MinHeight,
				Width = Width,
				MinWidth = MinWidth,
				RowDefs = GetRowDefinitions(),
				ColumnDefs = GetColumnDefinitions(),
				Title = Title,
			};

			foreach (WidgetLocationPair widgetLocationPair in visibleWidgetLocationPairs)
			{
				IWidget widget = widgetLocationPair.Widget;
				WidgetLocation location = widgetLocationPair.Location;

				if (widget.Type == UIBlockType.Undefined)
				{
					continue;
				}

				UIBlockDefinition blockDefinition = widget.BlockDefinition;
				blockDefinition.Row = location.Row;
				blockDefinition.RowSpan = location.RowSpan;
				blockDefinition.Column = location.Column;
				blockDefinition.ColumnSpan = location.ColumnSpan;
				builder.AppendBlock(blockDefinition);
			}

			return builder;
		}

		internal void LoadChanges(UIResults uir)
		{
			foreach (InteractiveWidget interactiveWidget in Panel.GetWidgets().OfType<InteractiveWidget>())
			{
				if (interactiveWidget.IsVisible)
				{
					interactiveWidget.LoadResult(uir);
				}
			}
		}

		internal void RaiseResultEvents(UIResults uir)
		{
			Interacted?.Invoke(this, EventArgs.Empty);

			if (uir.WasBack() && Back != null)
			{
				Back(this, EventArgs.Empty);
				return;
			}

			if (uir.WasForward() && Forward != null)
			{
				Forward(this, EventArgs.Empty);
				return;
			}

			// ToList is necessary to prevent InvalidOperationException when adding or removing widgets from a event handler.
			List<InteractiveWidget> intractableWidgets = Panel.GetWidgets()
				.OfType<InteractiveWidget>()
				.Where(widget => widget.WantsOnChange)
				.ToList();

			foreach (InteractiveWidget intractable in intractableWidgets)
			{
				intractable.RaiseResultEvents();
			}
		}

		private static void CheckIfWidgetsOverlap(WidgetLocationPair[] widgetLocationPairs)
		{
			var builder = new OverlappingWidgetsException.Builder();

			for (var i = 0; i < widgetLocationPairs.Length; i++)
			{
				IWidget widget = widgetLocationPairs[i].Widget;
				WidgetLocation location = widgetLocationPairs[i].Location;
				for (int j = i + 1; j < widgetLocationPairs.Length; j++)
				{
					IWidget otherWidget = widgetLocationPairs[j].Widget;
					WidgetLocation otherLocation = widgetLocationPairs[j].Location;
					if (location.Overlaps(otherLocation))
					{
						builder.Add(widget, location, otherWidget, otherLocation);
					}
				}
			}

			if (builder.Count != 0)
			{
				throw builder.Build();
			}
		}

		private static void EnableWidgets(IEnumerable<IInteractiveWidget> widgets)
		{
			foreach (IInteractiveWidget widget in widgets)
			{
				widget.IsEnabled = true;
			}
		}

		private static void DisableWidgets(IEnumerable<IInteractiveWidget> widgets)
		{
			foreach (IInteractiveWidget widget in widgets)
			{
				widget.IsEnabled = false;
			}
		}

		private string GetColumnDefinitions()
		{
			return GetDefinitions(columnDefinitions, Panel.GetColumnCount());
		}

		private string GetDefinitions(Dictionary<int, string> definitions, int amount)
		{
			return String.Join(";", GetDefinitionsEnumerator() ?? Array.Empty<string>());

			// ReSharper disable once RedundantNameQualifier
			// DIS code generation fails to generate this local function if the return type is not fully Qualified
			System.Collections.Generic.IEnumerable<string> GetDefinitionsEnumerator()
			{
				for (var i = 0; i < amount; i++)
				{
					if (definitions.TryGetValue(i, out string s))
					{
						yield return s;
					}
					else
					{
						yield return Auto;
					}
				}
			}
		}

		private string GetRowDefinitions()
		{
			return GetDefinitions(rowDefinitions, Panel.GetRowCount());
		}

		private IInteractiveWidget[] GetEnabledWidgets()
		{
			return Panel.GetWidgets(true)
				.OfType<IInteractiveWidget>()
				.Where(widget => widget.IsEnabled)
				.ToArray();
		}
	}
}