﻿namespace Facebook.Client.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Phone.Controls;

    /// <summary>
    /// Displays a list of selectable items with optional multi-selection.
    /// </summary>
    [TemplatePart(Name = PartListSelector, Type = typeof(LongListSelector))]
    public abstract class Picker<T> : Control
        where T : class
    {
        #region Part Definitions

        private const string PartListSelector = "PART_ListSelector";

        #endregion Part Definitions

        #region Default Property Values

        private const PickerSelectionMode DefaultSelectionMode = PickerSelectionMode.Multiple;

        #endregion Default Property Values

        #region Member variables

        private LongListSelector longListSelector;

        #endregion Member variables

        /// <summary>
        /// Initializes a new instance of the Picker class.
        /// </summary>
        public Picker()
        {
            this.SetValue(ItemsProperty, new ObservableCollection<T>());
            this.SetValue(SelectedItemsProperty, new ObservableCollection<T>());
        }

        #region Events

        /// <summary>
        /// Occurs when the current selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        #endregion Events

        #region Properties

        #region SelectionMode

        /// <summary>
        /// Gets or sets the selection behavior of the control. 
        /// </summary>
        public PickerSelectionMode SelectionMode
        {
            get { return (PickerSelectionMode)GetValue(SelectionModeProperty); }
            set { this.SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectionMode dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(PickerSelectionMode), typeof(Picker<T>), new PropertyMetadata(DefaultSelectionMode, OnSelectionModeProperyChanged));

        private static void OnSelectionModeProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (Picker<T>)d;
            picker.ClearSelection();          
        }

        #endregion SelectionMode

        #region Items

        /// <summary>
        /// Gets the list of currently selected items for the FriendPicker control.
        /// </summary>
        public ObservableCollection<T> Items
        {
            get { return (ObservableCollection<T>)this.GetValue(ItemsProperty); }
            private set { this.SetValue(ItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the Items dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<T>), typeof(Picker<T>), null);

        #endregion Items

        #region SelectedItems

        /// <summary>
        /// Gets the list of currently selected items for the FriendPicker control.
        /// </summary>
        public ObservableCollection<T> SelectedItems
        {
            get { return (ObservableCollection<T>)this.GetValue(SelectedItemsProperty); }
            private set { this.SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItems dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<T>), typeof(Picker<T>), null);

        #endregion SelectedItems

        #endregion Properties

        #region Implementation

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. In simplest 
        /// terms, this means the method is called just before a UI element displays in your app. Override this method to influence the 
        /// default post-template logic of a class. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.longListSelector = GetTemplateChild(Picker<T>.PartListSelector) as LongListSelector;
            if (this.longListSelector != null)
            {
                this.longListSelector.SelectionChanged += this.OnSelectionChanged;

                if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                {
                    this.SetDataSource(this.GetDesignTimeData());
                }
            }
        }

        protected void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionMode == PickerSelectionMode.None)
            {
                return;
            }

            if (this.longListSelector == null)
            {
                return;
            }

            if (this.longListSelector.SelectedItem == null)
            {
                return;
            }

            SelectionChangedEventArgs selectionChangedEventArgs;

            var selectedItem = this.longListSelector.SelectedItem as PickerItem<T>;

            if (this.SelectionMode == PickerSelectionMode.Single)
            {
                var unselectedItem = e.RemovedItems[0] as PickerItem<T>;

                selectedItem.IsSelected = true;
                if (unselectedItem != null)
                {
                    unselectedItem.IsSelected = false;
                    this.SelectedItems.Remove(unselectedItem.Item);
                    selectionChangedEventArgs = new SelectionChangedEventArgs(new object[1] { unselectedItem.Item }, new object[1] { selectedItem.Item });
                }
                else
                {
                    selectionChangedEventArgs = new SelectionChangedEventArgs(new object[0], new object[1] { selectedItem.Item });
                }
                
                this.SelectedItems.Add(selectedItem.Item);
            }
            else
            {
                selectedItem.IsSelected = !selectedItem.IsSelected;

                if (selectedItem.IsSelected)
                {
                    this.SelectedItems.Add(selectedItem.Item);
                    selectionChangedEventArgs = new SelectionChangedEventArgs(new object[0], new object[1] { selectedItem.Item });
                }
                else
                {
                    this.SelectedItems.Remove(selectedItem.Item);
                    selectionChangedEventArgs = new SelectionChangedEventArgs(new object[1] { selectedItem.Item }, new object[0]);
                }

                // Reset selected item to null (no selection)
                this.longListSelector.SelectedItem = null;
            }

            this.SelectionChanged.RaiseEvent(this, selectionChangedEventArgs);
        }

        protected void ClearSelection()
        {
            this.SelectedItems.Clear();

            if (this.longListSelector != null && this.longListSelector.ItemsSource != null)
            {
                (this.longListSelector.ItemsSource as IEnumerable<AlphaKeyGroup<PickerItem<T>>>)
                    .SelectMany(i => i)
                    .Where(f => f.IsSelected)
                    .ToList()
                    .ForEach(i => i.IsSelected = false);

                this.longListSelector.SelectedItem = null;
            }
        }

        protected void SetDataSource(IEnumerable<T> items)
        {
            if (this.longListSelector != null)
            {
                this.longListSelector.ItemsSource = GetData(items);
            }
        }

        protected virtual IEnumerable<T> GetDesignTimeData()
        {
            return null;
        }

        protected abstract IList GetData(IEnumerable<T> items);

        #endregion Implementation
    }
}