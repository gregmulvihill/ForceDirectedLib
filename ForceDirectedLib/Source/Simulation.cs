using ForceDirectedLib.Tools;
using Lattice;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ForceDirectedLib
{
    public class Simulation
	{
		/// <summary>
		/// The brush for drawing the info text.
		/// </summary>
		private static readonly Color InfoBrush = new Color(Color.FromArgb(200, new Color(0xffffffff)));

		/// <summary>
		/// The font for drawing the info text.
		/// </summary>
		private static readonly Font InfoFont = new Font("Lucida Console", 8);

		/// <summary>
		/// The distance from the right border to align the info text.
		/// </summary>
		private const int InfoWidth = 160;

		/// <summary>
		/// The distance from the top border and between info text lines.
		/// </summary>
		private const int InfoHeight = 14;

		/// <summary>
		/// The distance from the top border of the topmost info text line.
		/// </summary>
		private const int InfoHeightInitial = -3;

		/// <summary>
		/// The multiplicative factor that gives the rate the update FPS converges.
		/// </summary>
		private const double UpdateFpsEasing = 0.2;

		/// <summary>
		/// The multiplicative factor that gives the rate the update FPS converges.
		/// </summary>
		private const double DrawFpsEasing = 0.2;

		/// <summary>
		/// The maximum update FPS displayed.
		/// </summary>
		private const double UpdateFpsMax = 999.9;

		/// <summary>
		/// The maximum draw FPS displayed.
		/// </summary>
		private const double DrawFpsMax = 999.9;

		/// <summary>
		/// The target number of milliseconds between model updates.
		/// </summary>
		private const int UpdateInterval = 20;

		/// <summary>
		/// The target number of milliseconds between window drawing.
		/// </summary>
		private const int DrawInterval = 33;

		/// <summary>
		/// The model of nodes and edges.
		/// </summary>
		private readonly World _model = new World();

		/// <summary>
		/// The timer used to measure the drawing FPS.
		/// </summary>
		private readonly Stopwatch _drawTimer = new Stopwatch();

		/// <summary>
		/// The update FPS counter.
		/// </summary>
		private double _updateFps = 0;

		/// <summary>
		/// The drawing FPS counter.
		/// </summary>
		private double _drawFps = 0;

		/// <summary>
		/// The second most recent mouse location.
		/// </summary>
		private Point _previousMouseLocation;

		/// <summary>
		/// Whether the mouse is undergoing a drag gesture.
		/// </summary>
		private bool _drag = false;
		private bool _done;
		private readonly List<Task> _tasks = new List<Task>();

		public Action<object, IGraphics> OnPaint { get; }

		//public int ClientSizeWidth { get; private set; }
		//public int ClientSizeHeight { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public Action<object, (double, double)> MouseDown { get; private set; }
		public Func<object, Point, bool> MouseUp { get; private set; }
		public Action<object, (double X, double Y, Point Location, double Delta)> MouseMove { get; private set; }
		public Action<object, (double Delta, double Value)> MouseWheel { get; private set; }

		/// <summary>
		/// Constructs and initializes the main window.
		/// </summary>
		public Simulation()
		{
			//InitializeComponent();
			InitializeMouseEvents();

			//DoubleBuffered = true;
			OnPaint += Draw;

			_tasks.Add(Task.Run(() => RenderThread()));
			_tasks.Add(Task.Run(() => UpdateThread()));
			_tasks.Add(Task.Run(() => _model.StartGeneration()));
		}

		/// <summary>
		/// Draws the window and model.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The paint event data.</param>
		private void Draw(object sender, IGraphics g)
		{
			if (g == null)
			{
				return;
			}

			//g.SmoothingMode = SmoothingMode.AntiAlias;

			// Draw model.
			g.TranslateTransform(g.Width / 2, g.Height / 2);
			_model.Draw(g);
			g.ResetTransform();

			// Draw info text.
			int x = Width - InfoWidth;
			int y = InfoHeightInitial;
			g.DrawString(String.Format("{0,-9}{1:#0.0}", "Model", _updateFps), InfoFont, InfoBrush, x, y += InfoHeight);
			g.DrawString(String.Format("{0,-9}{1:#0.0}", "Render", _drawFps), InfoFont, InfoBrush, x, y += InfoHeight);
			g.DrawString(String.Format("{0,-9}{1}", "Nodes", _model.NodeCount), InfoFont, InfoBrush, x, y += InfoHeight);
			g.DrawString(String.Format("{0,-9}{1}", "Edges", _model.EdgeCount), InfoFont, InfoBrush, x, y += InfoHeight);
			g.DrawString(String.Format("{0,-9}{1}", "Frames", _model.Frames), InfoFont, InfoBrush, x, y += InfoHeight);

			g.DrawString("ZONG ZHENG LI", InfoFont, InfoBrush, x, Height - 60);

			// Fps stuff.
			_drawTimer.Stop();
			_drawFps += ((1000.0 / _drawTimer.Elapsed.TotalMilliseconds) - _drawFps) * DrawFpsEasing;
			_drawFps = Math.Min(_drawFps, DrawFpsMax);
			_drawTimer.Reset();
			_drawTimer.Start();
		}

		/// <summary>
		/// Initializes mouse behavior.
		/// </summary>
		private void InitializeMouseEvents()
		{
			// Initialize mouse down behavior.
			MouseDown += (sender, e) =>
			{
				_previousMouseLocation = new Point((int)e.Item1, (int)e.Item2);
				_drag = true;

				_model.StopCamera();
			};

			// Initialize mouse up behavior.
			MouseUp += (sender, e) => _drag = false;

			// Initialize mouse move behavior.
			MouseMove += (sender, e) =>
			{
				int dx = (int)(e.X - (_previousMouseLocation == null ? 0 : _previousMouseLocation.X));
				int dy = (int)(e.Y - (_previousMouseLocation == null ? 0 : _previousMouseLocation.Y));

				if (_drag)
				{
					RotationHelper.MouseDrag(_model.Rotate, dx, dy);
				}

				_previousMouseLocation = e.Location;
			};

			// Initialize mouse wheel behavior.
			MouseWheel += (sender, e) => _model.MoveCamera((int)e.Delta);
		}

		/// <summary>
		/// Starts the draw thread.
		/// </summary>
		private void RenderThread()
		{
			while (!_done)
			{
				Invalidate();
				Thread.Sleep(DrawInterval);
			}
		}

		private void Invalidate()
		{
			//throw new NotImplementedException();
		}

		/// <summary>
		/// Starts the update thread.
		/// </summary>
		private void UpdateThread()
		{
			var timer = new Stopwatch();

			while (!_done)
			{
				// Update the model.
				timer.Start();
				_model.Update();

				// Sleep for appropriate duration.
				int elapsed = (int)timer.ElapsedMilliseconds;

				if (elapsed < UpdateInterval)
				{
					Thread.Sleep(UpdateInterval - elapsed);
				}

				// Fps stuff.
				timer.Stop();
				_updateFps += ((1000.0 / timer.Elapsed.TotalMilliseconds) - _updateFps) * UpdateFpsEasing;
				_updateFps = Math.Min(_updateFps, UpdateFpsMax);
				timer.Reset();
			}
		}

		public void Rotate(Vector point, Vector direction, double angle)
		{
			_model.Rotate(point, direction, angle);
		}

		public void Stop()
		{
			_done = true;

			_model.Stop();

			if (!Task.WaitAll(_tasks.ToArray(), 1000))
			{
				throw new NotImplementedException();
			}
		}
	}
}
