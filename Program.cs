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
            string distanceFileName = "distances.csv";
            string[] fileNames = new string[] { "plik2.csv", "plik1.csv" };
            List<char> alphabet = new List<char> { 'A', 'G', 'T', 'C', '-' };
            List<double[,]> resultProfiles = new List<double[,]>();
            List<List<char>> alphabets = new List<List<char>>();
            List<List<string>> matrices = new List<List<string>>();
            Dictionary<(char, char), double> similarity = GetMatrix(matrixFileName);
            Dictionary<(char, char), double> distances = GetMatrix(distanceFileName);
            GuideTree guideTree;
            List<string> allSequences;
            double[,] distanceMatrix;

            try
            {
                foreach (var fileName in fileNames)
                {
                    matrices.Add(CreateMatrix(fileName));
                    var profile = CreateProfile(matrices.Last(), alphabet);
                    resultProfiles.Add(profile);
                    PrintProfileMatrix(profile, alphabet);
                    var consensusWord = CreateConsensusWord(profile, alphabet);
                    Console.WriteLine($"\nSłowo konsensusowe: {consensusWord}");
                }
                var concatenated = ConcatSequences(resultProfiles.First(),
                                resultProfiles.Last(),
                                matrices.First().Select(s => s.ToCharArray()).ToArray(),
                                matrices.Last().Select(s => s.ToCharArray()).ToArray(),
                                alphabet,
                                similarity);
                Console.WriteLine("\nZłożenie wielodopasowań");
                foreach (var s in concatenated)
                {
                    Console.WriteLine(s);
                }

                allSequences = matrices.SelectMany(x => x).ToList();
                PrintLegend(allSequences);

                distanceMatrix = CreateDistanceMatrix(allSequences, distances);
                Console.WriteLine("\nDistance matrix:");
                PrintDistances(distanceMatrix);

                guideTree = CreateGuideTreeWithUPGMA(distanceMatrix, allSequences);
                Console.WriteLine("\nGuide tree:");
                guideTree.PrintTree();

                var treeRootNodeAlligments = GetAlligmentsFromRootNode(guideTree.rootNode, alphabet, similarity);
                Console.WriteLine("\nZłożenie wielodopasowań");
                foreach (var alligment in treeRootNodeAlligments)
                {
                    Console.WriteLine(alligment);
                }
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

        private static Dictionary<(char, char), double> GetMatrix(string fileName)
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
                                                Dictionary<(char, char), double> similarity,
                                                bool scoreMatrixShouldBePrinted = true)
        {
            var result_length = multi1.Length + multi2.Length;
            var result = Enumerable.Repeat(string.Empty, result_length).ToArray();
            int i = profile1.GetLength(1);
            int j = profile2.GetLength(1);
            var scoreMatrix = CreateScoreMatrix(profile1, profile2, alphabet, similarity);

            if (scoreMatrixShouldBePrinted)
            {
                PrintScoreMatrix(scoreMatrix);
            }
            
            while(i>0 || j > 0)
            {
                var newi = i;
                var newj = j;
                switch (scoreMatrix[i, j])
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

        private static char[,] CreateScoreMatrix(
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
            return result;
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

        static double[,] CreateDistanceMatrix(List<string> sequences, Dictionary<(char, char), double> distances)
        {
            var result = new double[sequences.Count, sequences.Count];
            
            for (int i = 0; i < sequences.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    result[i,j] = CountDistanceMatrixCellValue(sequences[i], sequences[j], distances);
                }
            }
            return result;
        }

        static double CountDistanceMatrixCellValue(string firstSequence, string secondSequence,
            Dictionary<(char, char), double> distances)
        {
            double[,] valueMatrix = new double[firstSequence.Length + 1, secondSequence.Length + 1];
            List<double> results;
            double increase;

            valueMatrix[0, 0] = 0;

            for (int i = 0; i <= firstSequence.Length; i++)
            {
                valueMatrix[i, 0] = i;
            }

            for (int i = 0; i <= secondSequence.Length; i++)
            {
                valueMatrix[0, i] = i;
            }

            for (int i = 1; i <= firstSequence.Length; i++)
            {
                for (int j = 1; j <= secondSequence.Length; j++)
                {
                    results = new List<double>();

                    increase = distances[(firstSequence.ElementAt(i - 1), secondSequence.ElementAt(j - 1))];
                    results.Add(valueMatrix[i - 1, j - 1] + increase);
                    results.Add(valueMatrix[i - 1, j] + 1);
                    results.Add(valueMatrix[i, j - 1] + 1);
                    
                    valueMatrix[i, j] = results.Min();
                } 
            }

            return valueMatrix[firstSequence.Length, secondSequence.Length];
        }

        static void PrintLegend(List<string> sequences)
        {
            Console.WriteLine("\nLegenda:");
            char firstAlias = 'A';

            for (int i = 0; i < sequences.Count; i++)
            {
                Console.WriteLine((char)(firstAlias + i) + ": " + sequences[i]);
            }
        }

        static void PrintDistances(double[,] distanceMatrix)
        {
            char firstAlias = 'A';
            Console.Write("\t");

            for (int i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                Console.Write((char)(firstAlias + i) + "\t");
            }

            for (int i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                Console.Write("\n" + (char)(firstAlias + i) + "\t");
                for (int j = 0; j < i; j++)
                {
                    Console.Write(distanceMatrix[i, j] + "\t");
                }
            }
            Console.WriteLine();
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

        static List<string> GetAlligmentsFromRootNode(GuideTreeNode guideTreeNode, List<char> alphabet, Dictionary<(char, char), double> similarity)
        {
            if (guideTreeNode.rightChild == null && guideTreeNode.leftChild == null)
            {
                List<string> result = new List<string>();
                result.Add(guideTreeNode.name);
                return result;
            }

            List<string> alligmentsFromLeftChild = GetAlligmentsFromRootNode(guideTreeNode.leftChild, alphabet, similarity);
            List<string> alligmentsFromRightChild = GetAlligmentsFromRootNode(guideTreeNode.rightChild, alphabet, similarity);

            return guideTreeNode.alligments = SetAlligmentForTreeNode(alligmentsFromLeftChild, alligmentsFromRightChild, alphabet, similarity);
        }

        static List<string> SetAlligmentForTreeNode(List<string> leftSubtreeAlligments, List<string> rightSubtreeAlligments,
            List<char> alphabet, Dictionary<(char, char), double> similarity)
        {
            double[,] firstProfile = CreateProfile(leftSubtreeAlligments, alphabet);
            double[,] secondProfile = CreateProfile(rightSubtreeAlligments, alphabet);
            var multi1 = leftSubtreeAlligments.ToArray().Select(x => x.ToCharArray()).ToArray();
            var multi2 = rightSubtreeAlligments.ToArray().Select(x => x.ToCharArray()).ToArray();
            return ConcatSequences(firstProfile, secondProfile, multi1, multi2, alphabet, similarity, false).ToList();
        }
    }
}
