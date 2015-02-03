/**
 * This app is a tool to help users decipher a message that is encripted using a 
 * substitution cipher.
 * 
 * Project: FoundationsProject1
 * File:    Form1.cs
 * Author:  Jonathan Allen and Jacob Secor
 * Date:    2/2/14
 * Class:   CS3350 Foundations of Computer Security Programming Assignment 1
 * School:  Cedarville University
 * 
 * Summary of Modifications:
 *  - 2/2/14 JFA Project successfully working, commented, and ready to turn in.
 *  
 * Project Requirements:
 *  - Develop a tool for a cryptanalyst to use to decode a substitution cipher.
 *  - Your tool must have the following capabilities: 
 *      1. the analyst provides a text file to calibrate the plaintext letter 
 *         frequencies
 *      2. the analyst provides a text file that contains the ciphertext
 *      3. the tool allows the analyst to choose ciphertext-to-plaintext letter 
 *         mappings and then displays the putative plaintext
 */


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
    /// <summary>
    /// This app is a tool to help decipher a message that is encripted with a 
    /// substitution cipher.
    /// </summary>
    public partial class Form1 : Form
    {
        private const int ALPHABET_SIZE = 26;

        private char[] alphabet = new char[] 
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        // Holds all 26 of the comboboxes for mapping characters to each other
        private ComboBox[] selectors;
        // Stores the character values of the contents of the elements of selectors
        private char[] comboBoxValues;

        // The string that holds the value of the encrypted cipher text
        private string contentOfCipher = "";
        // The string that holds the value of the plaintext calibration text
        private string contentOfText = "";
        // The string that holds the value of the decrypted cipher text
        private string decipheredText = "";

        // Number of times each letters of the alphabet appear in the ciphertext
        private Dictionary<char, int> countsOfCipher;
        // Number of times each letters of the alphabet appear in the calibration text
        private Dictionary<char, int> countsOfPlain;
        // Percentage of times letters of the alphabet appear in the ciphertext
        private Dictionary<char, double> percentOfCipher;
        // Percentage of times letters of the alphabet appear in the calibration text
        private Dictionary<char, double> percentOfPlain;

        private bool init = true;

        /// <summary>
        /// Initialize the default settings for the app when it first loads.
        /// </summary>
        public Form1()
        {
            init = true;
            InitializeComponent();

            // initialize settings for the file selection dialog boxes
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Each comboBox corresbonds to a letter of the Alphabet
            this.selectors = new ComboBox[] 
            {
                this.comboBoxA, this.comboBoxB, this.comboBoxC, this.comboBoxD, this.comboBoxE,
                this.comboBoxF, this.comboBoxG, this.comboBoxH, this.comboBoxI, this.comboBoxJ,
                this.comboBoxK, this.comboBoxL, this.comboBoxM, this.comboBoxN, this.comboBoxO,
                this.comboBoxP, this.comboBoxQ, this.comboBoxR, this.comboBoxS, this.comboBoxT,
                this.comboBoxU, this.comboBoxV, this.comboBoxW, this.comboBoxX, this.comboBoxY,
                this.comboBoxZ
            };

            // Add all 26 letters as options for all 26 combo-boxes
            foreach (ComboBox box in selectors)
            {
                foreach (char letter in alphabet)
                {
                    box.Items.Add(letter);
                }
            }

            // Initialize the selected values of the comboboxes (a to A, b to B, etc)
            this.comboBoxValues = new char[ALPHABET_SIZE];
            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                selectors[index].SelectedIndex = index;
                comboBoxValues[index] = alphabet[index];
            }

            // Initialize Cipher Count
            countsOfCipher = new Dictionary<char, int>();
            percentOfCipher = new Dictionary<char, double>();
            foreach (char c in alphabet)
            {
                countsOfCipher.Add(c, 0);
                percentOfCipher.Add(c, 0);
            }

            // Initialize Calibration Count
            countsOfPlain = new Dictionary<char, int>();
            percentOfPlain = new Dictionary<char, double>();
            foreach (char c in alphabet)
            {
                countsOfPlain.Add(c, 0);
                percentOfPlain.Add(c, 0);
            }

            // Intialize the statistic chart settings
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "#%";
            drawChart();

            init = false;
        }

        /// <summary>
        /// Load the cipher text from a file and print it to the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadCipherText(object sender, EventArgs e)
        {
            // Get the file name from a dialogue window
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

            // Read in the encrypted message from the file
            contentOfCipher = File.ReadAllText(cipherFile);
            textBox1.Text = contentOfCipher;

            // Reset the ciphertext statistics to 0 in case this is not the first file loaded
            foreach (char letter in alphabet)
            {
                countsOfCipher[letter] = 0;
            }

            // get the counts for the cipher text
            foreach (char c in contentOfCipher)
            {
                char letter = Char.ToLower(c);
                if (letter >= 'a' && letter <= 'z')
                {
                    countsOfCipher[letter]++;
                }
            }

            // calculate the percentage counts for the cipher text
            foreach (char letter in alphabet)
            {
                percentOfCipher[letter] = (double)countsOfCipher[letter] / (double)contentOfCipher.Length;
            }

            // Redraw Chart
            drawChart();

            // Try to decrypt the message
            recalculate();
        }

        /// <summary>
        /// Open the calibration file and cound how many times each letter occurs to 
        /// compare to the cipher text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calibrateStats(object sender, EventArgs e)
        {
            // Get the file name from a dialogue box
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

            // Read in the calibration text (plaintext)
            contentOfText = File.ReadAllText(calibrationFile);

            // Reset the plaintext statistics to 0 in case this is not the first calibration
            foreach (char letter in alphabet)
            {
                countsOfPlain[letter] = 0;
            }

            // count the occurence of each alphabet letter in the plain text (calibration text)
            foreach (char c in contentOfText)
            {
                char letter = Char.ToLower(c);
                if (letter >= 'a' && letter <= 'z')
                {
                    countsOfPlain[letter]++;
                }
            }

            // Calculate the parcentage of times each letter occurs in the plaintext
            foreach (char letter in alphabet)
            {
                percentOfPlain[letter] = (double)countsOfPlain[letter] / (double)contentOfText.Length;
            }

            // Redraw Chart with new values
            drawChart();
        }

        /// <summary>
        /// Print statistics for how ofter letters appear in vertical bar graph
        /// </summary>
        private void drawChart()
        {
            // Clear the old datapoint values
            chart1.Series["Plain Text"].Points.Clear();
            chart1.Series["Cipher Text"].Points.Clear();

            // For each letter in the alphabet
            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                // Calibration Data
                double value = percentOfPlain.ElementAt(index).Value;
                string key = percentOfPlain.ElementAt(index).Key.ToString();

                DataPoint data = new DataPoint(index, value);
                data.AxisLabel = key;
                chart1.Series["Plain Text"].Points.Add(data);

                // Cipher Data
                value = percentOfCipher.ElementAt(index).Value;

                data = new DataPoint(index, value);
                data.AxisLabel = key;
                chart1.Series["Cipher Text"].Points.Add(data);
            }
        }

        /// <summary>
        /// The best guess feature maps the characters that occur most ofter in the
        /// calibration text to the ones who occur most ofter in the cipher text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generate_Click(object sender, EventArgs e)
        {
            //This is used to organize each set by frequency
            var freqInCipher = from pair in countsOfCipher
                               orderby pair.Value ascending
                               select pair;
            var freqInPlain = from pair2 in countsOfPlain
                              orderby pair2.Value ascending
                              select pair2;

            //the below code maps the cipher characters to the plaintext characters
            Dictionary<char, char> charMap = new Dictionary<char, char>();
            for (int index = 0; index < freqInCipher.Count(); index++)
            {
                char cip = freqInCipher.ElementAt(index).Key;
                char pla = freqInPlain.ElementAt(index).Key;
                charMap.Add(cip, pla);
            }

            // Take the mapped values and push them into the comboBoxes in the GUI
            for (int index = 0; index < ALPHABET_SIZE; index++)
            {
                char c = alphabet[index];
                selectors[index].SelectedIndex = charMap[c] - 'a';
                comboBoxValues[index] = charMap[c];
            }

            // Redetermine the message based on the new combobox values
            recalculate();
        }

        /// <summary>
        /// The method ensures that when the value of a combobox is changed, another box that
        /// has the same value as the new value will be assigned this box's old value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This prevents 2 comboboxes from holding the same value at the same time 
        /// (not a problem for the code, but confusing to the user).
        /// </remarks>
        private void valueChanged(object sender, EventArgs e)
        {
            if (init) return;

            for (int i = 0; i < ALPHABET_SIZE; i++)
            {
                // Get the index of the box that was changed
                if (selectors[i] == (ComboBox)sender)
                {
                    for (int j = 0; j < ALPHABET_SIZE; j++)
                    {
                        // Get the index of the other box with that value
                        if (selectors[j].SelectedIndex == ((ComboBox)sender).SelectedIndex && i != j)
                        {
                            // Swap values
                            selectors[j].SelectedIndex = comboBoxValues[i] - 'a';
                            comboBoxValues[i] = (char) ('a' + selectors[i].SelectedIndex);
                            comboBoxValues[j] = (char) ('a' + selectors[j].SelectedIndex);
                        }
                    }
                    break;
                }
            }            

            // Redetermine the meaning of the encrypted text using the new values.
            recalculate();
        }

        /// <summary>
        /// Re-determine the decipher-ed message based on the values in the comboBoxes
        /// </summary>
        private void recalculate()
        {
            decipheredText = "";
            foreach (char c in contentOfCipher)
            {
                char letter = Char.ToLower(c);
                if (letter >= 'a' && letter <= 'z')
                {
                    // Each letter is mapped to a different letter
                    int index = letter - 'a';
                    decipheredText += comboBoxValues[index];
                }
                else
                {
                    // Punctuation and whitespace can stay the same
                    decipheredText += c;
                }
            }

            // Display the results on the screen
            textBox2.Text = decipheredText;
        }

        /// <summary>
        /// Save the deciphered message to a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAs(object sender, EventArgs e)
        {
            // Get the file to write to
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

            // Write to that file
            System.IO.StreamWriter file = new System.IO.StreamWriter(decipherFile);
            file.Write(decipheredText);

            file.Close();
        }

        /// <summary>
        ///  Exit the program using the exit option in the file menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
