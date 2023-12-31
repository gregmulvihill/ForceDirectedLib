﻿using ForceDirectedLib.Tools;
using Lattice;

using System;
using System.Collections.Generic;
//using System.Drawing;

namespace ForceDirectedLib
{
    /// <summary>
    /// Represents a node in the graph.
    /// </summary>
    public class Node
	{
		/// <summary>
		/// The multiplicative factor that gives the rate at which velocity is
		/// dampened after each frame.
		/// </summary>
		private const double VelocityDampening = 0.4;

		/// <summary>
		/// The expected maximum radius. A random location is generated with each
		/// coordinate in the interval [0, RadiusRange).
		/// </summary>
		private const double RadiusRange = 1000;

		/// <summary>
		/// The text format used for drawing labels.
		/// </summary>
		//private static readonly StringFormat LabelFormat = new StringFormat()
		//{
		//	LineAlignment = StringAlignment.Center
		//};

		/// <summary>
		/// The collection of brushes for drawing labels. LabelBrush[x] gives the
		/// appropriate brush for drawing at alpha level x where x is an 8-bit
		/// integer.
		/// </summary>
		private static readonly Color[] LabelBrush = new Color[256];

		/// <summary>
		/// The collection of fonts for drawing labels. LabelFont[s] gives the
		/// appropriate font of size s where s is a floating point value rounded with
		/// exactly one decimal.
		/// </summary>
		private static readonly Dictionary<double, Font> LabelFont = new Dictionary<double, Font>();

		/// <summary>
		/// The multiplicative factor for label opacity.
		/// </summary>
		private const double LabelOpacity = 0.5;

		/// <summary>
		/// The intercept in the label opacity equation.
		/// </summary>
		private const double LabelOpacityIntercept = 1.5;

		/// <summary>
		/// The distance from the node for drawing labels.
		/// </summary>
		private const int LabelOffset = 5;

		/// <summary>
		/// Returns the radius defined for the given mass value.
		/// </summary>
		/// <param name="mass">The mass to calculate a radius for.</param>
		/// <returns>The radius defined for the given mass value.</returns>
		public static double GetRadius(double mass)
		{
			return 2.8 * Math.Pow(mass, 1.0 / 3.0);
		}

		/// <summary>
		/// The collection of nodes the node is connected to.
		/// </summary>
		public HashSet<Node> Connected;

		/// <summary>
		/// The location of the node.
		/// </summary>
		public Vector Location = Vector.Zero;

		/// <summary>
		/// The velocity of the node.
		/// </summary>
		public Vector Velocity = Vector.Zero;

		/// <summary>
		/// The acceleration applied to the node.
		/// </summary>
		public Vector Acceleration = Vector.Zero;

		/// <summary>
		/// The mass of the node.
		/// </summary>
		public double Mass => (Connected.Count > 0 ? Connected.Count : 1) + 5;

		/// <summary>
		/// The radius of the node.
		/// </summary>
		public double Radius => GetRadius(Mass);

		/// <summary>
		/// The text label of the node.
		/// </summary>
		public string Label
		{
			get;
			set;
		}

		/// <summary>
		/// The Size of the node.
		/// </summary>
		public Color Colour
		{
			get
			{
				return _colour;
			}
			set
			{
				_colour = value;
				_brush = new Color(_colour);
			}
		}

		public bool LockLocation { get; internal set; }

		private Color _colour = new Color(0xff000000);

		/// <summary>
		/// The brush used to draw the node.
		/// </summary>
		private Color _brush;

		/// <summary>
		/// Constructs a node with the given colour.
		/// </summary>
		/// <param name="colour">The colour of the node.</param>
		public Node(Color colour)
			 : this(null, colour, PseudoRandom.Vector(RadiusRange)) { }

		/// <summary>
		/// Constructs a node with the given label and colour.
		/// </summary>
		/// <param name="label">The label of the node.</param>
		/// <param name="colour">The colour of the node.</param>
		public Node(string label, Color colour)
			 : this(label, colour, PseudoRandom.Vector(RadiusRange)) { }

		/// <summary>
		/// Constructs a node with the given label, colour, and location.
		/// </summary>
		/// <param name="label">The label of the node.</param>
		/// <param name="colour">The colour of the node.</param>
		/// <param name="location">The starting location of the node.</param>
		public Node(string label, Color colour, Vector location)
		{
			Label = label;
			Colour = colour;
			Location = location;
			Velocity = Vector.Zero;
			Acceleration = Vector.Zero;
			Connected = new HashSet<Node>();
		}

		/// <summary>
		/// Returns whether the node is connected to the given node.
		/// </summary>
		/// <param name="other">A potentially connected node.</param>
		/// <returns>Whether the node is connected to the given nod</returns>
		public bool IsConnectedTo(Node other)
		{
			return Connected.Contains(other);
		}

		/// <summary>
		/// Updates the properties of the node such as location, velocity, and
		/// applied acceleration. This method should be invoked at each time step.
		/// </summary>
		public void Update()
		{
			if (!LockLocation)
			{
				Velocity += Acceleration;
				Location += Velocity;
			}
			else
			{
			}

			Velocity *= VelocityDampening;
			Acceleration = Vector.Zero;
		}

		/// <summary>
		/// Draws the node.
		/// </summary>
		/// <param name="renderer">The 3D renderer.</param>
		/// <param name="g">The graphics surface.</param>
		/// <param name="showLabels">Whether to draw the label.</param>
		public void Draw(Renderer renderer, IGraphics g, bool showLabels = true)
		{
			double radius = Radius;

			renderer.FillCircle2D(g, Color.FromArgb(unchecked((int)0xff555555)), Location, radius + 1);

			// Draw the node.
			if (renderer.FillCircle2D(g, _brush, Location, radius))
			{
				// The node is drawn successfully, so it visible. We try to draw the label
				// as well.
				if (showLabels && Label != null)
				{
					// Determine size and opacity.
					double ratio = renderer.ComputeScale(Location);
					int radiusOffset = (int)Math.Round(radius * ratio);

					double opacity = LabelOpacityIntercept - (Location.To(renderer.Camera).Magnitude() / 1000.0);
					opacity = Math.Min(Math.Max(opacity, 0), 1);
					opacity *= LabelOpacity;
					int alpha = (int)Math.Round(255 * opacity);

					// Determine if label is visible.
					if (alpha > 1 && ratio > 0)
					{
						// Initialize label brush if it has not been for the current alpha level.
						if (LabelBrush[alpha] == null)
						{
							LabelBrush[alpha] = new Color(Color.FromArgb(alpha, new Color(0xffffffff)));
						}

						// Initialize label font if it has not been for the current size.
						double size = Math.Round(ratio, 1);
						if (!LabelFont.ContainsKey(size))
						{
							LabelFont.Add(size, new Font("Lucida Console", (float)size));
						}

						// Determine screen location.
						Point point = renderer.ComputePoint(Location);
						point.Offset(radiusOffset + LabelOffset, 0);

						g.DrawString(Label, LabelFont[size], LabelBrush[alpha], point/*, LabelFormat*/);
					}
				}
			}
		}

		/// <summary>
		/// Rotates the node along an arbitrary axis.
		/// </summary>
		/// <param name="point">The starting point for the axis of rotation.</param>
		/// <param name="direction">The direction for the axis of rotation</param>
		/// <param name="angle">The angle to rotate by.</param>
		public void Rotate(Vector point, Vector direction, double angle)
		{
			Location = Location.Rotate(point, direction, angle);

			// To rotate velocity and acceleration we have to adjust for the starting
			// point for the axis of rotation. This way the vectors are effectively
			// rotated about their own starting points.
			Velocity += point;
			Velocity = Velocity.Rotate(point, direction, angle);
			Velocity -= point;
			Acceleration += point;
			Acceleration = Acceleration.Rotate(point, direction, angle);
			Acceleration -= point;
		}
	}
}
