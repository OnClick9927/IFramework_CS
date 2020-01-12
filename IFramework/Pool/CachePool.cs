﻿using System;

namespace IFramework
{
    public class RunningPool<T> : ObjectPool<T>
    {
        protected override T CreatNew(IEventArgs arg, params object[] param)
        {
            return default(T);
        }
    }
    public abstract class SleepingPool<T> : ObjectPool<T>
    {
    }
    public class CachePool<T> : IDisposable
    {
        public Type Type { get { return typeof(T); } }
        public bool AutoClear { get; set; }
        public int SleepCapcity { get; set; }

        protected SleepingPool<T> sleepPool;
        protected RunningPool<T> runningPoool;
        public virtual RunningPool<T> RunningPoool { get { return runningPoool; } set { runningPoool = value; } }
        public virtual SleepingPool<T> SleepPool { get { return sleepPool; } set { sleepPool = value; } }

        public int SleepCount { get { return SleepPool.Count; } }
        public int RunningCount { get { return RunningPoool.Count; } }

        public event ObjPoolEventDel<T> OnRunningPoolClearObject { add { runningPoool.OnClearObject += value; } remove { runningPoool.OnClearObject -= value; } }
        public event ObjPoolEventDel<T> OnRunningPoolGetObject { add { runningPoool.OnGetObject += value; } remove { runningPoool.OnGetObject -= value; } }
        public event ObjPoolEventDel<T> OnRunningPoolSetObject { add { runningPoool.OnSetObject += value; } remove { runningPoool.OnSetObject -= value; } }
        public event ObjPoolEventDel<T> OnRunningPoolCreateObject { add { runningPoool.OnCreateObject += value; } remove { runningPoool.OnCreateObject -= value; } }

        public event ObjPoolEventDel<T> OnClearObject { add { SleepPool.OnClearObject += value; } remove { SleepPool.OnClearObject -= value; } }
        public event ObjPoolEventDel<T> OnGetObject { add { SleepPool.OnGetObject += value; } remove { SleepPool.OnGetObject -= value; } }
        public event ObjPoolEventDel<T> OnSetObject { add { SleepPool.OnSetObject += value; } remove { SleepPool.OnSetObject -= value; } }
        public event ObjPoolEventDel<T> OnCreateObject { add { SleepPool.OnCreateObject += value; } remove { SleepPool.OnCreateObject -= value; } }

        public CachePool(SleepingPool<T> sleepPool) : this(new RunningPool<T>(), sleepPool, true, 16) { }
        public CachePool(RunningPool<T> runningPoool, SleepingPool<T> sleepPool) : this(runningPoool, sleepPool, true, 16) { }
        public CachePool(RunningPool<T> runningPoool, SleepingPool<T> sleepPool, bool autoClear, int sleepCapcity)
        {
            this.AutoClear = autoClear;
            this.SleepCapcity = sleepCapcity;
            this.RunningPoool = runningPoool;
            this.SleepPool = sleepPool;
        }
        public virtual void Dispose()
        {
            CycleRunningPool(null);
            SleepPool.Dispose();
            RunningPoool.Dispose();
        }

        public bool IsRunning(T t) { return RunningPoool.Contains(t); }
        public bool IsSleep(T t) { return SleepPool.Contains(t); }
        public bool Contains(T t) { return IsRunning(t) || IsSleep(t); }


        public T Get()
        {
            T t = SleepPool.Get();
            RunningPoool.Set(t);
            return t;
        }
        public void Set(T t)
        {
            RunningPoool.Clear(t);
            SleepPool.Set(t);
            AutoClean();
        }
        public T Get(IEventArgs arg, params object[] param)
        {
            T t = SleepPool.Get(arg, param);
            RunningPoool.Set(t);
            return t;
        }
        public void Set(T t, IEventArgs arg, params object[] param)
        {
            RunningPoool.Clear(t);
            SleepPool.Set(t, arg, param);
            AutoClean();
        }
        private void AutoClean()
        {
            if (!AutoClear) return;
            SleepPool.Clear(SleepPool.Count - SleepCapcity);
        }

        public void Clear(T t, bool ignoreRun = true)
        {
            if (IsRunning(t))
            {
                if (ignoreRun) return;
                Set(t);
            }
            SleepPool.Clear(t);
        }
        public void Clear(T[] ts, bool ignoreRun = true)
        {
            ts.ForEach((t) =>
            {
                Clear(t, ignoreRun);
            });
        }
        public void Clear(T t, IEventArgs arg, bool ignoreRun = true, params object[] param)
        {
            if (IsRunning(t))
            {
                if (ignoreRun) return;
                Set(t, arg, param);
            }
            SleepPool.Clear(t, arg, param);
        }
        public void Clear(T[] ts, IEventArgs arg, bool ignoreRun = true, params object[] param)
        {
            ts.ForEach((t) =>
            {
                Clear(t, arg, ignoreRun, param);
            });
        }

        public void CycleRunningPool(int count)
        {
            while (RunningPoool.Count > 0)
            {
                T t = RunningPoool.Get();
                SleepPool.Set(t);
                AutoClean();
            }
        }

        public void CycleRunningPool()
        {
            while (RunningPoool.Count > 0)
            {
                T t = RunningPoool.Get();
                SleepPool.Set(t);
                AutoClean();
            }
        }
        public void ClearSleepPool()
        {
            SleepPool.Clear();
        }
        public void CycleRunningPool(IEventArgs arg, params object[] param)
        {
            while (RunningPoool.Count > 0)
            {
                T t = RunningPoool.Get();
                SleepPool.Set(t, arg, param);
                AutoClean();
            }
        }
        public void ClearSleepPool(IEventArgs arg, params object[] param)
        {
            SleepPool.Clear(arg, param);
        }
    }
}
