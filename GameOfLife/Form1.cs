using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
	public partial class Form1 : Form
	{
		bool[][] Grid;
		bool[][] OldGrid;
		Pen BlackPen = new Pen(Color.Black, 0.5f);
		Bitmap GridBack;

		String ActivePattern = "OO\nOO";

		bool BOUNDLESS = false;
		bool PLACING = false;
		int ROTATIONS = 0;
		bool PROCESSING = false;
		bool MOUSEDOWN = false;
		bool PAUSED = false;
		int SPEED = 20;

		System.Timers.Timer timer;

		int SIZE = 5;

		int W_OFF = 20; //Form bloat
		int H_OFF = 43;

		int WIDTH = 200;
		int HEIGHT = 200;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.UserPaint |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.ResizeRedraw |
						  ControlStyles.ContainerControl |
						  ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.SupportsTransparentBackColor
						  , true);

			Init(WIDTH,HEIGHT);
		}

		public void Init(int WIDTH, int HEIGHT)
		{
			MOUSEDOWN = false;
			PAUSED = true;

			StartTimer();

			Size = new Size(WIDTH * SIZE + W_OFF, HEIGHT * SIZE + H_OFF);

			InitGrid(WIDTH, HEIGHT);

			ActivePattern = @"...........O......
	.........OO.O.....
	.......OO.........
	..........OO......
	.......OOO........
	..................
	OOO...............
	...OO..OOO.OO.....
	..........OOOOOOO.
	.OOOOOOO..........
	.....OO.OOO..OO...
	...............OOO
	..................
	........OOO.......
	......OO..........
	.........OO.......
	.....O.OO.........
	......O...........";

			//InsertShapeAtSpot(ConvertStringToShape("OO\nOO"), WIDTH / 2, 10, 2);


			PAUSED = true;
		}

		public void StartTimer()
		{
			timer = new System.Timers.Timer(SPEED);

			timer.AutoReset = true;
			timer.Elapsed += timer_Elapsed;

			timer.Start();
		}

		public Point GetGridByPoint(int x, int y)
		{
			return new Point((int)Math.Floor(x / (double)SIZE), (int)Math.Floor(y / (double)SIZE));
		}

		void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (!PAUSED) DoCycle();
			Invalidate();
		}

		public void DoCycle()
		{
			if (PROCESSING) return;
			PROCESSING = true;
			OldGrid = Grid.Select(a => a.ToArray()).ToArray();

			for (int i = 0; i < Grid.Length; i++)
			{
				for (int j = 0; j < Grid[i].Length; j++)
				{
					if (Grid[i][j] && LivingNeighbors(OldGrid, i, j) > 3)
						Grid[i][j] = false;
					if (Grid[i][j] && LivingNeighbors(OldGrid, i, j) < 2)
						Grid[i][j] = false;
					if (!Grid[i][j] && LivingNeighbors(OldGrid, i, j) == 3)
						Grid[i][j] = true;
				}
			}
			PROCESSING = false;
		}

		public void InsertShapeAtSpot(bool[][] shape, int x, int y, int rotations = 0)
		{
			bool state = PAUSED;
			PAUSED = true;
			shape = RotateMatrix(shape, rotations);

			for (int i = x; i < x + shape.Length; i++)
			{
				for(int j = y; j < y + shape[i-x].Length; j++)
				{
					if (i >= 0 && i < WIDTH && j >= 0 && j < HEIGHT)
						Grid[i][j] = shape[i - x][j - y];
				}
			}
			PAUSED = state;
		}

		public bool[][] ConvertStringToShape(string s)
		{
			string[] bits = s.Split(new char[]{'\n'});

			bool[][] ret = new bool[bits[0].Trim().Length][];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = new bool[bits.Length];
			}

			for(int i = 0; i < bits.Length; i++)
			{
				for (int j = 0; j < bits[i].Trim().Length; j++)
				{
					if (bits[i].Trim().ToCharArray()[j] == 'O')
						ret[j][i] = true;
					else
						ret[j][i] = false;
				}
			}

			return ret;
		}

		public bool[][] RotateMatrix(bool[][] oldMatrix, int times)
		{
			times %= 4;

			bool[][] ret = oldMatrix;
			for (int i = 0; i < times; i++)
			{
				ret = RotateMatrixCounterClockwise(ret);
			}

			return ret;
		}

		public bool[][] RotateMatrixCounterClockwise(bool[][] mat)
		{
			bool[,] oldMatrix = new bool[mat.Length, mat[0].Length];
			for (int i = 0; i < mat.Length; i++)
				for (int j = 0; j < mat[i].Length; j++)
					oldMatrix[i, j] = mat[i][j];

			bool[,] newMatrix = new bool[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
			int newColumn, newRow = 0;
			for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
			{
				newColumn = 0;
				for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
				{
					newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
					newColumn++;
				}
				newRow++;
			}

			bool[][] ret = new bool[newMatrix.GetLength(0)][];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = new bool[newMatrix.GetLength(1)];
				for (int j = 0; j < ret[i].Length; j++)
					ret[i][j] = newMatrix[i, j];
			}
			return ret;
		}

		public bool GetNeighborAlive(bool[][] Grid, int w, int h, int x, int y)
		{
			if (!BOUNDLESS)
				return (w + x > 0 && w + x < WIDTH && h + y > 0 && h + y < HEIGHT && Grid[w + x][h + y]);
			else
			{
				int px = (w + x + Grid.Length) % Grid.Length;
				int py = (h + y + Grid[px].Length) % Grid[px].Length;
				return (Grid[px][py]);
			}
				
		}

		public int LivingNeighbors(bool[][] Grid, int w, int h)
		{
			int c = 0;

			if (GetNeighborAlive(Grid, w, h, 1, 0)) c++;
			if (GetNeighborAlive(Grid, w, h, 1, 1)) c++;
			if (GetNeighborAlive(Grid, w, h, 0, 1)) c++;
			if (GetNeighborAlive(Grid, w, h, -1, 1)) c++;

			if (GetNeighborAlive(Grid, w, h, -1, 0)) c++;
			if (GetNeighborAlive(Grid, w, h, -1, -1)) c++;
			if (GetNeighborAlive(Grid, w, h, 0, -1)) c++;
			if (GetNeighborAlive(Grid, w, h, 1, -1)) c++;

			return c;
		}

		public void DrawPixels(Graphics g)
		{
			Rectangle[] on = GetOnRectangles();
			Rectangle[] off = GetOffRectangles();

			if (on.Length == 0 || off.Length == 0) return;

			g.FillRectangles(Brushes.Black, on);
			g.FillRectangles(Brushes.White, off);
		}

		public Rectangle[] GetOnRectangles()
		{
			List<Rectangle> on = new List<Rectangle>();
			for(int i = 0; i < Grid.Length; i++)
			{
				for (int j = 0; j < Grid[i].Length; j++)
				{
					if (Grid[i][j])
					{
						on.Add(new Rectangle(i * SIZE + 1, j * SIZE + 1, (SIZE - 1), (SIZE - 1)));
					}
				}
			}
			return on.ToArray();
		}

		public Rectangle[] GetOffRectangles()
		{
			List<Rectangle> off = new List<Rectangle>();
			for (int i = 0; i < Grid.Length; i++)
			{
				for (int j = 0; j < Grid[i].Length; j++)
				{
					if (!Grid[i][j])
					{
						off.Add(new Rectangle(i * SIZE + 1, j * SIZE + 1, (SIZE - 2), (SIZE - 2)));
					}
				}
			}
			return off.ToArray();
		}

		public void InitGrid(int WIDTH, int HEIGHT)
		{
			Grid = new bool[WIDTH][];
			OldGrid = new bool[WIDTH][];
			for (int i = 0; i < WIDTH; i++)
			{
				Grid[i] = new bool[HEIGHT];
				OldGrid[i] = new bool[HEIGHT];
				for (int j = 0; j < HEIGHT; j++)
				{
					Grid[i][j] = false;
					OldGrid[i][j] = false;
				}
			}
		}

		public void AdjustSpeed(int dms)
		{
			SPEED -= dms;

			if (SPEED < 10) SPEED = 10;
			if (SPEED > 1000) SPEED = 1000;

			timer.Interval = SPEED;
		}

		public void ClearAllCells()
		{
			timer.Stop();
			for (int i = 0; i < WIDTH; i++)
			{
				for (int j = 0; j < HEIGHT; j++)
				{
					Grid[i][j] = false;
					OldGrid[i][j] = false;
				}
			}
			timer.Start();
		}

		public void GenerateBackgroundGrid(Graphics g)
		{
			int w = this.Size.Width;
			int h = this.Size.Height;

			GridBack = new Bitmap(w, h);

			g.Clear(Color.White);

			for (int i = 0; i <= w; i += w / Grid.Length)
			{
				g.DrawLine(BlackPen, new Point(i, 0), new Point(i, h));
			}

			for (int i = 0; i <= h; i += h / Grid[0].Length)
			{
				g.DrawLine(BlackPen, new Point(0, i), new Point(w, i));
			}
		}

		private void PromptPatternInput()
		{
			using (Form prompt = new Form())
			{
				prompt.Size = new Size(300, 300);

				TextBox input = new TextBox();
				input.Multiline = true;
				input.ScrollBars = ScrollBars.Both;
				input.Dock = DockStyle.Top;

				Button b = new Button();
				b.Dock = DockStyle.Bottom;
				b.Text = "Submit";
				b.Click += (s, e) => { ActivePattern = input.Text; prompt.Close(); };

				input.Height = prompt.ClientRectangle.Height - b.Height;
				input.Width = prompt.ClientRectangle.Width;
				input.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				
				prompt.Controls.Add(b);
				prompt.Controls.Add(input);
				prompt.ShowDialog();
			}
		}


		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			GenerateBackgroundGrid(e.Graphics);
			DrawPixels(e.Graphics);
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			Point p = GetGridByPoint(e.Location.X, e.Location.Y);

			MOUSEDOWN = !PLACING;
			if (e.Button == MouseButtons.Left)
				if (PLACING)
					InsertShapeAtSpot(ConvertStringToShape(ActivePattern), p.X, p.Y, ROTATIONS);
				else
					Grid[p.X][p.Y] = true;
			else
				Grid[p.X][p.Y] = false;

			Invalidate();
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e)
		{
			MOUSEDOWN = false;
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e)
		{
			if (MOUSEDOWN)
			{
				Point p = GetGridByPoint(e.Location.X, e.Location.Y);

				if (p.X < 0 || p.X > WIDTH - 1 || p.Y < 0 || p.Y > HEIGHT - 1) return;

				if (e.Button == MouseButtons.Right)
					Grid[p.X][p.Y] = false;
				else
					Grid[p.X][p.Y] = true;

				Invalidate();
			}
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == ' ')
			{
				PAUSED = !PAUSED;
			}
			if (e.KeyChar == 'c')
			{
				ClearAllCells();
			}
			if (e.KeyChar == '[')
			{
				AdjustSpeed(-10);
			}
			if (e.KeyChar == ']')
			{
				AdjustSpeed(10);
			}
			if (e.KeyChar == ',')
			{
				WIDTH--;
				HEIGHT--;
				Init(WIDTH, HEIGHT);
			}
			if (e.KeyChar == '.')
			{
				WIDTH++;
				HEIGHT++;
				Init(WIDTH, HEIGHT);
			}
			if (e.KeyChar == 'v')
			{
				PLACING = !PLACING;
			}
			if (e.KeyChar == 'r')
			{
				ROTATIONS++;
				ROTATIONS %= 4;
			}
			if (e.KeyChar == 'x')
			{
				BOUNDLESS = !BOUNDLESS;
			}
			if (e.KeyChar == 'e')
			{
				PromptPatternInput();
			}
		}
	}
}
