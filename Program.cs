using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Projekt1
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "plik.csv";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not exists");
                return;
            }
            var inputMatrixFile = new StreamReader("plik.csv");
            var matrix = new List<string>();

            while (!inputMatrixFile.EndOfStream)
            {
                matrix.Add(inputMatrixFile.ReadLine());
            }

            if (!matrix.Any())
            {
                Console.WriteLine("File contains no sequences");
                return;
            }
            (var profile, var alphabet) = CreateProfile(matrix);
            PrintProfileMatrix(profile, alphabet);
            var consensusWord = CreateConsensusWord(profile, alphabet);
            Console.WriteLine(consensusWord);
        }

        private static string CreateConsensusWord(double[][] profile, List<char> alphabet)
        {
            string result = string.Empty;
            for(int j = 0; j < profile[0].Length; j++)
            {
                var max = .0;
                var maxIndex = 0;
                var len = profile.GetLength(0);
                for (int i = 0; i < len; i++)
                {
                    if(profile[i][j]>max)
                    {
                        max = profile[i][j];
                        maxIndex = i;
                    }
                }
                result += alphabet[maxIndex];
            }
            return result;
        }

        static (double[][], List<char>) CreateProfile(List<string> inputMatrix)
        {
            var counterMatrix = new Dictionary<char, List<int>>();
            for (int i = 0; i < inputMatrix.Count; i++)
            {
                var line = inputMatrix[i];
                for (int j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    if (!counterMatrix.ContainsKey(c))
                    {
                        counterMatrix.Add(c, new List<int>(new int[line.Length]));
                    }
                    counterMatrix[c][j] += 1;
                }
            }
            var result = new Dictionary<char, double[]>();
            foreach ((var c, var list) in counterMatrix)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!result.ContainsKey(c))
                    {
                        result.Add(c, new double[list.Count]);
                    }
                    result[c][i] = (double)list[i] / inputMatrix.Count;
                }
            }
            return (result.Values.ToArray(), result.Keys.ToList());
        }

        static void PrintProfileMatrix(double[][] matrix, List<char> alphabet)
        {
            Console.Write("\t");
            for (var i = 1; i <= matrix[0].Length; i++)
            {
                Console.Write($"{i}\t");
            }
            Console.Write("\n");
            for(int i=0;i<matrix.Length;i++)
            {
                Console.Write($"{alphabet[i]}\t");
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write($"{matrix[i][j]}\t");
                }
                Console.Write("\n");
            }
        }
    }
}
