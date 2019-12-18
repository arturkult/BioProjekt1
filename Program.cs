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
            string[] fileNames = new string[] { "plik2.csv", "plik1.csv" };
            List<char> alphabet = new List<char> { 'A', 'G', 'T', 'C', '-' };
            List<double[,]> resultProfiles = new List<double[,]>();
            List<List<char>> alphabets = new List<List<char>>();
            List<List<string>> matrices = new List<List<string>>();
            Dictionary<(char, char), double> similarity = new Dictionary<(char, char), double>
            {
                { ('A', 'A'), 1 },
                { ('C', 'C'), 1 },
                { ('T', 'T'), 1 },
                { ('G', 'G'), 1 },
                { ('C', 'A'), 0 },
                { ('T', 'A'), 0 },
                { ('G', 'A'), 0 },
                { ('-', 'A'), 0 },
                { ('A', 'C'), 0 },
                { ('T', 'C'), 0 },
                { ('G', 'C'), 0 },
                { ('-', 'C'), 0 },
                { ('A', 'T'), 0 },
                { ('C', 'T'), 0 },
                { ('G', 'T'), 0 },
                { ('-', 'T'), 0 },
                { ('A', 'G'), 0 },
                { ('C', 'G'), 0 },
                { ('T', 'G'), 0 },
                { ('-', 'G'), 0 },
                { ('A', '-'), 0 },
                { ('C', '-'), 0 },
                { ('T', '-'), 0 },
                { ('G', '-'), 0 },
                { ('-', '-'), 0 },

            };
            try
            {
                foreach (var fileName in fileNames)
                {
                    matrices.Add(CreateMatrix(fileName));
                    var profile = CreateProfile(matrices.Last(), alphabet);
                    resultProfiles.Add(profile);
                    PrintProfileMatrix(profile, alphabet);
                    var consensusWord = CreateConsensusWord(profile, alphabet);
                    Console.WriteLine($"Słowo konsensusowe: {consensusWord}");
                }
                var concatenated = ConcatSequences(resultProfiles.First(),
                                resultProfiles.Last(),
                                matrices.First().Select(s => s.ToCharArray()).ToArray(),
                                matrices.Last().Select(s => s.ToCharArray()).ToArray(),
                                alphabet,
                                similarity);
                Console.WriteLine("Złożenie wielodopasowań");
                foreach (var s in concatenated)
                {
                    Console.WriteLine(s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        private static string[] ConcatSequences(double[,] profile1,
                                                double[,] profile2,
                                                char[][] multi1,
                                                char[][] multi2,
                                                List<char> alphabet,
                                                Dictionary<(char, char), double> similarity)
        {
            var result_length = multi1.Length + multi2.Length;
            var result = Enumerable.Repeat(string.Empty, result_length).ToArray();
            int i = profile1.GetLength(1);
            int j = profile2.GetLength(1);
            while (i > 0 && j > 0)
            {
                (var newi, var newj) = GetBestMove(profile1, profile2, alphabet, similarity, i, j);
                var s = Enumerable.Concat(multi1.Select(c => newi != i ? c[newi] : '-'), multi2.Select(c => newj != j ? c[newj] : '-')).ToArray();
                for (int k = 0; k < result_length; k++)
                {
                    result[k] = s[k] + result[k];
                }
                i = newi;
                j = newj;
            }
            //if (i > 0)
            //{
            //    for (int k = 0; k < i; k++)
            //    {
            //        for (int l = 0; l < multi1.Length; l++)
            //        {
            //            result[l] = '-' + result[l];
            //        }
            //    }
            //}
            //if (j > 0)
            //{
            //    for (int k = 0; k < j; k++)
            //    {
            //        for (int l = multi1.Length; l < result_length; l++)
            //        {
            //            result[l] = '-' + result[l];
            //        }
            //    }
            //}
            return result;
        }

        private static (int, int) GetBestMove(double[,] profile1,
                                              double[,] profile2,
                                              List<char> alphabet,
                                              Dictionary<(char, char), double> similarity,
                                              int profile1Position,
                                              int profile2Position)
        {
            var left = Calculate(profile1, profile1Position, profile2, profile2Position - 1, alphabet, similarity);
            var cross = Calculate(profile1, profile1Position - 1, profile2, profile2Position - 1, alphabet, similarity);
            var up = Calculate(profile1, profile1Position - 1, profile2, profile2Position, alphabet, similarity);
            var max = new double[] { left, up, cross }.Max();
            if (max == cross)
            {
                return (profile1Position - 1, profile2Position - 1);
            }
            if (max == up)
            {
                return (profile1Position - 1, profile2Position);
            }
            return (profile1Position, profile2Position - 1);

        }

        private static double Calculate(double[,] profile1,
                                        int profile1Position,
                                        double[,] profile2,
                                        int profile2Position,
                                        List<char> alphabet,
                                        Dictionary<(char, char), double> similarity)
        {
            double result = 0;
            if(profile2Position >= profile2.GetLength(1) || profile1Position >= profile1.GetLength(1))
            {
                return result;
            }
            for (int i = 0; i < profile1.GetLength(1); i++)
            {
                for (int j = 0; j < profile1.GetLength(1); j++)
                {
                    result += profile1[i, profile1Position] * profile2[j, profile2Position] * similarity[(alphabet[i], alphabet[j])];
                }
            }
            return result;
        }

        private static List<string> CreateMatrix(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("File not exsists");
            }
            var inputMatrixFile = new StreamReader(fileName);
            var matrix = new List<string>();

            while (!inputMatrixFile.EndOfStream)
            {
                matrix.Add(inputMatrixFile.ReadLine());
            }

            if (!matrix.Any())
            {
                throw new Exception("File contains no sequences");
            }

            return matrix;
        }

        private static string CreateConsensusWord(double[,] profile, List<char> alphabet)
        {
            string result = string.Empty;
            for (int j = 0; j < profile.GetLength(1); j++)
            {
                var max = .0;
                var maxIndex = 0;
                var len = profile.GetLength(0);
                for (int i = 0; i < len; i++)
                {
                    if (profile[i, j] > max)
                    {
                        max = profile[i, j];
                        maxIndex = i;
                    }
                }
                result += alphabet[maxIndex];
            }
            return result;
        }

        static double[,] CreateProfile(List<string> inputMatrix, List<char> alphabet)
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
            var result = new double[alphabet.Count, inputMatrix.First().Length];
            foreach ((var c, var list) in counterMatrix)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    result[alphabet.IndexOf(c), i] = (double)list[i] / inputMatrix.Count;
                }
            }
            return result;
        }

        static void PrintProfileMatrix(double[,] matrix, List<char> alphabet)
        {
            Console.Write("\t");
            for (var i = 1; i <= matrix.GetLength(1); i++)
            {
                Console.Write($"{i}\t");
            }
            Console.Write("\n");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                Console.Write($"{alphabet[i]}\t");
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j].ToString("f2")}\t");
                }
                Console.Write("\n");
            }
        }
    }
}
