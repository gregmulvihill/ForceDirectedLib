using ForceDirectedLib.Tools;
using Lattice;

using System;
using System.Collections.Generic;
using System.Linq;
//using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ForceDirectedLib
{
    /// <summary>
    /// Represents a world model of nodes and edges.
    /// </summary>
    public class World
	{
		/// <summary>
		/// The multiplicative factor for origin attraction of nodes.
		/// </summary>
		public const double OriginFactor = 2e4;

		/// <summary>
		/// The distance softening factor for origin attraction of nodes.
		/// </summary>
		public const double OriginEpsilon = 7000;

		/// <summary>
		/// The distance within which origin attraction of nodes becomes weaker.
		/// </summary>
		public const double OriginWeakDistance = 100;

		/// <summary>
		/// The multiplicative factor for repulsion between nodes.
		/// </summary>
		public const double RepulsionFactor = -900;

		/// <summary>
		/// The distance softening factor for repulsion between nodes.
		/// </summary>
		public const double RepulsionEpsilon = 2;

		/// <summary>
		/// The multiplicative factor for edge spring stiffness.
		/// </summary>
		public const double EdgeFactor = 0.1;

		/// <summary>
		/// The ideal length of edges.
		/// </summary>
		public const double EdgeLength = 30;

		/// <summary>
		/// The multiplicative factor for camera acceleration.
		/// </summary>
		private const double CameraZAcceleration = -2e-4;

		/// <summary>
		/// The multiplicative factor that gives the rate camera acceleration is
		/// dampened.
		/// </summary>
		private const double CameraZEasing = 0.94;

		/// <summary>
		/// The target number of milliseconds between node generations.
		/// </summary>
		private const int GenerationInterval = 2500;

		/// <summary>
		/// The number of nodes in the world model.
		/// </summary>
		public int NodeCount => _nodes.Count;

		/// <summary>
		/// The number of edges in the world model.
		/// </summary>
		public int EdgeCount => _edges.Count;

		/// <summary>
		/// The number of frames elapsed.
		/// </summary>
		public long Frames
		{
			get;
			private set;
		}

		/// <summary>
		/// The 3D renderer.
		/// </summary>
		private readonly Renderer _renderer = new Renderer()
		{
			Camera = new Vector(0, 0, 2000),
			FOV = 1400
		};

		/// <summary>
		/// The collection of nodes in the world model.
		/// </summary>
		private readonly List<Node> _nodes = new List<Node>();

		/// <summary>
		/// The collection of edges in the world model.
		/// </summary>
		private readonly List<Edge> _edges = new List<Edge>();

		/// <summary>
		/// The lock required to modify the nodes collection.
		/// </summary>
		private readonly object _nodeLock = new object();

		/// <summary>
		/// The camera's position on the z-axis.
		/// </summary>
		private double _cameraZ = 5000;

		/// <summary>
		/// The camera's velocity along the z-axis.
		/// </summary>
		private double _cameraZVelocity = 0;
		private readonly List<Task> _tasks = new List<Task>();
		private bool _done;

		/// <summary>
		/// Constructs a world model.
		/// </summary>
		public World()
		{
			Frames = 0;
		}

		/// <summary>
		/// Adds a node to the world model.
		/// </summary>
		/// <param name="node">The node to add to the world model.</param>
		public void Add(Node node)
		{
			lock (_nodeLock)
			{
				_nodes.Add(node);
			}
		}

		/// <summary>
		/// Adds a collection of nodes to the world model.
		/// </summary>
		/// <param name="nodes">The collection of nodes to add to the world model.</param>
		public void AddRange(IEnumerable<Node> nodes)
		{
			lock (_nodeLock)
			{
				_nodes.AddRange(nodes);
			}
		}

		/// <summary>
		/// Connects two nodes in the world model.
		/// </summary>
		/// <param name="a">A node to connect.</param>
		/// <param name="b">A node to connect.</param>
		public void Connect(Node a, Node b)
		{
			if (a == b)
			{
				throw new ArgumentException("Cannot connect a node to itself.");
			}

			lock (_nodeLock)
			{
				a.Connected.Add(b);
				b.Connected.Add(a);
				_edges.Add(new Edge(a, b));
			}
		}

		/// <summary>
		/// Advances the world model by one frame.
		/// </summary>
		public void Update()
		{
			// Update nodes.
			lock (_nodeLock)
			{
				// Update the nodes and determine required tree width.
				double halfWidth = 0;

				foreach (Node node in _nodes)
				{
					node.Update();
					halfWidth = Math.Max(Math.Abs(node.Location.X), halfWidth);
					halfWidth = Math.Max(Math.Abs(node.Location.Y), halfWidth);
					halfWidth = Math.Max(Math.Abs(node.Location.Z), halfWidth);
				}

				// Build tree for node repulsion.
				var tree = new Octree(2.1 * halfWidth);

				foreach (Node node in _nodes)
				{
					tree.Add(node);
				}

				Parallel.ForEach(_nodes, node =>
				{
					// Apply repulsion between nodes.
					tree.Accelerate(node);

					// Apply origin attraction of nodes.
					Vector originDisplacementUnit = -node.Location.Unit();
					double originDistance = node.Location.Magnitude();

					double attractionCofficient = OriginFactor;

					if (originDistance < OriginWeakDistance)
					{
						attractionCofficient *= originDistance / OriginWeakDistance;
					}

					node.Acceleration += originDisplacementUnit * attractionCofficient / (originDistance + OriginEpsilon);

					// Apply edge spring forces.
					foreach (Node other in node.Connected)
					{
						Vector displacement = node.Location.To(other.Location);
						Vector direction = displacement.Unit();
						double distance = displacement.Magnitude();
						double idealLength = EdgeLength + node.Radius + other.Radius;

						node.Acceleration += direction * EdgeFactor * (distance - idealLength) / node.Mass;
					}
				});

				// Update frame info.
				if (_nodes.Count > 0)
				{
					Frames++;
				}
			}

			// Update camera.
			_cameraZ += _cameraZVelocity * _cameraZ;
			_cameraZ = Math.Max(1, _cameraZ);
			_cameraZVelocity *= CameraZEasing;
			_renderer.Camera.Z = _cameraZ;
		}

		/// <summary>
		/// Rotates the world model along an arbitrary axis.
		/// </summary>
		/// <param name="point">The starting point for the axis of rotation.</param>
		/// <param name="direction">The direction for the axis of rotation.</param>
		/// <param name="angle">The angle to rotate by.</param>
		public void Rotate(Vector point, Vector direction, double angle)
		{
			lock (_nodeLock)
			{
				Parallel.ForEach(_nodes, node => node.Rotate(point, direction, angle));
			}
		}

		/// <summary>
		/// Moves the camera in association with the given mouse wheel delta.
		/// </summary>
		/// <param name="delta">The signed number of dents the mouse wheel moved.</param>
		public void MoveCamera(int delta)
		{
			_cameraZVelocity += delta * CameraZAcceleration;
		}

		/// <summary>
		/// Stops the camera if it is moving.
		/// </summary>
		public void StopCamera()
		{
			_cameraZVelocity = 0;
		}

		/// <summary>
		/// Draws the world model.
		/// </summary>
		/// <param name="g">The graphics surface.</param>
		/// <param name="showLabels">Whether to draw node labels.</param>
		public void Draw(IGraphics g, bool showLabels = true)
		{
			// Draw edges.
			int edgeCount = _edges.Count;
			for (int i = 0; i < edgeCount; i++)
			{
				Edge edge = _edges[i];
				edge?.Draw(_renderer, g);
			}

			// Draw nodes.
			Node[] n = _nodes.OrderBy(x => x.Location.Z).ToArray();

			int nodeCount = n.Length;// _nodes.Count;
			for (int i = 0; i < nodeCount; i++)
			{
				n[i]?.Draw(_renderer, g, showLabels);
			}
		}

		/// <summary>
		/// Generates nodes and edges endlessly for demonstration.
		/// </summary>
		public void StartGeneration()
		{
			lock (_nodeLock)
			{
				double d = 1000.0;
				double dd = 500.0;

				int inputCount = 14;
				int outputCount = 3;

				// add stationary input nodes
				for (int i = 0; i < inputCount; i++)
				{
					double offset = (i * d / (inputCount - 1)) - (d / 2);
					Add(CreateInputOutputNode($"Input {i + 1}", true, new Vector(offset, dd, 0.0), new Color(0x7fff00ff)));
				}

				// add stationary output nodes
				for (int i = 0; i < outputCount; i++)
				{
					double offset = (i * d / (outputCount - 1)) - (d / 2);
					Add(CreateInputOutputNode($"Output {i + 1}", true, new Vector(offset, -dd, 0.0), new Color(0xffff7f3f)));
				}

				if (System.Environment.TickCount == -1)
				{
					for (int j = 0; j < 4; j++)
					{
						for (int i = 4; i < 7; i++)
						{
							Connect(_nodes[j], _nodes[i]);
						}
					}
				}

				//while (ConnectRandomNodes(out Node a, out Node b)) { }

				var colour = new Color(0xff003f3f);

				if (System.Environment.TickCount == -1)
				{
					// Add basis nodes.
					for (int i = 0; i < 10; i++)
					{
						Add(new Node(PseudoRandom.Int32().ToString(), colour));
					}

					// Connect some basis nodes.
					for (int i = 0; i < 8; i++)
					{
						ConnectRandomNodes(out Node a, out Node b);
					}

					// Add group nodes.
					for (int i = 0; i < 20; i++)
					{
						var node = new Node(PseudoRandom.Int32().ToString(), colour);
						Connect(node, _nodes[PseudoRandom.Int32(10)]);
						Add(node);
					}

					// Add outlier nodes.
					for (int i = 0; i < 20; i++)
					{
						var node = new Node(PseudoRandom.Int32().ToString(), colour);
						Connect(node, _nodes[PseudoRandom.Int32(_nodes.Count - 1)]);
						Add(node);
					}

					// Connect more nodes.
					for (int i = 0; i < 5; i++)
					{
						ConnectRandomNodes();
					}
				}

				//if (System.Environment.TickCount == -1)
				{
					// Add endless outlier nodes.
					_tasks.Add(Task.Run(() => AddRandomNodeThread(colour)));
					_tasks.Add(Task.Run(() => ConnectRandomNodesThread()));
				}
			}
		}

		public void Stop()
		{
			_done = true;

			if (!Task.WaitAll(_tasks.ToArray(), 3000))
			{
				throw new NotImplementedException();
			}
		}

		private void ConnectRandomNodesThread()
		{
			while (!_done)
			{
				ConnectRandomNodes();
				Thread.Sleep(1000);
			}
		}

		private void AddRandomNodeThread(Color colour)
		{
			while (!_done)
			{
				var node = new Node(PseudoRandom.Int32().ToString(), colour);
				Connect(node, _nodes[PseudoRandom.Int32(_nodes.Count - 1)]);
				Add(node);
				Thread.Sleep(GenerationInterval);
			}
		}

		private void ConnectRandomNodes()
		{
			Node a, b;

			do
			{
				a = _nodes[PseudoRandom.Int32(_nodes.Count - 1)];
				b = _nodes[PseudoRandom.Int32(_nodes.Count - 1)];
			}
			while (a == b || a.IsConnectedTo(b));

			Connect(a, b);
		}

		private bool ConnectRandomNodes(out Node a, out Node b)
		{
			int count = _nodes.Count * 2;

			do
			{
				a = _nodes[PseudoRandom.Int32(_nodes.Count - 1)];
				b = _nodes[PseudoRandom.Int32(_nodes.Count - 1)];

				if (count-- <= 0)
				{
					return false;
				}
			}
			while (a == b || a.IsConnectedTo(b));

			Connect(a, b);
			return true;
		}

		private Node CreateInputOutputNode(string Label, bool isInputNode, Vector location, Color color)
		{
			var colour0 = Color.FromArgb(0xff, color);
			var n = new Node(Label, colour0);
			n.Location = location;
			n.Acceleration = new Vector();
			n.Label = "my label";
			n.LockLocation = true;
			return n;
		}
	}
}
