using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Map.Scripting
{
    public abstract class MapScriptBuilder
    {
        internal StringBuilder Script;

        protected MapScriptBuilder()
        {
            Script = new StringBuilder();
        }

        public string Render()
        {
            return Script.ToString();
        }

        public void Clear()
        {
            Script = new StringBuilder();
        }
    }
}