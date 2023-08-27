using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ForceDirectedLibDemo.ViewModel
{
	public class EventHandlerAttachedProperty
	{
		//https://stackoverflow.com/questions/22538814/attached-dependencyproperty-with-enums-act-weird
		//https://www.generacodice.com/en/articolo/1048597/WPFMVVM---how-to-handle-double-click-on-TreeViewItems-in-the-ViewModel

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "DependencyProperty")]
		public static DependencyProperty EventsProperty = DependencyProperty.RegisterAttached(
			"Events",
			typeof(EventTypes),
			typeof(EventHandlerAttachedProperty));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "DependencyProperty")]
		public static DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
			"Command",
			typeof(ICommand),
			typeof(EventHandlerAttachedProperty),
			new UIPropertyMetadata(CommandChanged));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "DependencyProperty")]
		public static DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
			"CommandParameter",
			typeof(object),
			typeof(EventHandlerAttachedProperty));

		public static void SetEvents(DependencyObject target, EventTypes value) => target.SetValue(EventsProperty, value);

		public static EventTypes GetEvents(DependencyObject target) => (EventTypes)target.GetValue(EventsProperty);

		public static void SetCommand(DependencyObject target, ICommand value) => target.SetValue(CommandProperty, value);

		public static void SetCommandParameter(DependencyObject target, object value) => target.SetValue(CommandParameterProperty, value);

		public static object GetCommandParameter(DependencyObject target) => target.GetValue(CommandParameterProperty);

		private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			EventTypes events = GetEvents(target);

			if (target is Window w)
			{
				if (HasFlag(events, EventTypes.Closing))
				{
					w.Closing += Closing;
				}
			}

			if (target is FrameworkElement fe)
			{
				if ((e.NewValue != null) && (e.OldValue == null))
				{
					if (HasFlag(events, EventTypes.MouseMove))
					{
						fe.MouseWheel += MouseWheel;
					}

					if (HasFlag(events, EventTypes.MouseMove))
					{
						fe.MouseMove += MouseMove;
					}

					if (HasFlag(events, EventTypes.MouseDown))
					{
						fe.MouseDown += MouseDown;
					}

					if (HasFlag(events, EventTypes.MouseUp))
					{
						fe.MouseUp += MouseUp;
					}

					if (HasFlag(events, EventTypes.SizeChanged))
					{
						fe.SizeChanged += SizeChanged;
					}

					if (HasFlag(events, EventTypes.Loaded))
					{
						fe.Loaded += Loaded;
					}

					if (HasFlag(events, EventTypes.Unloaded))
					{
						fe.Unloaded += Unloaded;
					}

					if (HasFlag(events, EventTypes.KeyDown))
					{
						fe.KeyDown += KeyDown;
					}
				}
			}
		}

		private static bool HasFlag(EventTypes events, EventTypes match) => (events & match) == match;

		private static void OnEvent(object? sender, EventArgs e, EventTypes et)
		{
			if (sender is DependencyObject o)
			{
				var command = (ICommand)o.GetValue(CommandProperty);
				object commandParameter = o.GetValue(CommandParameterProperty);
				command.Execute(new EventHandlerEventArgs(et, commandParameter, o, e));
			}
		}

		private static void Closing(object? sender, CancelEventArgs e) => OnEvent(sender, e, EventTypes.Closing);

		private static void MouseWheel(object sender, MouseWheelEventArgs e) => OnEvent(sender, e, EventTypes.MouseWheel);

		private static void MouseMove(object sender, MouseEventArgs e) => OnEvent(sender, e, EventTypes.MouseMove);

		private static void MouseDown(object sender, MouseButtonEventArgs e) => OnEvent(sender, e, EventTypes.MouseDown);

		private static void MouseUp(object sender, RoutedEventArgs e) => OnEvent(sender, e, EventTypes.MouseUp);

		private static void KeyDown(object sender, KeyEventArgs e) => OnEvent(sender, e, EventTypes.KeyDown);

		private static void Unloaded(object sender, RoutedEventArgs e) => OnEvent(sender, e, EventTypes.Unloaded);

		private static void Loaded(object sender, RoutedEventArgs e) => OnEvent(sender, e, EventTypes.Loaded);

		private static void SizeChanged(object sender, SizeChangedEventArgs e) => OnEvent(sender, e, EventTypes.SizeChanged);
	}

	public class EventHandlerEventArgs : EventArgs
	{
		public EventHandlerEventArgs(EventTypes loaded, object commandParameter, object sender, EventArgs e)
		{
			EventType = loaded;
			CommandParameter = commandParameter;
			Sender = sender;
			EventArgs = e;
		}

		public EventTypes EventType { get; set; }
		public object Sender { get; set; }
		public object CommandParameter { get; set; }
		public EventArgs EventArgs { get; set; }
	}

	[Flags]
	public enum EventTypes
	{
		None = 0,
		Closing = 1,
		SizeChanged = 2,
		Loaded = 4,
		Unloaded = 8,
		MouseUp = 16,
		MouseDown = 32,
		MouseMove = 64,
		MouseWheel = 128,
		KeyDown = 256,
	}
}
