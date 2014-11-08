using System.Collections.Generic;

namespace ScriptCs
{
    public class ScriptEnvironment
    {
        public ScriptEnvironment(string[] scriptArgs)
        {
            ScriptArgs = scriptArgs;
        }

        public IEnumerable<string> ScriptArgs { get; private set; }
    }
}