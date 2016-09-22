using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_API
{
	class Disp
	{

		#region Print Functions
		/// <summary>
		/// Print an object to the screen
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		public static void print(object thing)
		{
			Console.Write(thing);
		}

		/// <summary>
		/// Print an object to the screen at (1, y)
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		public static void print(object thing, int y)
		{
			print(thing, 1, y);
		}

		/// <summary>
		/// Print an object to the screen at (x, y)
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		public static void print(object thing, int x, int y)
		{
			int _x = Console.CursorLeft;
			int _y = Console.CursorTop;
			Console.SetCursorPosition(x, y);
			print(thing);
			Console.SetCursorPosition(_x, _y);
		}

		/// <summary>
		/// Print an object to the screen at (x, y), with the specified text colour
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The color of the text</param>
		public static void print(object thing, int x, int y, ConsoleColor textColor)
		{
			ConsoleColor original = Console.ForegroundColor;
			Console.ForegroundColor = textColor;
			print(thing, x, y);
			Console.ForegroundColor = original;
		}

		/// <summary>
		/// Print an object to the screen at (x, y), with the specified text colour and background colour
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The color of the text</param>
		/// <param name="backGround">The color of the background</param>
		public static void print(object thing, int x, int y, ConsoleColor textColor, ConsoleColor backGround)
		{
			ConsoleColor originalBack = Console.BackgroundColor;
			Console.BackgroundColor = backGround;
			print(thing, x, y, textColor);
			Console.BackgroundColor = originalBack;
		}
		#endregion

		#region printRight Functions
		/// <summary>
		/// Prints the object to the right side of the screen
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		public static void printRight(object thing, int y)
		{
			int x = Console.BufferWidth - thing.ToString().Length;
			print(thing, x, y);
		}

		/// <summary>
		/// Prints the object to the right side of the screen with the specified text color
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The text color</param>
		public static void printRight(object thing, int y, ConsoleColor textColor)
		{
			int x = Console.BufferWidth - thing.ToString().Length;
			print(thing, x, y, textColor);
		}

		/// <summary>
		/// Prints the object to the right side of the screen with the specified text color and background color
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The text color</param>
		/// <param name="backGround">The background color</param>
		public static void printRight(object thing, int y, ConsoleColor textColor, ConsoleColor backGround)
		{
			int x = Console.BufferWidth - thing.ToString().Length;
			print(thing, x, y, textColor, backGround);
		}
		#endregion

		#region printCentre
		/// <summary>
		/// Prints the object to the centre screen
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		public static void printCentre(object thing, int y)
		{
			int x = (Console.BufferWidth - thing.ToString().Length)/2;
			print(thing, x, y);
		}

		/// <summary>
		/// Prints the object to the centre of the screen with the specified text color
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The text color</param>
		public static void printCentre(object thing, int y, ConsoleColor textColor)
		{
			int x = Console.BufferWidth - thing.ToString().Length;
			print(thing, x/2, y, textColor);
		}

		/// <summary>
		/// Prints the object to the centre of the screen with the specified text color and background color
		/// </summary>
		/// <param name="thing">The object to be printed</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="textColor">The text color</param>
		/// <param name="backGround">The background color</param>
		public static void printCentre(object thing, int y, ConsoleColor textColor, ConsoleColor backGround)
		{
			int x = Console.BufferWidth - thing.ToString().Length;
			print(thing, x/2, y, textColor, backGround);
		}
		#endregion

	}
}
