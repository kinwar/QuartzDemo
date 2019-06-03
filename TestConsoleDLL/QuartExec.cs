using Quartz;
using System;
using System.Threading.Tasks;

namespace TestConsoleDLL
{
    public class QuartExec: IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var _task = Task.Run(()=>GetHelloWord());

            return _task;
        }

        private string GetHelloWord()
        {
            return "HelloWord";
        }
    }
}
