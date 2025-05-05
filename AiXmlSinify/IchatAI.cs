using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiXmlSinify
{
    public interface IchatAI
    {
        public void Init(params string[] args);
        public Task<string> GetTranslationResults(string content);
    }
}
