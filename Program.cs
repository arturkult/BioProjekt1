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

            //(string matrixFileName, string[] fileNames, List<char> alphabet) = ParseArguments(args);
            string matrixFileName = "similarity.csv";
            string[] fileNames = new string[] { "plik2.csv", "plik1.csv" };
            //string[] fileNames = new string[] { "plik2.csv", "plik1.csv" };
            List<char> alphabet = new List<char> { 'A', 'G', 'T', 'C', '-' };
            List<double[,]> resultProfiles = new List<double[,]>();
            List<List<char>> alphabets = new List<List<char>>();
            List<List<string>> matrices = new List<List<string>>();
            Dictionary<(char, char), double> similarity = GetSimilarityMatrix(matrixFileName);
            GuideTree guideTree;

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

                guideTree = CreateGuideTreeWithUPGMA(CreateDistanceMatrix(matrices, alphabet), new List<string> { "A", "B", "C", "D", "E", "F", "G"});
                Console.WriteLine("\nGuide tree:");
                guideTree.PrintTree();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        private static (string matrixFileName, string[] fileNames, List<char> alphabet) ParseArguments(string[] args)
        {
            string matrixFileName = string.Empty;
            string[] fileNames = new string[2];
            var alphabet = new List<char>();
            for (int i = 0; i < args.Length; i += 2)
            {
                string command = args[i];
                string argument = args[i + 1];
                switch (command)
                {
                    case "--similarity":
                    case "-s":
                        matrixFileName = argument;
                        break;
                    case "--input":
                    case "-i":
                        fileNames = argument.Split(';');
                        break;
                    case "--alphabet":
                    case "-a":
                        alphabet = argument.Split(';').SelectMany(a => a.ToUpper().ToCharArray()).ToList();
                        break;
                }
            }
            return (matrixFileName, fileNames, alphabet);
        }

        private static Dictionary<(char, char), double> GetSimilarityMatrix(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("File not exsists");
            }
            var result = new Dictionary<(char, char), double>();
            using var fileReader = new StreamReader(fileName);
            var header = fileReader.ReadLine().Split(';');
            while (!fileReader.EndOfStream)
            {
                var line = fileReader.ReadLine();
                var lineChars = line.Split(';');
                for (var i = 1; i < lineChars.Length; i++)
                {
                    result.Add(
                        (lineChars[0].ToCharArray().First(),
                        header[i].ToCharArray().First()),
                        double.Parse(lineChars[i]));
                }
            }
            return result;
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
            var scoreMatrix = CreateScoreMatrix(profile1, profile2, alphabet, similarity);
            PrintScoreMatrix(scoreMatrix.Item2);
            while(i>0 || j > 0)
            {
                var newi = i;
                var newj = j;
                switch (scoreMatrix.Item2[i, j])
                {
                    case 'C':
                        newi -= 1;
                        newj -= 1;
                        break;
                    case 'U':
                        newi -= 1;
                        break;
                    case 'L':
                        newj -= 1;
                        break;
                }
                var s = Enumerable.Concat(
                                        multi1.Select(c => newi != i && i > 0 ? c[i - 1] : '-'),
                                        multi2.Select(c => newj != j && j > 0 ? c[j - 1] : '-')
                                        ).ToArray();
                for (int k = 0; k < result_length; k++)
                {
                    result[k] = s[k] + result[k];
                }
                i = newi;
                j = newj;
            }
            return result;
        }

        /* double GetSimilarity(profile p1, profile p2, int i, int j, matrix similarityMatrix)
         * suma wg wzoru ktory rozpisalem
         * 
         * matrix computeAllignment(profile p1, profile p2, matrix similarityMatrix)
         * matrix allignmentMatrix = new matrix(p1.Length +1, p2.Length+1)
         * 0,0 = jest 0
         * for (auto i =1; i <allignmentMatrix.X; i++)
         * {
         * allignmentMatrix(i,0) = alignmentMatrix(i-1,0) + GetSimilarity(p1(i), '-')
         * }
         * for (auto i =1; i <allignmentMatrix.Y; i++)
         * {
         * allignmentMatrix(0,i) = alignmentMatrix(0,j-1) + GetSimilarity(p2(i), '-')
         * }
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * */

        private static void PrintScoreMatrix(double[,] matrix)
        {
            Console.WriteLine("Score Matrix:");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j].ToString("f2")}\t");
                }
                Console.Write("\n");
            }
        }
        private static void PrintScoreMatrix(char[,] matrix)
        {
            Console.WriteLine("Score Matrix:");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j]}\t");
                }
                Console.Write("\n");
            }
        }

        private static (double[,], char[,]) CreateScoreMatrix(
            double[,] profile1,
            double[,] profile2,
            List<char> alphabet,
            Dictionary<(char, char), double> similarity)
        {
            var matrix = new double[profile1.GetLength(1) + 1, profile2.GetLength(1) + 1];
            var result = new char[profile1.GetLength(1) + 1, profile2.GetLength(1) + 1];
            matrix[0, 0] = 0;
            for (int i = 1; i <= profile1.GetLength(1); i++)
            {
                matrix[i, 0] = matrix[i - 1, 0] + Calculate(profile1,
                                                            i - 1,
                                                            profile1Indel: false,
                                                            profile2,
                                                            0,
                                                            profile2Indel: true,
                                                            alphabet,
                                                            similarity);
                result[i, 0] = 'U';
            }
            for (int j = 1; j <= profile2.GetLength(1); j++)
            {
                matrix[0, j] = matrix[0, j - 1] + Calculate(profile1,
                                                            0,
                                                            profile1Indel: true,
                                                            profile2,
                                                            j - 1,
                                                            profile2Indel: false,
                                                            alphabet,
                                                            similarity);
                result[0, j] = 'L';
            }
            for (int i = 1; i <= profile1.GetLength(1); i++)
            {
                for (int j = 1; j <= profile2.GetLength(1); j++)
                {
                    var left = matrix[i, j - 1] + Calculate(profile1,
                                                            i - 1,
                                                            profile1Indel: true,
                                                            profile2,
                                                            j - 1,
                                                            profile2Indel: false,
                                                            alphabet,
                                                            similarity);
                    var cross = matrix[i - 1, j - 1] + Calculate(profile1,
                                                                i - 1,
                                                                profile1Indel: false,
                                                                profile2,
                                                                j - 1,
                                                                profile2Indel: false,
                                                                alphabet,
                                                                similarity);
                    var up = matrix[i - 1, j] + Calculate(profile1,
                                                          i - 1,
                                                          profile1Indel: false,
                                                          profile2,
                                                          j - 1,
                                                          profile2Indel: true,
                                                          alphabet,
                                                          similarity);
                    matrix[i, j] = new double[] { up, left, cross }.Max();
                    if(matrix[i, j] == cross)
                    {
                        result[i, j] = 'C';
                    }
                    else if (matrix[i, j] == left)
                    {
                        result[i, j] = 'L';
                    }
                    else
                    {
                        result[i, j] = 'U';
                    }
                }
            }
            return (matrix,result);
        }

        private static double Calculate(double[,] profile1,
                                        int profile1Position,
                                        bool profile1Indel,
                                        double[,] profile2,
                                        int profile2Position,
                                        bool profile2Indel,
                                        List<char> alphabet,
                                        Dictionary<(char, char), double> similarity)
        {
            double result = 0;
            if (profile2Position >= profile2.GetLength(1)
                || profile1Position >= profile1.GetLength(1)
                || profile1Position < 0
                || profile2Position < 0)
            {
                return result;
            }
            if (profile1Indel)
            {
                for (int i = 0; i < profile2.GetLength(0); i++)
                {
                    result += 1 * profile2[i, profile2Position] * similarity[('-', alphabet[i])];
                }
                return result;
            }
            if (profile2Indel)
            {
                for (int i = 0; i < profile1.GetLength(0); i++)
                {
                    result += 1 * profile1[i, profile1Position] * similarity[('-', alphabet[i])];
                }
                return result;
            }
            for (int i = 0; i < profile1.GetLength(0); i++)
            {
                for (int j = 0; j < profile2.GetLength(0); j++)
                {
                    result += (profile1Indel ? 1 : profile1[i, profile1Position]) *
                              (profile2Indel ? 1 : profile2[j, profile2Position]) *
                              similarity[(profile1Indel ? '-' : alphabet[i],
                                          profile2Indel ? '-' : alphabet[j])];
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

        static double[,] CreateDistanceMatrix(List<List<string>> sequences, List<char> alphabet)
        {
            // do celow testowych
            //var result = new double[7, 7];
            //result[1, 0] = 19;
            //result[2, 0] = 27;
            //result[2, 1] = 31;
            //result[3, 0] = 8;
            //result[3, 1] = 18;
            //result[3, 2] = 26;
            //result[4, 0] = 33;
            //result[4, 1] = 36;
            //result[4, 2] = 41;
            //result[4, 3] = 31;
            //result[5, 0] = 18;
            //result[5, 1] = 1;
            //result[5, 2] = 32;
            //result[5, 3] = 17;
            //result[5, 4] = 35;
            //result[6, 0] = 13;
            //result[6, 1] = 13;
            //result[6, 2] = 29;
            //result[6, 3] = 14;
            //result[6, 4] = 28;
            //result[6, 5] = 12;
            //return result;


            var result = new double[sequences.Count, sequences.Count];

            for (int i = 0; i < sequences.Count; i++)
            {
                for (int j = 0; j < i; i++)
                {
                    CountDistanceMatrixCellValue(sequences[i], sequences[j], alphabet);
                }
            }
            return result;
        }

        static double CountDistanceMatrixCellValue(List<string> firstSequence, List<string> secondSequence, List<char> alphabet)
        {
            return 0;
        }

        static GuideTree CreateGuideTreeWithUPGMA(double[,] distanceMatrix, List<string> sequencesNames)
        {
            GuideTree guideTree = new GuideTree(InitializeGuideTreeGroups(sequencesNames));
            List<List<int>> idsOfSequencesInGroups = new List<List<int>>();

            for (int i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                List<int> newList = new List<int>();
                newList.Add(i);
                idsOfSequencesInGroups.Add(newList);
            }

            DoUPGMAIteration(guideTree, distanceMatrix, distanceMatrix,idsOfSequencesInGroups);
            return guideTree;
        }

        static List<GuideTreeNode> InitializeGuideTreeGroups(List<string> sequencesNames)
        {
            List<GuideTreeNode> result = new List<GuideTreeNode>();

            for (int i = 0; i < sequencesNames.Count; i++)
            {
                result.Add(new GuideTreeNode(0, 0, null, null, sequencesNames[i]));
            }

            return result;
        }

        static void DoUPGMAIteration(GuideTree guideTree, double[,] actualDistanceMatrix,
            double[,] originalDistanceMatrix, List<List<int>> idsOfSequencesInGroups)
        {
            (int, int, double) shortestDistanceValues;

            if (actualDistanceMatrix.Length <= 1)
            {
                return;
            }

            shortestDistanceValues = FindShortestPairwiseDistanceCoordinatesAndValues(actualDistanceMatrix);
            guideTree.JoinGroups(shortestDistanceValues.Item1, shortestDistanceValues.Item2, shortestDistanceValues.Item3);
            idsOfSequencesInGroups = UpdateSequencesGroups(idsOfSequencesInGroups,
                (shortestDistanceValues.Item1, shortestDistanceValues.Item2));
            actualDistanceMatrix = CreateNewDistanceMatrix(actualDistanceMatrix, idsOfSequencesInGroups, originalDistanceMatrix);

            DoUPGMAIteration(guideTree, actualDistanceMatrix, originalDistanceMatrix, idsOfSequencesInGroups);
        }

        static (int, int, double) FindShortestPairwiseDistanceCoordinatesAndValues(double[,] distanceMatrix)
        {
            double shortestDistanceValue = distanceMatrix[1, 0];
            (int, int) shortestDistanceValueCoordinates = (1, 0);

            for (int i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (distanceMatrix[i, j] < shortestDistanceValue)
                    {
                        shortestDistanceValue = distanceMatrix[i, j];
                        shortestDistanceValueCoordinates = (i, j);
                    }
                }
            }

            return (shortestDistanceValueCoordinates.Item1, shortestDistanceValueCoordinates.Item2, shortestDistanceValue);
        }

        static List<List<int>> UpdateSequencesGroups(List<List<int>> idsOfSequencesInGroups, (int, int) shortestDistanceValueCoordinates)
        {
            List<List<int>> newIdsOfSequencesInGroups = new List<List<int>>();
            int idOfLowerCoordinate = shortestDistanceValueCoordinates.Item2 >= shortestDistanceValueCoordinates.Item1 ?
                shortestDistanceValueCoordinates.Item1 : shortestDistanceValueCoordinates.Item2;
            int idOfGreaterCoordinate = shortestDistanceValueCoordinates.Item1 == idOfLowerCoordinate ?
                shortestDistanceValueCoordinates.Item2 : shortestDistanceValueCoordinates.Item1;

            for (int i = 0; i < idsOfSequencesInGroups.Count; i++)
            {
                if (i != shortestDistanceValueCoordinates.Item1 && i != shortestDistanceValueCoordinates.Item2)
                {
                    newIdsOfSequencesInGroups.Add(idsOfSequencesInGroups[i]);
                }
                else if (i == idOfLowerCoordinate)
                {
                    newIdsOfSequencesInGroups.Add(
                        idsOfSequencesInGroups[i].Concat(
                            idsOfSequencesInGroups[idOfGreaterCoordinate]).ToList());
                }
            }

            return newIdsOfSequencesInGroups;
        }

        static double[,] CreateNewDistanceMatrix(double[,] actualDistanceMatrix, List<List<int>> idsOfSequencesInGroups,
            double[,] originalDistanceMatrix)
        {
            int newLength = actualDistanceMatrix.GetLength(0) - 1;
            double[,] result = new double[newLength, newLength];

            for (int i = 0; i < newLength; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    result[i, j] = CalculateValueForDistanceMatrixCell(idsOfSequencesInGroups,
                        originalDistanceMatrix, (i,j));
                }
            }

            return result;
        }

        static double CalculateValueForDistanceMatrixCell(List<List<int>> idsOfSequencesInGroups,
            double[,] originalDistanceMatrix, (int, int) cellCoordinates)
        {
            double sum = 0;
            int numberOfElements = 0;


            for (int i = 0; i < idsOfSequencesInGroups.ElementAt(cellCoordinates.Item1).Count; i++)
            {
                int indexOfFirstElement = idsOfSequencesInGroups.ElementAt(cellCoordinates.Item1).ElementAt(i);

                for (int j = 0; j < idsOfSequencesInGroups.ElementAt(cellCoordinates.Item2).Count; j++)
                {
                    int indexOfSecondElement = idsOfSequencesInGroups.ElementAt(cellCoordinates.Item2).ElementAt(j);
                    sum += indexOfFirstElement > indexOfSecondElement ?
                        originalDistanceMatrix[indexOfFirstElement, indexOfSecondElement] :
                        originalDistanceMatrix[indexOfSecondElement, indexOfFirstElement];
                    numberOfElements++;
                }
            }

            return sum / numberOfElements;
        }
    }
}
