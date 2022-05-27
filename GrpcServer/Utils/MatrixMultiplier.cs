using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GrpcServer.Utils
{
    enum Operation :ushort
    {
        NO_OPERATION,
        ADD,
        MULTIPLY
    };
    enum OperandState
    {
        NONE,       //ДО ОТРИМАННЯ ДАНИХ
        NOT_READY,   //ПІСЛЯ ПОЧАТКУ ОТИМАННЯ ДАНИХ
        READY,      //ПІСЛЯ ОРИМАННЯ ДАНИХ
    }
    class MatrixsegmentData
    {
        public int begin_col_index, end_col_index,begin_row_index,end_row_index;
    }
    class MatrixOperation
    {
        MyMatrix[] operands;
        MyMatrix result;
        OperandState operands_status;
        OperandState result_status;
        Operation operation_type;
        public Thread calcThread;
        public MatrixOperation()
        {
            operands_status = OperandState.NONE;
            operation_type = Operation.NO_OPERATION;
        }

        public void SetOperation(Operation op)
        {
            operation_type = op;
        }
        public void SetOperands(MyMatrix[] matrix)
        {
            operands= matrix;
            operands_status = OperandState.READY;
            result_status = OperandState.NONE;
        }
        public MyMatrix GetResult()
        {
            result_status = OperandState.NONE;
            return result;
        }
        void Calculate()
        {
            int index = 2;
            MyMatrix prev_result =new MyMatrix(1,1);
            if (operation_type == Operation.MULTIPLY)
                prev_result = operands[0].Multiply(operands[1]);
            else if (operation_type == Operation.ADD)
            {
                prev_result = operands[0].Add(operands[1]);
            }else return;
            
            while(index < operands.Length)
            {
                if (operation_type == Operation.MULTIPLY)
                    prev_result = prev_result.Multiply(operands[1]);
                else if (operation_type == Operation.ADD)
                {
                    prev_result = prev_result.Add(operands[1]);
                }
                index++;
            }
            result = prev_result;

            result_status = OperandState.READY;

            operands_status = OperandState.NONE;
            operation_type = Operation.NO_OPERATION;
            return ;
        }
        public void Startup()
        {
            result_status = OperandState.NOT_READY;
            
            calcThread = new Thread(Calculate);
            calcThread.Start();
            
            //calcThread.Join();

            
            
            
        }
    }
}
