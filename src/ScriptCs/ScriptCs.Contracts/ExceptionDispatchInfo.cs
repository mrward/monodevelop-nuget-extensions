using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptCs.Contracts
{
    public class ExceptionDispatchInfo
    {
        public Exception SourceException { get; private set; }

        public static ExceptionDispatchInfo Capture(Exception source)
        {
            return new ExceptionDispatchInfo
            {
                SourceException = source
            };
        }

        public void Throw()
        {
            throw SourceException;
        }
    }
}
