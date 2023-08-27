using CommunityToolkit.Mvvm.Input;

using ForceDirectedLib;

using ForceDirectedLibDemo.Tools;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ForceDirectedLibDemo.ViewModel
{
	public class MainWindowViewModel : ViewModelBase
	{
		private const double OneDegree = (Math.PI * 2) * 1 / 360;

		private WriteableBitmap _renderSurface;

		public WriteableBitmap RenderSurface { get => _renderSurface; set { _renderSurface = value; OnPropertyChanged(); } }

		public RelayCommand<EventHandlerEventArgs?> EventHandlerCommand { get; set; }

		public bool DrawBackgroundEnabled { get; set; } = true;

		public double LeftRight { get; private set; }
		public double UpDown { get; private set; }
		public double FrontBack { get; private set; }

		public static List<OrbitBug> _bugs = new();
		private Simulation _simulation;
		private readonly List<Task> _tasks = new();
		private bool _done;

		private void OnEventHandler(EventHandlerEventArgs? eventHandlerEventArgs)
		{
			if (eventHandlerEventArgs == null)
			{
				return;
			}

			switch (eventHandlerEventArgs.EventType)
			{
				case EventTypes.Closing:
					Closing(eventHandlerEventArgs);
					break;
				case EventTypes.SizeChanged:
					SizeChanged(eventHandlerEventArgs);
					break;
				case EventTypes.Loaded:
					Init(eventHandlerEventArgs);
					break;
				case EventTypes.KeyDown:
					if (eventHandlerEventArgs.EventArgs is KeyEventArgs kea)
					{
						switch (kea.Key)
						{
							case Key.Down:
								UpDown += 1;
								break;
							case Key.Up:
								UpDown -= 1;
								break;
							case Key.Right:
								LeftRight += 1;
								break;
							case Key.Left:
								LeftRight -= 1;
								break;
							case Key.PageUp:
								FrontBack += 1;
								break;
							case Key.PageDown:
								FrontBack -= 1;
								break;
							case Key.Space:
								UpDown = 0;
								LeftRight = 0;
								FrontBack = 0;
								break;
						}
					}
					break;

				case EventTypes.MouseWheel:
					if (eventHandlerEventArgs.EventArgs is MouseWheelEventArgs mwea)
					{
						_simulation.MouseWheel(eventHandlerEventArgs.Sender, (mwea.Delta, 0));
					}
					break;
				case EventTypes.MouseMove:
					if (eventHandlerEventArgs.EventArgs is MouseEventArgs mea)
					{
						System.Windows.Point mousePosition = mea.GetPosition((IInputElement)eventHandlerEventArgs.Sender);
						double delta = 0.0;
						_simulation.MouseMove(
							eventHandlerEventArgs.Sender,
							(mousePosition.X, mousePosition.Y, new ForceDirectedLib.Tools.Point((int)mousePosition.X, (int)mousePosition.Y), delta));
					}
					break;
				case EventTypes.MouseUp:
					if (eventHandlerEventArgs.EventArgs is MouseEventArgs mouseUpEvent)
					{
						System.Windows.Point position = mouseUpEvent.GetPosition((IInputElement)eventHandlerEventArgs.Sender);
						_simulation.MouseUp(eventHandlerEventArgs.Sender, position.ToSDPoint());
					}
					break;
				case EventTypes.MouseDown:
					if (eventHandlerEventArgs.EventArgs is MouseEventArgs mouseDownEvent)
					{
						System.Windows.Point position = mouseDownEvent.GetPosition((IInputElement)eventHandlerEventArgs.Sender);
						_simulation.MouseDown(eventHandlerEventArgs.Sender, (position.X, position.Y));
					}
					break;
			}
		}

		private void Init(EventHandlerEventArgs eventHandlerEventArgs)
		{
			_simulation = new Simulation();

			FrontBack = 1;
			ExecuteRotate();
			FrontBack = 0;
			LeftRight = 1;
			//ExecuteRotate();

			_tasks.Add(Task.Run(UpdateThread));
			_tasks.Add(Task.Run(RenderThread));
		}

		private void Closing(EventHandlerEventArgs eventHandlerEventArgs)
		{
			_done = true;
			_simulation.Stop();

			if (!Task.WaitAll(_tasks.ToArray(), 1000))
			{
				EventArgs ee = eventHandlerEventArgs.EventArgs;
			}
		}

		public MainWindowViewModel()
		{
			_renderSurface = default!;
			_simulation = default!;

			EventHandlerCommand = new RelayCommand<EventHandlerEventArgs?>(OnEventHandler, _ => true);

			_bugs.Add(new OrbitBug { Color = new ForceDirectedLib.Tools.Color(0xffff0000), MultX = 2, MultY = 3, Offset = 2 * Math.PI * 0.33 });
			_bugs.Add(new OrbitBug { Color = new ForceDirectedLib.Tools.Color(0xffffff00), MultX = 5, MultY = 7, Offset = -2 * Math.PI * 0.33 });
			_bugs.Add(new OrbitBug { Color = new ForceDirectedLib.Tools.Color(0xff0000ff), MultX = 11, MultY = 13, Offset = 0.00 });
		}

		private void SizeChanged(EventHandlerEventArgs? args)
		{
			if (args != null)
			{
				if (args.EventArgs is SizeChangedEventArgs scea)
				{
					RenderSurface = new WriteableBitmap((int)scea.NewSize.Width, (int)scea.NewSize.Height, 96.0, 96.0, PixelFormats.Pbgra32, null);
					RenderSurface.Clear(Colors.Black);

					// needed?
					OnPropertyChanged(nameof(RenderSurface));
				}
			}
		}

		private void UpdateThread()
		{
			while (!_done)
			{
				if (UpDown != 0 || LeftRight != 0 || FrontBack != 0)
				{
					ExecuteRotate();
				}

				Thread.Sleep(1000 / 30);
			}
		}

		private void ExecuteRotate()
		{
			var direction = new Lattice.Vector(UpDown, LeftRight, FrontBack);
			double m = direction.Magnitude();
			_simulation.Rotate(new Lattice.Vector(0, 0, 0), direction, m * OneDegree);
		}

		private void RenderThread()
		{
			int tick = 0;

			while (!_done)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					WriteableBitmap rs = RenderSurface;

					if (rs != null)
					{
						using (rs.GetBitmapContext())
						{
							RenderSurface.Clear(Colors.Black);

							//FadeBackground(rs, 0x04);
							//Draw(rs);

							_simulation.OnPaint(this, new Graphics(rs));

							//DrawOrbitBugs(rs);
						}
					}
				});

				Thread.Sleep(1000 / 120);
				tick++;
			}
		}

		public static void FadeBackground(WriteableBitmap wb, byte FadeDelta)
		{
			int PixelCount = wb.PixelWidth * wb.PixelHeight;
			var DirtyRect = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);

			//using (RenderSurface.GetBitmapContext())???
			wb.Lock();

			unsafe
			{
				if (FadeDelta == 0xff)
				{
					//wipe background clean (black)
					ManagedMemoryTools.MemorySet32Dyn(wb.BackBuffer, unchecked((int)0xff000000), PixelCount);
					// or
					//RenderSurface.Clear(Colors.Black);
				}
				else
				{
					//produces tail affect by fading the entire render bitmap over time
					float nMultiplier = 1.0F - (FadeDelta / (float)0xFF);
					//ManagedMemoryTools.FadeMulDYN(wb.BackBuffer, PixelCount, nMultiplier);
					ManagedMemoryTools.FadeMul(wb.BackBuffer, PixelCount, FadeDelta);
				}

				//draw here!
			}

			wb.AddDirtyRect(DirtyRect);
			wb.Unlock();
		}

		private static void DrawOrbitBugs(WriteableBitmap rs)
		{
			foreach (OrbitBug bug in _bugs)
			{
				double xc = rs.Width / 2;
				double yc = rs.Height / 2;

				double rx = xc * 0.75;
				double ry = yc * 0.75;

				double angle = Environment.TickCount / 4000.0;
				double xo = Math.Cos((angle + bug.Offset) * bug.MultX) * rx;
				double yo = Math.Sin((angle + bug.Offset) * bug.MultY) * ry;

				double rr = 2.50;

				ForceDirectedLib.Tools.Color color = bug.Color;

				rs.FillEllipse((int)(xc + xo - rr), (int)(yc + yo - rr), (int)(xc + xo + rr), (int)(yc + yo + rr), color.ToArgb());
			}
		}

		public class OrbitBug
		{
			public int MultX = 1;
			public int MultY = 1;
			public double Offset = 0.0;
			public ForceDirectedLib.Tools.Color Color = new(0xffff7f00);
		}
	}
}