using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        SixState currentState;
        int moveCounter;
        double temperature = 1000.0; 
        double coolingRate = 0.99; 
        Random rand = new Random();

        int[,] hTable;
        List<Point> bMoves; 
        Object chosenMove;

        public Form1()
        {
            InitializeComponent();
            side = pictureBox1.Width / n;

            startState = randomSixState();
            currentState = new SixState(startState);

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void updateUI()
        {
            pictureBox2.Refresh();

            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(currentState);
            bMoves = getBestMoves(hTable);

            listBox1.Items.Clear();
            foreach (Point move in bMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bMoves.Count > 0)
                chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            int[,] hStates = new int[n, n];

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = getAttackingPairs(possible);
                }
            }

            return hStates;
        }

        private List<Point> getBestMoves(int[,] heuristicTable)
        {
            List<Point> bestMoves = new List<Point>();
            int bestHeuristicValue = int.MaxValue;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (currentState.Y[i] != j)
                    {
                        if (heuristicTable[i, j] < bestHeuristicValue)
                        {
                            bestHeuristicValue = heuristicTable[i, j];
                            bestMoves.Clear();
                            bestMoves.Add(new Point(i, j));
                        }
                        else if (heuristicTable[i, j] == bestHeuristicValue)
                        {
                            bestMoves.Add(new Point(i, j));
                        }
                    }
                }
            }
            label5.Text = "Possible Moves (H=" + bestHeuristicValue + ")";
            return bestMoves;
        }

        private Object chooseMove(List<Point> possibleMoves)
        {
            int randomMove = rand.Next(possibleMoves.Count);
            return possibleMoves[randomMove];
        }

        private void executeMove(Point move)
        {
            SixState newState = new SixState(currentState);
            newState.Y[move.X] = move.Y;

            int currentCost = getAttackingPairs(currentState);
            int newCost = getAttackingPairs(newState);

            if (newCost < currentCost || acceptWorseMove(currentCost, newCost))
            {
                currentState = newState;
                moveCounter++;
                updateUI();
                temperature *= coolingRate; 
            }
        }

        private bool acceptWorseMove(int currentCost, int newCost)
        {
            if (temperature <= 0) return false;

            double probability = Math.Exp((currentCost - newCost) / temperature);
            return rand.NextDouble() < probability;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(currentState) > 0)
                executeMove((Point)chosenMove);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState = new SixState(startState);
            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(currentState) > 0 && temperature > 1)
            {
                executeMove((Point)chosenMove);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle the event here
            if (listBox1.SelectedItem != null)
            {
                Point selectedMove = (Point)listBox1.SelectedItem;
                // You can execute the selected move or perform any other logic
                executeMove(selectedMove);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
