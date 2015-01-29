﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FoundationsProject1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void generate_Click(object sender, EventArgs e)
        {
            generateGuess();

        }

        private void generateGuess()
        {
            //get the data from the input fields
            //NOTE: THESE NEED TO CHANGE TO MATCH YOUR SETUP 
            //      IF YOU WANT TO TEST THIS
            string cipherPath = "C:\\Users\\Seecs\\Documents\\Visual Studio 2013\\Projects\\FoundationsProject1\\cipher.txt";
            string plainText = "C:\\Users\\Seecs\\Documents\\Visual Studio 2013\\Projects\\FoundationsProject1\\plaintext.txt";
            //Get the characters from the file
            string contentOfCipher = File.ReadAllText(cipherPath);
            string contentOfText = File.ReadAllText(plainText);
            //create a Dictionary for each file
            Dictionary<char, int> countsOfCipher = new Dictionary<char, int>();
            Dictionary<char, int> countsOfPlain = new Dictionary<char, int>();
            //get the counts for the ciphertext
            for (int i = 0; i < contentOfCipher.Length; ++i)
            {
                char c = contentOfCipher[i];
                if (c != '\n' && c != ' ' && c != '\t' && c != '\r')
                {
                    if (countsOfCipher.ContainsKey(c))
                    {
                        countsOfCipher[c]++;
                    }
                    else
                    {
                        countsOfCipher.Add(c, 1);
                    }
                }
            }
            //get the counts for the plain text (calibration text)
            for (int i = 0; i < contentOfText.Length; ++i)
            {
                char c = Char.ToLower(contentOfText[i]);
                if (c != '\n' && c != ' ' && c != '\t' && c != '\r')
                {
                    if (countsOfPlain.ContainsKey(c))
                    {
                        countsOfPlain[c]++;
                    }
                    else
                    {
                        countsOfPlain.Add(c, 1);
                    }
                }
            }

            //This is used to organize each set by frequency
            var freqInCipher = from pair in countsOfCipher
                        orderby pair.Value ascending
                        select pair;
            var freqInPlain = from pair2 in countsOfPlain
                               orderby pair2.Value ascending
                               select pair2;

            //below is print our for checking letters
            /*foreach (KeyValuePair<char, int> kvp in freqInCipher)
            {
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
            }
            foreach (KeyValuePair<char, int> kvp2 in freqInPlain)
            {
                Console.WriteLine("{0}:{1}", kvp2.Key, kvp2.Value);
            }*/


            //the below code is used to map the characters to the characters
            Dictionary<char, char> charMap = new Dictionary<char, char>();
            for (int index = 0; index < freqInCipher.Count(); index++)
            {
                char cip = freqInCipher.ElementAt(index).Key;
                char pla = freqInPlain.ElementAt(index).Key;
                charMap.Add(cip, pla);
            }

            //printout of mapping
            foreach (KeyValuePair<char, char> kvp in charMap)
            {
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
            }

            //below changes the cipher to our best guess, it writes the guess to file
            //it will need to be changed, note that we should delete deciphered.txt after
            //each run. It can be found in the project folder -> FoundationsProject1 -> bin -> debug
            if ((!File.Exists("deciphered.txt")))
            {
                using (FileStream fs = File.Create("deciphered.txt"))
                {

                    byte[] toWrite = new byte[contentOfCipher.Length];
                    for (int i = 0; i < contentOfCipher.Length; ++i)
                    {
                        char c = contentOfCipher[i];
                        if (charMap.ContainsKey(c))
                        {
                            toWrite[i] = (byte) charMap[c];
                        }
                        else
                        {
                            toWrite[i] = (byte)c;
                        }
                    }
                    fs.Write(toWrite, 0, toWrite.Length);
                    fs.Close();
                }
            }
        }
    }
}
