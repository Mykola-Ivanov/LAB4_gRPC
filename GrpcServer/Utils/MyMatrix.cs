using System;
using System.Collections.Generic;
using System.Text;

namespace GrpcServer.Utils
{
    class MyMatrix
    {
        public int rows, cols;
        public float[] matrix;

        public static void setC2(ref MyMatrix c2)
        {
            for (int i = 0; i < c2.cols; i++)
            {
                for (int j = 0; j < c2.rows; j++)
                {
                    c2.matrix[i * c2.rows + j] = 2.0f / (i + j + 4);
                }
            }
        }
        public static void setb(ref MyMatrix b)
        {
            for (int i = 0; i < b.rows; i++)
            {
                b.matrix[i] = 8.0f / (i + 1);
            }
        }
        public static void randomize(ref MyMatrix Matrix)
        {
            for (int i = 0; i < Matrix.cols; i++)
            {
                for (int j = 0; j < Matrix.cols; j++)
                {
                    Matrix.matrix[i * Matrix.rows + j] = (float)(new Random().NextDouble() * 0.5);
                }
            }
        }
        public MyMatrix(int cols, int rows)
        {
            matrix = new float[rows * cols];
            this.cols = cols;
            this.rows = rows;
        }
        public MyMatrix(int n)
        {
            matrix = new float[n * n];
            cols = n;
            rows = n;
        }
        public float[] GetRow(int index)
        {
            if (index < 0 || index >= rows)
            {
                return null;
            }
            else
            {
                float[] row = new float[cols];
                for (int i = 0; i < cols; i++)
                {
                    row[i] = matrix[index * rows + i];
                }
                return row;
            }
        }
        public MyMatrix Transposition()
        {
            MyMatrix m = new MyMatrix(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    m.matrix[i * cols + j] = matrix[j * rows + i];
                }
            }
            return m;
        }
        public MyMatrix Multiply(float multiply)
        {
            MyMatrix result = new MyMatrix(this.cols, this.rows);

            for (int i = 0; i < this.rows; i++)
            {
                for (int j = 0; j < this.cols; j++)
                {
                    result.matrix[i * cols + j] = this.matrix[i * cols + j] * multiply;
                }
            }

            return result;
        }
        public MyMatrix Multiply(MyMatrix other)
        {
            MyMatrix result = new MyMatrix(other.cols, this.rows);

            for (int i = 0; i < this.rows; i++)
            {
                for (int j = 0; j < other.cols; j++)
                {
                    float s = 0;
                    for (int k = 0; k < other.rows; k++)
                    {
                        s += this.matrix[i * cols + k] * other.matrix[k * other.cols + j];
                    }
                    result.matrix[j * rows + i] = s;
                }
            }

            return result;
        }
        public MyMatrix Add(MyMatrix other)
        {
            MyMatrix result = new MyMatrix(this.cols, this.rows);

            for (int i = 0; i < this.rows; i++)
            {
                for (int j = 0; j < other.cols; j++)
                {
                    result.matrix[i * cols + j] = this.matrix[i * cols + j] + other.matrix[i * cols + j];
                }
            }

            return result;
        }

        public void Show()
        {
            Console.WriteLine();

            for (int i = 0; i < this.rows; i++)
            {
                for (int j = 0; j < this.cols; j++)
                {
                    Console.Write(matrix[i * cols + j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
