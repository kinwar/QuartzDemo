using ConsoleQuartz.Common;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleQuartz
{
    class Program
    {
        private static int time = 0;
        /// <summary>
        /// 获取一个随机数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static async Task<int> GetRandomAsync()
        {
            time++;
            var t1 = Task.Run(() =>
            {
                Thread.Sleep(time * 1000);
                return new Random().Next();
            });
            time++;
            var t2 = Task.Run(() =>
            {
                Thread.Sleep(time * 1000);
                return new Random().Next();
            });
            Console.WriteLine($"执行完毕，等待结果");
            //异步等待集合内的 Task 都完成，才进行下一步操作
            await Task.WhenAll(new List<Task<int>>() { t1, t2 });
            //Console.WriteLine($"    t1.{nameof(t1.IsCompleted)}: {t1.IsCompleted}");
            //Console.WriteLine($"    t2.{nameof(t2.IsCompleted)}: {t2.IsCompleted}");
            Console.WriteLine($"执行完毕，已计算完成结果");
            return t1.Result + t2.Result;
        }

        public static Program Instance { get; } = new Program();

        public static Log logger = new Log();

        private static ILog stcLogger = new NamedLog("statistic");

        private List<string> ParseList(string str, char splitTag)
        {
            var data = new List<string>();

            var strSplit = splitTag.ToString();
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(strSplit))
            {
                data = str.Split(new char[] { splitTag }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return data;
        }
        static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now} 执行开始...");
            //string strValue = "1,2,3,4,";
            //var list = Instance.ParseList(strValue, ',');

            //0 / 5 * *** ? 每隔5秒执行
            //0 0 / 1 * ** ? 每隔一分钟从0开始
            //0 3 / 20 * ** ? 小时中从第3分钟开始，每隔20分钟执行
            //0 15 10 ? *MON - FRI  在星期一到星期五的每天上午10点15分执行
            //0 15 10 ? *6L 2007 - 2010
            //在2007年到2010年的每个月的最后一个星期五上午10点15分执行

            //TestSample();
            //var t1 = StartJobAsync();  

            //Assembly assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "Midea.MES.Kanban.Plugin.RptOutputDay.dll");
            //Assembly assembly = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + @"Midea.MES.Kanban.Plugin.RptOutputDay.dll");
            //var types = assembly.GetType("Midea.MES.Kanban.Plugin.RptOutputDay.Workers.RptOutputDayWorker");

            //Assembly assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "TestConsoleDLL.dll");
            //var types = assembly.GetType("TestConsoleDLL.QuartExec");

            
            var props = new NameValueCollection
            {
                //{ "quartz.serializer.type", "binary" },
                { "quartz.threadPool.ThreadCount", "1000" }
            };
            var factory = new StdSchedulerFactory(props);
            var sched = factory.GetScheduler().Result;
            sched.Start();
            var job = JobBuilder.Create(typeof(DemoJob))
                .WithIdentity("myJob", "group1")
                .UsingJobData("TestParamA", "测试参数A")
                .Build();

            //job.JobDataMap.Add("TaskParam", "测试参数"); 0/5 * * * * ?

            var strExp = "";
            try
            {
                strExp = GetCronExpBySecond(3600*24*2);
                //strExp = $"0 0 0 */31 * ?";

            }
            catch (Exception ex)
            {
                var str = ex.ToString();
                var strMsg = ex.Message;
                strExp = $"0 0/5 * * * ?";
            }

            Console.WriteLine(strExp);

            //var strExp = $"0 0/{_minute} * * * ?";//每隔5分钟
            //var strExp = $"1 1 * * * ?";//每隔5分钟
            var cronExp = CronScheduleBuilder.CronSchedule(strExp);


            //CronScheduleBuilder cronExp = CronScheduleBuilder.CronSchedule("0/5 * * * * ?");

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .WithSchedule(cronExp)
                //.WithCalendarIntervalSchedule((b) => b.WithInterval(5, IntervalUnit.Second))
                //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(11, 07))//上午10:40//.WithSchedule(CronScheduleBuilder.CronSchedule("0 45 10 * * ?"))//上午10:40
                //.WithSchedule(CronScheduleBuilder.CronSchedule("0 0/2 8-17 * * ?"))//每隔2分钟，每天上午8点至下午5点之间
                //.WithSimpleSchedule(x => x
                //    .WithIntervalInSeconds(61)
                //    .RepeatForever())
                .StartNow()
                .Build();

            sched.ScheduleJob(job, trigger);

            //var job2 = JobBuilder.Create(typeof(DemoJob))
            //   .WithIdentity("myJob2", "group2")
            //   .UsingJobData("TestParamA", "测试参数B")
            //   .Build();
            //ITrigger trigger2 = TriggerBuilder.Create()
            //    .WithIdentity("myTrigger2", "group2")
            //    .StartNow()
            //    .Build();
            //sched.ScheduleJob(job2, trigger2);
            //Console.WriteLine($"任务结束...");

            Console.WriteLine($"{DateTime.Now} 执行完毕...");
            Console.ReadKey();
        }

        private static async Task<DateTimeOffset> CreateJobAsync(string typeFullName, CronScheduleBuilder cronExp)
        {
            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            var factory = new StdSchedulerFactory(props);
            var sched = factory.GetScheduler().Result;
            await sched.Start();
            var job = JobBuilder.Create(typeof(DemoJob))
                .WithIdentity("myJob", "group1")
                .UsingJobData("TestParamA", "测试参数A")
                .Build();

            job.JobDataMap.Add("TaskParam", "测试参数");
            
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .WithSchedule(cronExp)
                .Build();

            return await sched.ScheduleJob(job, trigger);
        }

        private static async Task StartJobAsync()
        {
            var props = new NameValueCollection
{
    { "quartz.serializer.type", "binary" }
};
            var factory = new StdSchedulerFactory(props);
            var sched = await factory.GetScheduler();
            await sched.Start();
            var job = JobBuilder.Create<DemoJob>()
                .WithIdentity("myJob", "group1")
                .UsingJobData("TestParamA", "测试参数A")
                .Build();
            //或
            //Type type = Type.GetType("ConsoleQuartz.DemoJob");
            //IJobDetail job = new JobDetailImpl("myJob", type);
            //添加任务执行参数
            job.JobDataMap.Add("TaskParam", "测试参数");

            var trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(5)
                    .RepeatForever())
            .Build();

            await sched.ScheduleJob(job, trigger);
        }


        private static void TestSample()
        {
            Type t = Type.GetType("ConsoleQuartz.DynamicSample");

            object obj = Activator.CreateInstance(t);
            var addMethod = obj.GetType().GetMethod("Add");
            var re = (string)addMethod.Invoke(obj, new object[] { 1, 2 });
            
            Console.WriteLine(re);
            Console.Read();
        }

        private static string GetCronExpBySecond(int seconds)
        {
            var strExp = $"0 0/5 * * * ?";//每隔5分钟

            var ts = new TimeSpan(0, 0, seconds);

            double _day = ts.Days;
            double _hour = ts.Hours;
            double _minute = ts.Minutes;
            double _second = ts.Seconds;

            var ex_day = "*";
            var ex_hour = "*";
            var ex_minute = "*";
            var ex_second = "*";

            try
            {
                if (_day > 0)
                {
                    if (_day > 31) _day = 31;
                    ex_day = $"*/{_day}";
                }

                if (_hour > 0)
                {
                    if (_day > 0)
                        ex_hour = $"{_hour}";
                    else
                        ex_hour = $"0/{_hour}";
                }
                else
                {
                    if (_day > 0)
                        ex_hour = "0";
                }
                //分
                if (_minute > 0)
                {
                    if (_hour > 0 || _day > 0)
                        ex_minute = $"{_minute}";
                    else 
                        ex_minute = $"0/{_minute}";
                }
                else
                {
                    if (_hour > 0 || _day > 0)
                        ex_minute = "0";
                }

                if (_second > 0)
                {
                    if (_minute > 0 || _hour > 0 || _day > 0)
                        ex_second = $"{_second}";
                    else
                        ex_second = $"0/{_second}";
                }
                else
                {
                    if (_minute > 0 || _hour > 0 || _day > 0)
                        ex_second = "0";
                }
                strExp = $"{ex_second} {ex_minute} {ex_hour} {ex_day} * ?";

                return strExp;
            }
            catch (Exception ex)
            {
                throw new Exception("内部错误", ex);
            }

        }
    }

    public class DemoJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            //Console.WriteLine($"执行参数A...{context.JobDetail.JobDataMap.Get("TestParamA").ToString()}");
            Console.WriteLine($"{DateTime.Now} 执行参数...{context.JobDetail.JobDataMap.Get("TestParamA").ToString()}");
            var _task = Task.Run(() => GetHelloWord());
            //var _task = new Task(() => GetHelloWord());
            return _task;
        }


        private string GetHelloWord()
        {
            Console.WriteLine($"执行中...");
            return "HelloWord";
        }

    }

    public class DynamicSample
    {
        public static DynamicSample Instance { get; } = new DynamicSample();

        public string Name { get; set; }

        public string Add(int a, int b)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var sum = 0;
            for (var i = 0; i < 999000000; i++)
            {
                sum++;
            }
            sw.Stop();
            var ts = sw.Elapsed;
            var time = Math.Round(ts.TotalMilliseconds / 1000, 2).ToString() + "秒";

            return time;
        }
    }
}
