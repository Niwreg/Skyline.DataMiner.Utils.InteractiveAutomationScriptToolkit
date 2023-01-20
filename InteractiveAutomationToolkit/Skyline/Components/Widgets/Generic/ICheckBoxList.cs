namespace Skyline.DataMiner.Utils.InteractiveAutomationToolkit
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	///     Represents a list of checkboxes.
	/// </summary>
	/// <typeparam name="T">The type of the value of the options.</typeparam>
	public interface ICheckBoxList<T> : IInteractiveWidget, IValidate
	{
		/// <summary>
		///     Triggered when the state of a checkbox changes.
		///     WantsOnChange will be set to true when this event is subscribed to.
		/// </summary>
		event EventHandler<CheckBoxList<T>.ChangedEventArgs> Changed;

		/// <summary>
		///     Gets all checked options.
		/// </summary>
		ICollection<Option<T>> CheckedOptions { get; }

		/// <summary>
		///     Gets all options.
		/// </summary>
		IList<Option<T>> Options { get; }

		/// <summary>
		///     Gets or sets a value indicating whether the options are sorted naturally.
		/// </summary>
		/// <remarks>Available from DataMiner 9.5.6 onwards.</remarks>
		bool IsSorted { get; set; }

		/// <summary>
		///     Gets or sets the tooltip.
		/// </summary>
		string Tooltip { get; set; }
	}
}