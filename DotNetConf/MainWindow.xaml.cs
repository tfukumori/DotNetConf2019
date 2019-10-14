﻿using System.Windows;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;

namespace DotNetConf
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private static PaperTrail _paper;
		private Operation _lastOper;
		private string _lastVal;
		private string _memVal;

		public MainWindow()
		{
			InitializeComponent();
            AppCenter.Start("9a2d560a-7da8-4996-97c6-c781f7522e66",
                   typeof(Analytics), typeof(Crashes));
            _paper = new PaperTrail(this);
			ProcessKey('0');
			EraseDisplay = true;
		}

		/// <summary>
		///     Flag to erase or just add to current display flag
		/// </summary>
		private bool EraseDisplay { get; set; }

		/// <summary>
		///     Get/Set Memory cell value
		/// </summary>
		private double Memory
		{
			get
			{
				if (_memVal == string.Empty)
					return 0.0;
				return Convert.ToDouble(_memVal);
			}
			set { _memVal = value.ToString(CultureInfo.InvariantCulture); }
		}

		//Lats value entered
		private string LastValue
		{
			get
			{
				if (_lastVal == string.Empty)
					return "0";
				return _lastVal;
			}
			set { _lastVal = value; }
		}

		//The current Calculator display
		private string Display { get; set; }
		// Sample event handler:  
		private void OnWindowKeyDown(object sender, TextCompositionEventArgs /*System.Windows.Input.KeyEventArgs*/ e)
		{
			string s = e.Text;
			char c = (s.ToCharArray())[0];
			e.Handled = true;

			if ((c >= '0' && c <= '9') || c == '.' || c == '\b') // '\b' is backspace
			{
				ProcessKey(c);
				return;
			}
			switch (c)
			{
				case '+':
					ProcessOperation("BPlus");
					break;
				case '-':
					ProcessOperation("BMinus");
					break;
				case '*':
					ProcessOperation("BMultiply");
					break;
				case '/':
					ProcessOperation("BDevide");
					break;
				case '%':
					ProcessOperation("BPercent");
					break;
				case '=':
					ProcessOperation("BEqual");
					break;
			}
		}

		private void DigitBtn_Click(object sender, RoutedEventArgs e)
		{
			string s = ((Button)sender).Content.ToString();

			//char[] ids = ((Button)sender).ID.ToCharArray();
			char[] ids = s.ToCharArray();
			ProcessKey(ids[0]);
		}

		private void ProcessKey(char c)
		{
			if (EraseDisplay)
			{
				Display = string.Empty;
				EraseDisplay = false;
			}
			AddToDisplay(c);
		}

		private void ProcessOperation(string s)
		{
			double d = 0.0;
			switch (s)
			{
				case "BPM":
					_lastOper = Operation.Negate;
					LastValue = Display;
					CalcResults();
					LastValue = Display;
					EraseDisplay = true;
					_lastOper = Operation.None;
					break;
				case "BDevide":

					if (EraseDisplay) //stil wait for a digit...
					{
						//stil wait for a digit...
						_lastOper = Operation.Divide;
						break;
					}
					CalcResults();
					_lastOper = Operation.Divide;
					LastValue = Display;
					EraseDisplay = true;
					break;
				case "BMultiply":
					if (EraseDisplay) //stil wait for a digit...
					{
						//stil wait for a digit...
						_lastOper = Operation.Multiply;
						break;
					}
					CalcResults();
					_lastOper = Operation.Multiply;
					LastValue = Display;
					EraseDisplay = true;
					break;
				case "BMinus":
					if (EraseDisplay) //stil wait for a digit...
					{
						//stil wait for a digit...
						_lastOper = Operation.Subtract;
						break;
					}
					CalcResults();
					_lastOper = Operation.Subtract;
					LastValue = Display;
					EraseDisplay = true;
					break;
				case "BPlus":
					if (EraseDisplay)
					{
						//stil wait for a digit...
						_lastOper = Operation.Add;
						break;
					}
					CalcResults();
					_lastOper = Operation.Add;
					LastValue = Display;
					EraseDisplay = true;
					break;
				case "BEqual":
					if (EraseDisplay) //stil wait for a digit...
						break;
                    Analytics.TrackEvent("Pressed Equals");
                    CalcResults();
					EraseDisplay = true;
					_lastOper = Operation.None;
					LastValue = Display;
					//val = Display;
					break;
				case "BSqrt":
					_lastOper = Operation.Sqrt;
					LastValue = Display;
					CalcResults();
					LastValue = Display;
					EraseDisplay = true;
					_lastOper = Operation.None;
					break;
				case "BPercent":
					if (EraseDisplay) //stil wait for a digit...
					{
						//stil wait for a digit...
						_lastOper = Operation.Percent;
						break;
					}
					CalcResults();
					_lastOper = Operation.Percent;
					LastValue = Display;
					EraseDisplay = true;
					//LastOper = Operation.None;
					break;
				case "BOneOver":
					_lastOper = Operation.OneX;
					LastValue = Display;
					CalcResults();
					LastValue = Display;
					EraseDisplay = true;
					_lastOper = Operation.None;
					break;
				case "BC": //clear All
					_lastOper = Operation.None;
					Display = LastValue = string.Empty;
					_paper.Clear();
					UpdateDisplay();
					break;
				case "BCE": //clear entry
					_lastOper = Operation.None;
					Display = LastValue;
					UpdateDisplay();
					break;
				case "BMemClear":
					Memory = 0.0F;
					DisplayMemory();
					break;
				case "BMemSave":
					Memory = Convert.ToDouble(Display);
					DisplayMemory();
					EraseDisplay = true;
					break;
				case "BMemRecall":
					Display = /*val =*/ Memory.ToString(CultureInfo.InvariantCulture);
					UpdateDisplay();
					//if (LastOper != Operation.None)   //using MR is like entring a digit
					EraseDisplay = false;
					break;
				case "BMemPlus":
					d = Memory + Convert.ToDouble(Display);
					Memory = d;
					DisplayMemory();
					EraseDisplay = true;
					break;
			}
		}

		private void OperBtn_Click(object sender, RoutedEventArgs e)
		{
			ProcessOperation(((Button)sender).Name);
		}

		private double Calc(Operation lastOper)
		{
			double d = 0.0;


			try
			{
				switch (lastOper)
				{
					case Operation.Divide:
						_paper.AddArguments(LastValue + " / " + Display);
						d = (Convert.ToDouble(LastValue) / Convert.ToDouble(Display));
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Add:
						_paper.AddArguments(LastValue + " + " + Display);
						d = Convert.ToDouble(LastValue) + Convert.ToDouble(Display);
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Multiply:
						_paper.AddArguments(LastValue + " * " + Display);
						d = Convert.ToDouble(LastValue) * Convert.ToDouble(Display);
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Percent:
						//Note: this is different (but make more sense) then Windows calculator
						_paper.AddArguments(LastValue + " % " + Display);
						d = (Convert.ToDouble(LastValue) * Convert.ToDouble(Display)) / 100.0F;
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Subtract:
						_paper.AddArguments(LastValue + " - " + Display);
						d = Convert.ToDouble(LastValue) - Convert.ToDouble(Display);
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Sqrt:
						_paper.AddArguments("Sqrt( " + LastValue + " )");
						d = Math.Sqrt(Convert.ToDouble(LastValue));
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.OneX:
						_paper.AddArguments("1 / " + LastValue);
						d = 1.0F / Convert.ToDouble(LastValue);
						CheckResult(d);
						_paper.AddResult(d.ToString(CultureInfo.InvariantCulture));
						break;
					case Operation.Negate:
						d = Convert.ToDouble(LastValue) * (-1.0F);
						break;
				}
			}
			catch (Exception e)
			{
				d = 0;
				Window parent = (Window)MyPanel.Parent;
				_paper.AddResult("Error");
				string equation = _paper.args;
                Crashes.TrackError(e, new Dictionary<string, string>() { { "Equation: ", equation } });
                throw e;
				//MessageBox.Show(parent, "Operation cannot be perfomed", parent.Title);
			}

			return d;
		}

		private void CheckResult(double d)
		{
			if (double.IsNegativeInfinity(d) || double.IsPositiveInfinity(d) || double.IsNaN(d))
				throw new Exception("Illegal value");
		}

		private void DisplayMemory()
		{
			if (_memVal != string.Empty)
				BMemBox.Text = "Memory: " + _memVal;
			else
				BMemBox.Text = "Memory: [empty]";
		}

		private void CalcResults()
		{
			double d;
			if (_lastOper == Operation.None)
				return;

			d = Calc(_lastOper);
			Display = d.ToString(CultureInfo.InvariantCulture);

			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			DisplayBox.Text = Display == string.Empty ? "0" : Display;
		}

		private void AddToDisplay(char c)
		{
			if (c == '.')
			{
				if (Display.IndexOf('.', 0) >= 0) //already exists
					return;
				Display = Display + c;
			}
			else
			{
				if (c >= '0' && c <= '9')
				{
					Display = Display + c;
				}
				else if (c == '\b') //backspace ?
				{
					if (Display.Length <= 1)
						Display = string.Empty;
					else
					{
						int i = Display.Length;
						Display = Display.Remove(i - 1, 1); //remove last char 
					}
				}
			}

			UpdateDisplay();
		}

		private void OnMenuAbout(object sender, RoutedEventArgs e)
		{
			Window parent = (Window)MyPanel.Parent;
			MessageBox.Show(parent, parent.Title, parent.Title, MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void OnMenuExit(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnMenuStandard(object sender, RoutedEventArgs e)
		{
			//((MenuItem)ScientificMenu).IsChecked = false;
			StandardMenu.IsChecked = true; //for now always Standard
		}

		private void OnMenuScientific(object sender, RoutedEventArgs e)
		{
			//((MenuItem)StandardMenu).IsChecked = false; 
		}

		private enum Operation
		{
			None,
			Divide,
			Multiply,
			Subtract,
			Add,
			Percent,
			Sqrt,
			OneX,
			Negate
		}

		private class PaperTrail
		{
			private readonly MainWindow _window;
			public string args;

			public PaperTrail(MainWindow window)
			{
				_window = window;
			}

			public void AddArguments(string a)
			{
				args = a;
			}

			public void AddResult(string r)
			{
				_window.PaperBox.Text += args + " = " + r + "\n";
			}

			public void Clear()
			{
				_window.PaperBox.Text = string.Empty;
				args = string.Empty;
			}
		}
	}
}