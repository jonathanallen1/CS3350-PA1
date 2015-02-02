using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FoundationsProject1
{
    public partial class Form1 : Form
    {
        private const int ALPHABET_SIZE = 26;

        private ComboBox[] selectors;
        private char[] uppers = new char[] 
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };
        private char[] lowers = new char[] 
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        private string contentOfCipher = "";
        private string contentOfText = "";
        private string decipheredText = "";
        private Dictionary<char, int> countsOfCipher;
        private Dictionary<char, int> countsOfPlain;
        private Dictionary<char, double> countsOfCipherDouble;
        private Dictionary<char, double> countsOfPlainDouble;

        public Form1()
        {
            InitializeComponent();

            // initialize the file selection dialog boxes
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            this.selectors = new ComboBox[] 
            {
                this.comboBoxA, this.comboBoxB, this.comboBoxC, this.comboBoxD, this.comboBoxE,
                this.comboBoxF, this.comboBoxG, this.comboBoxH, this.comboBoxI, this.comboBoxJ,
                this.comboBoxK, this.comboBoxL, this.comboBoxM, this.comboBoxN, this.comboBoxO,
                this.comboBoxP, this.comboBoxQ, this.comboBoxR, this.comboBoxS, this.comboBoxT,
                this.comboBoxU, this.comboBoxV, this.comboBoxW, this.comboBoxX, this.comboBoxY,
                this.comboBoxZ
            };

            foreach (ComboBox box in selectors)
            {
                foreach (char letter in lowers)
                {
                    box.Items.Add(letter);
                }
            }

            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                selectors[index].SelectedIndex = index;
            }

            // Initialize Cipher Count
            countsOfCipher = new Dictionary<char, int>();
            countsOfCipherDouble = new Dictionary<char, double>();
            foreach (char c in lowers)
            {
                countsOfCipher.Add(c, 0);
                countsOfCipherDouble.Add(c, 0);
            }

            // Initialize Calibration Count
            countsOfPlain = new Dictionary<char, int>();
            countsOfPlainDouble = new Dictionary<char, double>();
            foreach (char c in lowers)
            {
                countsOfPlain.Add(c, 0);
                countsOfPlainDouble.Add(c, 0);
            }

            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "#%";
            drawChart();
        }

        private void generate_Click(object sender, EventArgs e)
        {
            // Load Stat Values
            // Fill Dropdowns with guesses
            // Recalculate
            generateGuess();

        }

        private void generateGuess()
        {
            
            //This is used to organize each set by frequency
            var freqInCipher = from pair in countsOfCipher
                               orderby pair.Value ascending
                               select pair;
            var freqInPlain = from pair2 in countsOfPlain
                              orderby pair2.Value ascending
                              select pair2;

            //the below code is used to map the characters to the characters
            Dictionary<char, char> charMap = new Dictionary<char, char>();
            for (int index = 0; index < freqInCipher.Count(); index++)
            {
                char cip = freqInCipher.ElementAt(index).Key;
                char pla = freqInPlain.ElementAt(index).Key;
                charMap.Add(cip, pla);
            }

            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                char c = lowers[index];
                selectors[index].SelectedIndex = charMap[c] - 'a';
            }

            recalculate(null, null);
        }

        // Exit the program using the exit option in the file menu
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void calibrateStats(object sender, EventArgs e)
        {
            DialogResult result = this.openFileDialog1.ShowDialog();
            string calibrationFile;

            if (result == DialogResult.OK)
            {
                calibrationFile = openFileDialog1.FileName;
            }
            else
            {
                return;
            }

            contentOfText = File.ReadAllText(calibrationFile);

            // Reset the plaintext statistics to 0 in case this is not the first calibration
            foreach (char c in lowers)
            {
                countsOfPlain[c] = 0;
            }

            //get the counts for the plain text (calibration text)
            foreach (char letter in contentOfText)
            {
                char c = Char.ToLower(letter);
                if (c >= 'a' && c <= 'z')
                {
                    countsOfPlain[c]++;
                }
            }

            foreach (char letter in lowers)
            {
                countsOfPlainDouble[letter] = (double) countsOfPlain[letter] / (double) contentOfText.Length;
            }

            // Redraw Chart
            drawChart();
        }

        private void recalculate(object sender, EventArgs e)
        {
            string output = "";
            foreach (char c in contentOfCipher)
            {
                char letter = Char.ToLower(c);
                if (letter >= 'a' && letter <= 'z')
                {
                    int index = letter - 'a';
                    output += lowers[selectors[index].SelectedIndex];
                }
                else
                {
                    output += c;
                }
            }

            textBox2.Text = output;
            decipheredText = output;
        }

        private void drawChart()
        {
            chart1.Series["Plain Text"].Points.Clear();
            chart1.Series["Cipher Text"].Points.Clear();

            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                // Calibration Data
                double value = countsOfPlainDouble.ElementAt(index).Value;
                string key = countsOfPlainDouble.ElementAt(index).Key.ToString();

                DataPoint data = new DataPoint(index, value);
                data.AxisLabel = key;
                chart1.Series["Plain Text"].Points.Add(data);

                // Cipher Data
                value = countsOfCipherDouble.ElementAt(index).Value;

                data = new DataPoint(index, value);
                data.AxisLabel = key;
                chart1.Series["Cipher Text"].Points.Add(data);
            }
        }

        private void loadCipherText(object sender, EventArgs e)
        {
            DialogResult result = this.openFileDialog1.ShowDialog();
            string cipherFile;

            if (result == DialogResult.OK)
            {
                cipherFile = openFileDialog1.FileName;
            }
            else
            {
                return;
            }

            contentOfCipher = File.ReadAllText(cipherFile);
            textBox1.Text = contentOfCipher;

            // Reset the ciphertext statistics to 0 in case this is not the first file loaded
            foreach (char c in lowers)
            {
                countsOfCipher[c] = 0;
            }

            //get the counts for the cipher text
            foreach (char letter in contentOfCipher)
            {
                char c = Char.ToLower(letter);
                if (c >= 'a' && c <= 'z')
                {
                    countsOfCipher[c]++;
                }
            }

            foreach (char letter in lowers)
            {
                countsOfCipherDouble[letter] = (double) countsOfCipher[letter] / (double) contentOfCipher.Length;
            }

            // Redraw Chart
            drawChart();
        }

        private void saveDecipheredAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = this.saveFileDialog1.ShowDialog();
            string decipherFile;

            if (result == DialogResult.OK)
            {
                decipherFile = saveFileDialog1.FileName;
            }
            else
            {
                return;
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(decipherFile);
            file.Write(decipheredText);

            file.Close();
        }
    }
}
