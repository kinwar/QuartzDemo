using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleQuartz
{
    /// <summary>
    /// 任务处理帮助类
    /// </summary>
    public class QuartzHelper
    {
        public QuartzHelper() { }

        public QuartzHelper(string quartzServer, string quartzPort)
        {
            Server = quartzServer;
            Port = quartzPort;
        }

        /// <summary>
        /// 锁对象
        /// </summary>
        private static readonly object Obj = new object();

        /// <summary>
        /// 方案
        /// </summary>
        private const string Scheme = "tcp";

        /// <summary>
        /// 服务器的地址
        /// </summary>
        public static string Server { get; set; }

        /// <summary>
        /// 服务器的端口
        /// </summary>
        public static string Port { get; set; }

        /// <summary>
        /// 缓存任务所在程序集信息
        /// </summary>
        private static readonly Dictionary<string, Assembly> AssemblyDict = new Dictionary<string, Assembly>();

        /// <summary>
        /// 程序调度
        /// </summary>
        private static IScheduler _scheduler;

        /// <summary>
        /// 初始化任务调度对象
        /// </summary>
        public static void InitScheduler()
        {
            try
            {
                lock (Obj)
                {
                    if (_scheduler != null) return;
                    //配置文件的方式，配置quartz实例
                    ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                    _scheduler = schedulerFactory.GetScheduler();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 启用任务调度
        /// 启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public static void StartScheduler()
        {
            try
            {
                if (_scheduler.IsStarted) return;
                //添加全局监听
                _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
                _scheduler.Start();

                //获取所有执行中的任务
                List<TaskModel> listTask = TaskHelper.GetAllTaskList().ToList();

                if (listTask.Count > 0)
                {
                    foreach (TaskModel taskUtil in listTask)
                    {
                        try
                        {
                            ScheduleJob(taskUtil);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(taskUtil.TaskName, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 启用任务
        /// <param name="task">任务信息</param>
        /// <param name="isDeleteOldTask">是否删除原有任务</param>
        /// <returns>返回任务trigger</returns>
        /// </summary>
        public static void ScheduleJob(TaskModel task, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
            {
                //先删除现有已存在任务
                DeleteJob(task.TaskID.ToString());
            }
            //验证是否正确的Cron表达式
            if (ValidExpression(task.CronExpressionString))
            {
                IJobDetail job = new JobDetailImpl(task.TaskID.ToString(), GetClassInfo(task.AssemblyName, task.ClassName));
                //添加任务执行参数
                job.JobDataMap.Add("TaskParam", task.TaskParam);

                CronTriggerImpl trigger = new CronTriggerImpl
                {
                    CronExpressionString = task.CronExpressionString,
                    Name = task.TaskID.ToString(),
                    Description = task.TaskName
                };
                _scheduler.ScheduleJob(job, trigger);
                if (task.Status == TaskStatus.STOP)
                {
                    JobKey jk = new JobKey(task.TaskID.ToString());
                    _scheduler.PauseJob(jk);
                }
                else
                {
                    List<DateTime> list = GetNextFireTime(task.CronExpressionString, 5);
                    foreach (var time in list)
                    {
                        //LogHelper.WriteLog(time.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            else
            {
                throw new Exception(task.CronExpressionString + "不是正确的Cron表达式,无法启动该任务!");
            }
        }


        /// <summary>
        /// 初始化 远程Quartz服务器中的，各个Scheduler实例。
        /// 提供给远程管理端的后台，用户获取Scheduler实例的信息。
        /// </summary>
        public static void InitRemoteScheduler()
        {
            try
            {
                NameValueCollection properties = new NameValueCollection
                {
                    ["quartz.scheduler.instanceName"] = "ExampleQuartzScheduler",
                    ["quartz.scheduler.proxy"] = "true",
                    ["quartz.scheduler.proxy.address"] = string.Format("{0}://{1}:{2}/QuartzScheduler", Scheme, Server, Port)
                };

                ISchedulerFactory sf = new StdSchedulerFactory(properties);

                _scheduler = sf.GetScheduler();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        /// <summary>
        /// 删除现有任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void DeleteJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则删除
                    _scheduler.DeleteJob(jk);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void PauseJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则暂停任务
                    _scheduler.PauseJob(jk);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobKey">任务key</param>
        public static void ResumeJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则暂停任务
                    _scheduler.ResumeJob(jk);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary> 
        /// 获取类的属性、方法  
        /// </summary>  
        /// <param name="assemblyName">程序集</param>  
        /// <param name="className">类名</param>  
        private static Type GetClassInfo(string assemblyName, string className)
        {
            try
            {
                assemblyName = FileHelper.GetAbsolutePath(assemblyName + ".dll");
                Assembly assembly = null;
                if (!AssemblyDict.TryGetValue(assemblyName, out assembly))
                {
                    assembly = Assembly.LoadFrom(assemblyName);
                    AssemblyDict[assemblyName] = assembly;
                }
                Type type = assembly.GetType(className, true, true);
                return type;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public static void StopSchedule()
        {
            try
            {
                //判断调度是否已经关闭
                if (!_scheduler.IsShutdown)
                {
                    //等待任务运行完成
                    _scheduler.Shutdown(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public static bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        /// 获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="CronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public static List<DateTime> GetNextFireTime(string CronExpressionString, int numTimes)
        {
            if (numTimes < 0)
            {
                throw new Exception("参数numTimes值大于等于0");
            }
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(CronExpressionString).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            List<DateTime> list = new List<DateTime>();
            foreach (DateTimeOffset dtf in dates)
            {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local));
            }
            return list;
        }


        public static object CurrentTaskList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取当前执行的Task 对象
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TaskModel GetTaskDetail(IJobExecutionContext context)
        {
            TaskModel task = new TaskModel();

            if (context != null)
            {

                task.TaskID = Guid.Parse(context.Trigger.Key.Name);
                task.TaskName = context.Trigger.Description;
                task.RecentRunTime = DateTime.Now;
                task.TaskParam = context.JobDetail.JobDataMap.Get("TaskParam") != null ? context.JobDetail.JobDataMap.Get("TaskParam").ToString() : "";
            }
            return task;
        }
    }
}
