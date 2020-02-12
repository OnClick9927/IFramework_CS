﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IFramework.Modules.Loom
{
    internal interface ILoomModule
    {
        void RunOnMainThread(Action action, float time = 0.0f);
        void RunOnSubThread(Action action);
    }
    public class LoomModule : FrameworkModule, ILoomModule
    {
        private struct DelayedTask
        {
            public float time;
            public Action action;
            public DelayedTask(float time, Action action)
            {
                this.time = time;
                this.action = action;
            }

        }
        private int MaxThreadCount = 8;
        private int ThreadCount;
        private Semaphore semaphore;
        private List<DelayedTask> tasks;
        private List<DelayedTask> delayedTasks;
        private List<DelayedTask> NoDelayTasks;
        protected override bool needUpdate { get { return true; } }


        private double GetUtcFrame(DateTime time)
        {
            DateTime timeZerro = new DateTime(1970, 1, 1);
            return (time - timeZerro).TotalMilliseconds;
        }
        public void RunOnMainThread(Action action, float time = 0.0f)
        {
            if (time != 0)
            {
                lock (delayedTasks)
                {
                    delayedTasks.Add(new DelayedTask((float)GetUtcFrame(DateTime.Now) + time, action));
                }
            }
            else
            {
                lock (NoDelayTasks)
                {
                    NoDelayTasks.Add(new DelayedTask(0, action));
                }
            }
        }
        public void RunOnSubThread(Action action)
        {
            semaphore.WaitOne();
            Interlocked.Increment(ref ThreadCount);
            ThreadPool.QueueUserWorkItem((ar) => {

                try
                {
                    action();
                }
                catch { }
                finally
                {
                    Interlocked.Decrement(ref ThreadCount);
                }
            });
        }

        protected override void OnUpdate()
        {
            lock (NoDelayTasks)
            {
                while (NoDelayTasks.Count > 0)
                {
                    NoDelayTasks.Dequeue().action();
                }
            }
            lock (tasks)
            {
                lock (delayedTasks)
                {
                    var temp = delayedTasks.Where(d => d.time <= (float)GetUtcFrame(DateTime.Now)).ToList();
                    tasks.AddRange(temp);
                    temp.ForEach((task) => { delayedTasks.Remove(task); });
                }
                while (tasks.Count != 0)
                {
                    tasks.Dequeue().action();
                }
            }
        }
        protected override void OnDispose()
        {
            NoDelayTasks.Clear();
            tasks.Clear();
            delayedTasks.Clear();
            semaphore.Close();
        }
        protected override void Awake()
        {
            semaphore = new Semaphore(MaxThreadCount, MaxThreadCount);
            tasks = new List<DelayedTask>();
            delayedTasks = new List<DelayedTask>();
            NoDelayTasks = new List<DelayedTask>();
        }
    }
}
