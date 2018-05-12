using System;
using System.Collections.Generic;
using System.Linq;
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
            return Parser.Parse(script).FunctionDeclarations.Select(x => x.Id.Name).ToList();
        }
        /// <summary>
        /// Set a script variable to a value
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">value to assign</param>
        /// <returns></returns>
        public IScriptEngine SetValue(string name, object value)
        {
            Engine.SetValue(name, value);
            return this;
        }
        /// <summary>
        /// Get a script variable value
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <returns></returns>
        public object GetValue(string name)
        {
            return Engine.GetValue(name).ToObject();
        }
        /// <summary>
        /// Execute a script
        /// </summary>
        /// <param name="script">Script code to execute</param>
        /// <returns></returns>
        public IScriptEngine Execute(string script)
        {
            Engine.Execute(script);
            return this;
        }
        /// <summary>
        /// Execute a single function already loaded into the engine
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public IScriptEngine Invoke(string name, object argument)
        {
            Engine.Invoke(name, argument);
            return this;
        }
    }
}
