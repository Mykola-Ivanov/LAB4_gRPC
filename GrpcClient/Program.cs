using Grpc.Net.Client;
using GrpcClient.Utils;
using GrpcServer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static MyMatrix getMatrixFromReply(ref ReplyMatrix replyMatrix)
        {
            MyMatrix myMatrix = new MyMatrix(replyMatrix.Cols, replyMatrix.Rows);

            for (int i = 0; i < myMatrix.rows; i++)
            {
                for (int j = 0; j < myMatrix.cols; j++)
                {
                    myMatrix.matrix[i * myMatrix.cols + j] = replyMatrix.Row[i * myMatrix.cols + j] ;
                }
            }

            return myMatrix;
        }
        static async Task Main(string[] args)
        {
            Stopwatch time = new Stopwatch();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Введіть розмір матриць");

            int n =Convert.ToInt32( Console.ReadLine());
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new GrpcServer.MatrixOperator.MatrixOperatorClient(channel);
            //new Greeter.GreeterClient(channel);

            time.Restart();
            var reply = await client.CreateMatricesAsync(new MatrixesDimensionRequest { Size = n }).ResponseAsync;
            //.SayHelloAsync(new HelloRequest { Name = "Mykola" });
            
            Console.WriteLine($"Матриці успішно створенні {reply.Count}");

            MyMatrix y1 = new MyMatrix(n);
            ReplyMatrix replyy1 =await client.GetmatrixAsync(new RequestMatrix { Id = 1 });

            y1 = getMatrixFromReply(ref replyy1);
            Console.WriteLine("y1 успішно отримано");
            y1.Show();

            MyMatrix y2 = new MyMatrix(n);
            ReplyMatrix replyy2 = await client.GetmatrixAsync(new RequestMatrix { Id = 2 });
            
            y2 = getMatrixFromReply(ref replyy2);
            Console.WriteLine("\ny2 успішно отримано");
            y2.Show();

            MyMatrix Y3 = new MyMatrix(n);
            ReplyMatrix replyy3 = await client.GetmatrixAsync(new RequestMatrix { Id = 3 });
            
            Y3 = getMatrixFromReply(ref replyy3);
            Console.WriteLine("\ny3 успішно отримано");
            Y3.Show();

            GrpcClient.Utils.MatrixOperation[] operations = new Utils.MatrixOperation[5];

            operations[0] = new MatrixOperation();
            operations[0].SetOperation(Operation.MULTIPLY);
            operations[0].SetOperands(new MyMatrix[] { y1.Transposition(),Y3,y1,y2,y2.Transposition()});
            operations[0].Startup();


            operations[1] = new MatrixOperation();
            operations[1].SetOperation(Operation.MULTIPLY);
            operations[1].SetOperands(new MyMatrix[] { Y3, Y3 });
            operations[1].Startup();


            operations[2] = new MatrixOperation();
            operations[2].SetOperation(Operation.MULTIPLY);
            operations[2].SetOperands(new MyMatrix[] { y1, y2.Transposition() });
            operations[2].Startup();
            ////////////////////////////////////////////
            operations[0].calcThread.Join();
            operations[1].calcThread.Join();
            operations[2].calcThread.Join();
            
            operations[3] = new MatrixOperation();
            operations[3].SetOperation(Operation.ADD);
            operations[3].SetOperands(new MyMatrix[] { operations[0].GetResult(), operations[1].GetResult(), operations[2].GetResult() }) ;
            operations[3].Startup();

            ///////////////////////////////////////////
            operations[3].calcThread.Join();
            MyMatrix res1 = operations[3].GetResult();
            MyMatrix res = new MyMatrix(n);
            ReplyMatrix replyres = await client.GetmatrixAsync(new RequestMatrix { Id = 0 });
            
            res = getMatrixFromReply(ref replyres);
            Console.WriteLine("res успішно отримано");

            operations[4] = new MatrixOperation();
            operations[4].SetOperation(Operation.MULTIPLY);
            operations[4].SetOperands(new MyMatrix[] { res1, res.Transposition() }) ;
            operations[4].Startup();

            operations[4].calcThread.Join();
            time.Stop();
            MyMatrix result = operations[4].GetResult();
            Console.WriteLine("\nРезультат обрахунків");
            result.Show();
            Console.WriteLine($"обрахунки закінчено за {time.Elapsed.TotalSeconds} s");

            Console.ReadLine();
        }
    }
}
