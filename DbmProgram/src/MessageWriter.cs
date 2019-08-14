using System;
using System.Collections.Generic;
using System.Text;

namespace DBMProgram.src
{
    public interface IMessageWriter
    {
        void WriteMessage(string message);
        void WriteError(string error);
    }

    public class ConsoleMessageWriter : IMessageWriter
    {
        public void WriteError(string error)
        {
            Console.Error.WriteLine(error);
        }
        public void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class NullMessageWriter : IMessageWriter
    {
        public void WriteError(string error)
        {
            throw new NotImplementedException();
        }
        public void WriteMessage(string message)
        {
            throw new NotImplementedException();
        }
    }

    public class AwsCloudWatchMessageWriter : IMessageWriter
    {
        public void WriteError(string error)
        {
            throw new NotImplementedException();
        }
        public void WriteMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}
