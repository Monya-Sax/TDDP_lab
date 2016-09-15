﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Ccr.Core;
using System.Threading;

namespace ConsoleApplication2
{
    class Program
    {
        public class InputData
        {
            public int start; // начало диапазона 
            public int stop;  // начало диапазона 
        }


        static void Main(string[] args)
        {

            int[] A;//вектор а
            int[] B;//вектор b
            int[] C;//результат вычисления скалярного произведения векторов
            int m;
            int nc;

            nc = 2;//количество ядер
            m = 500000;//количество строк матрицы
            A = new int[m];
            B = new int[m];
            C = new int[m];
            Random r = new Random();
            for (int i = 0; i < m; i++)
            {
                A[i] = r.Next(100);

                B[i] = r.Next(100);
            }

            Stopwatch sWatch = new Stopwatch();//определение время выполнения вычислений для последовательного алгоритма
            sWatch.Start();
            for (int i = 0; i < m; i++)
            {
                C[i] = 0;
                C[i] += A[i] * B[i];
            }
            sWatch.Stop();
            Console.WriteLine("Последовательный алгоритм = {0} мс.",
            sWatch.ElapsedMilliseconds.ToString());
            //Console.ReadKey();


            // создание массива объектов для хранения параметров
            InputData[] ClArr = new InputData[nc];
            for (int i = 0; i < nc; i++)
                ClArr[i] = new InputData();

            // делим количество строк в матрице на nc частей 
            int step = (Int32)(m / nc);
            // заполняем массив параметров 
            int c = -1;
            for (int i = 0; i < nc; i++)
            {
                ClArr[i].start = c + 1;
                ClArr[i].stop = c + step;
                c = c + step;
            }

            Dispatcher d = new Dispatcher(nc, "Test Pool");//Создание диспетчера с пулом из двух потоков
            DispatcherQueue dq = new DispatcherQueue("Test Queue", d);
            Port<int> p = new Port<int>();

            for (int i = 0; i < nc; i++)
                Arbiter.Activate(dq, new Task<InputData, Port<int>>(ClArr[i], p, Mul));
            
            Arbiter.Activate(dq,Arbiter.MultipleItemReceive(true, p, nc, delegate(int[] array) //запуск задачи, обрабатывающей получение двух сообщений портом p
  {
      Console.WriteLine("Вычисления завершены");
  }));

        }

       static void Mul(InputData data, Port<int> resp)
        {

            int[] A;
            int[] B;
            int[] C;
            int m;
            

            m = 500000;
      
            A = new int[m];
            B = new int[m];
            C = new int[m];
            Random r = new Random();
            for (int i = 0; i < m; i++)
            {
                A[i] = r.Next(100);

                B[i] = r.Next(100);
            }

            Stopwatch sWatch = new Stopwatch();//определение время выполнения вычислений для параллельного алгоритма
            sWatch.Start();

            for (int i = data.start; i < data.stop; i++)
            {

                C[i] = 0;
                C[i] += A[i] * B[i];
            }
            sWatch.Stop();
            Console.WriteLine("Поток № {0}: Паралл. алгоритм = {1} мс.",
        Thread.CurrentThread.ManagedThreadId,
          sWatch.ElapsedMilliseconds.ToString());
            resp.Post(1);
        }

    }
}