using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Engines
{
    /// <summary>
    /// Parse and execute item javascript
    /// </summary>
    public class JsScriptEngine : IScriptEngine
    {
        protected Jint.Parser.JavaScriptParser Parser { set; get; }
        protected Jint.Engine Engine { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public JsScriptEngine()
        {
            Parser = new Jint.Parser.JavaScriptParser();
            Engine = new Jint.Engine();
        }
        /// <summary>
        /// Get all function declarations in a script
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public List<string> GetFunctionDeclarations(string script)
        {
            var functions = new List<string>();

            return functions;
        }
    }
}
