﻿using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ConsoleApp2
{
  class MatrixOperations
  {
    public static void print(int[,] len)
    {
      for (int i = 0; i < len.GetLength(0); i++)
      {
        for (int j = 0; j < len.GetLength(1); j++)
        {
          Console.Write($"{len[i, j]} ");
        }
        Console.WriteLine();
      }
      Console.WriteLine();
    }
    
    public static int[,] generateMatrix(int n, int m)
    {
      int[,] matrix = new int[n, m];
      Random random = new Random();
      
      for (int i = 0; i < n; i++)
      {
        for (int j = 0; j < m; j++)
        {
          matrix[i, j] = random.Next(0, 100);
        }
      }
      return matrix;
    }
    
    public static TimeSpan AddMatrices(int[,] matrixA, int[,] matrixB)
    {
      if (matrixA.GetLength(0) != matrixB.GetLength(0) || matrixA.GetLength(1) != matrixB.GetLength(1))
      {
        throw new ArgumentException("Матриці повинні мати однаковий розмір.");
      }

      int rows = matrixA.GetLength(0);
      int cols = matrixA.GetLength(1);

      int[,] result = new int[rows, cols];

      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();

      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < cols; j++)
        {
          result[i, j] = matrixA[i, j] + matrixB[i, j];
        }
      }

      stopWatch.Stop();
      return stopWatch.Elapsed;
    }

    public static TimeSpan SubMatrices(int[,] matrixA, int[,] matrixB)
    {
      if (matrixA.GetLength(0) != matrixB.GetLength(0) || matrixA.GetLength(1) != matrixB.GetLength(1))
      {
        throw new ArgumentException("Матриці повинні мати однаковий розмір.");
      }

      int rows = matrixA.GetLength(0);
      int cols = matrixA.GetLength(1);

      int[,] result = new int[rows, cols];

      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();

      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < cols; j++)
        {
          result[i, j] = matrixA[i, j] - matrixB[i, j];
        }
      }

      stopWatch.Stop();
      return stopWatch.Elapsed;
    }

    public static void AddMatrixRows(int[,] matrixA, int[,] matrixB, int[,] result, int startRow, int endRow)
    {
      for (int i = startRow; i < endRow; i++)
      {
        for (int j = 0; j < matrixA.GetLength(1); j++)
        {
          result[i, j] = matrixA[i, j] - matrixB[i, j];
        }
      }
    }

    public static TimeSpan AddMatricesParallel(int[,] matrixA, int[,] matrixB, int threadCount)
    {
      if (matrixA.GetLength(0) != matrixB.GetLength(0) || matrixA.GetLength(1) != matrixB.GetLength(1))
      {
        throw new ArgumentException("Матриці повинні мати однаковий розмір.");
      }

      int rows = matrixA.GetLength(0);
      int cols = matrixA.GetLength(1);
      int[,] result = new int[rows, cols];

   

      Thread[] threads = new Thread[threadCount];
      int rowsPerThread = rows / threadCount;
      int remainingRows = rows % threadCount;
      
      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();
      
      for (int i = 0; i < threadCount; i++)
      {
        int startRow = i * rowsPerThread;
        int endRow = (i + 1) * rowsPerThread;

        if (i == threadCount - 1)
        {
          endRow += remainingRows;
        }

        threads[i] = new Thread(() => AddMatrixRows(matrixA, matrixB, result, startRow, endRow));
        threads[i].Start();
      }

      stopWatch.Stop();
      return stopWatch.Elapsed;
    }
    
    static void Main(string[] args)
    {
      int n = 10000;
      int m = 10000;
      int threadCount = 10;

      var m1 = generateMatrix(n, m);
      var m2 = generateMatrix(n, m);

      Console.WriteLine($"n = {n} m = {m} threads = {threadCount}");

      var syncMethod = SubMatrices(m1, m2);
      Console.WriteLine($"Sequential method: {syncMethod.Ticks} tk");

      var parallel = AddMatricesParallel(m1, m2, threadCount);
      Console.WriteLine($"Parallel method: {parallel.Ticks} tk");

      var acceleration = syncMethod / parallel;
      var efficiency = acceleration / threadCount;

      Console.WriteLine($"Acceleration: {acceleration}");
      Console.WriteLine($"Efficiency: {efficiency}");
    }
  }
}
