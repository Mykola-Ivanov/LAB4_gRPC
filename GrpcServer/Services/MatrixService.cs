using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using GrpcServer.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcServer
{
    public class MatrixService : MatrixOperator.MatrixOperatorBase
    {
        private readonly ILogger<MatrixService> _logger;
        public Thread calcThread;
        public Thread prepThread;

        GrpcServer.Utils.MatrixOperation[] operations;
        static bool y1Ready;
        static MyMatrix y1;
        static bool y2Ready;
        static MyMatrix y2;
        static bool y3Ready;
        static MyMatrix Y3;
        static MyMatrix result;
        int size;

        public MatrixService(ILogger<MatrixService> logger)
        {
            _logger = logger;
        }

        
        private void Preapare()
        {
            MyMatrix A, A1, A2, b, b1, B2, c1, C2;
            A = new MyMatrix(size, size);
            A1 = new MyMatrix(size, size);
            A2 = new MyMatrix(size, size);
            B2 = new MyMatrix(size, size);
            C2 = new MyMatrix(size, size);
            b = new MyMatrix(1, size);
            b1 = new MyMatrix(1, size);
            c1 = new MyMatrix(1, size);


            MyMatrix.setC2(ref C2);
            MyMatrix.setb(ref b);
            MyMatrix.randomize(ref A);
            MyMatrix.randomize(ref A1);
            MyMatrix.randomize(ref A2);
            MyMatrix.randomize(ref B2);
            MyMatrix.randomize(ref b1);
            MyMatrix.randomize(ref c1);

            y1 = A.Multiply(b);
            y1Ready = true;
            y1.Show();
            y2 = A1.Multiply(b1.Multiply(2).Add(c1.Multiply(5)));
            y2Ready = true;
            y2.Show();

            Y3 = A2.Multiply(B2.Add(C2.Multiply(-1)));
            y3Ready = true;
            Y3.Show();
            calcThread = new Thread(StartupCalculation);
            calcThread.Start();
        }
        private void StartupCalculation()
        {
            {


                operations = new Utils.MatrixOperation[2];

                operations[0] = new MatrixOperation();
                operations[0].SetOperation(Operation.MULTIPLY);
                operations[0].SetOperands(new MyMatrix[]{y2.Transposition(),Y3,y2,y1 });
                operations[0].Startup();

                operations[0].calcThread.Join();

                operations[1] = new MatrixOperation();
                operations[1].SetOperation(Operation.ADD);
                operations[1].SetOperands(new MyMatrix[] { operations[0].GetResult(), y1 });
                operations[1].Startup();
                operations[1].calcThread.Join();
                result = operations[1].GetResult();
            }

        }
        public override async Task<MatrixesDimensionReply> CreateMatrices(MatrixesDimensionRequest request, ServerCallContext context)
        {
            if (prepThread == null)
            {
                size = request.Size;
                prepThread = new Thread(Preapare);
                prepThread.Start();
            }

            MatrixesDimensionReply reply = new MatrixesDimensionReply();
            reply.Count = 0;
            
            return await Task.FromResult(reply);//base.CreateMatrices(request, context);
        }
        public override Task<ReplyMatrix> Getmatrix(RequestMatrix request, ServerCallContext context)
        {
            ReplyMatrix reply = new ReplyMatrix();
            if (prepThread != null)
            {
                if (prepThread.ThreadState == ThreadState.Running && request.Id != 0) 
                {
                    prepThread.Join();
                    prepThread = null;
                }
            }
            if (calcThread !=null)
            {
                if (calcThread.ThreadState == ThreadState.Running && request.Id == 0)
                {
                    calcThread.Join();
                    calcThread = null;
                }
            }
            if (request.Id == 0)
            {
                reply.Cols = result.cols;
                reply.Rows = result.rows;
                reply.Row.AddRange(result.matrix.ToArray());
            }
            else if (request.Id == 1) 
            {
                reply.Cols = y1.cols;
                reply.Rows = y1.rows;
                reply.Row.AddRange(y1.matrix.ToArray());
            }else if (request.Id == 2 && y2Ready) 
            {
                reply.Cols = y2.cols;
                reply.Rows = y2.rows;
                reply.Row.AddRange(y2.matrix.ToArray());
            }
            else if (request.Id == 3 && y3Ready)
            {
                reply.Cols = Y3.cols;
                reply.Rows = Y3.rows;
                reply.Row.AddRange(Y3.matrix.ToArray());
            }
            
            return Task.FromResult(reply);
        }
    }
}
