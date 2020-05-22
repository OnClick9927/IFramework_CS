﻿using IFramework.Singleton;
using System;
using System.Collections.Generic;

namespace IFramework.Pool
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    [System.Runtime.InteropServices.ComVisible(false)]

    public class PoolManager : SingletonPropertyClass<PoolManager>
    {
        private class PoolObjInfo
        {
            public IPoolObject content;
            public Type type;
            public IPoolObjectOwner owner;
        }
        private class PoolObjInfoPool : ObjectPool<PoolObjInfo>
        {
            protected override PoolObjInfo CreatNew(IEventArgs arg)
            {
                return new PoolObjInfo();
            }
        }
        private PoolObjInfoPool infoCreater;
        private List<PoolObjInfo> infos;
        private Dictionary<Type, PoolObjectPool> dic = new Dictionary<Type, PoolObjectPool>();

        private PoolManager() { }

        protected override void OnSingletonInit()
        {
            infoCreater = new PoolObjInfoPool();
            infos = new List<PoolObjInfo>();
        }
        public override void Dispose()
        {
            foreach (var item in dic.Values) item.Dispose();
            infoCreater.Dispose();
        }

        public PoolObjectPool this[Type type]
        {
            get
            {
                if (!Instance.dic.ContainsKey(type))
                {
                    var pool = new PoolObjectPool();
                    Instance.dic.Add(type, pool);

                    pool.OnRunningPoolCreateObject += (obj, arg) => { Log.E("Err running pool cant't Creat anything"); };
                    pool.OnRunningPoolGetObject += (obj, arg) => { /*Log.L("running pool Get obj");*/};
                    pool.OnRunningPoolSetObject += (obj, arg) => { /*Log.L("running pool Set obj"); */};
                    pool.OnRunningPoolClearObject += (obj, arg) => { /*Log.L("running pool Cycle obj");*/ };
                }
                return Instance.dic[type];
            }
        }
        public static PoolObjectPool GetPool<T>() where T : IPoolObject
        {
            return GetPool(typeof(T));
        }
        public static PoolObjectPool GetPool(Type type)
        {
            return Instance[type];
        }

        public static void SetCreater<T>(SleepingPool<T> creater) where T : IPoolObject
        {
            GetPool(typeof(T)).SleepPool = creater as SleepingPool<IPoolObject>;
        }
        public static void AddCreaterDel<T>(PoolObjCreaterDel creater) where T : IPoolObject
        {
            Type type = typeof(T);
            GetPool(typeof(T)).AddCreater(creater);
        }


        private static void SetInfo(IPoolObject obj, IPoolObjectOwner owner, Type t)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj; });
            if (o == null)
            {
                o = Instance.infoCreater.Get();
                Instance.infos.Add(o);
            }
            o.owner = owner;
            o.type = t;
            o.content = obj;
        }
        public static IPoolObject Get(Type objType, IPoolObjectOwner owner, IEventArgs arg)
        {

            IPoolObject o = Instance[objType].Get(arg);
            SetInfo(o, owner, objType);
            //Log.W(type.ToString() + "Get Idel" + Instance.dic[type].CacheCount + "  worker " + Instance.dic[type].WorkerCount);
            return o;

        }
        public static T Get<T>(IPoolObjectOwner owner, IEventArgs arg) where T : IPoolObject
        {
            return (T)Get(typeof(T), owner, arg);
        }


        private static void SetOne(PoolObjInfo o, IEventArgs arg)
        {
            Instance[o.type].Set(o.content, arg);
        }

        public static void Set(Type objType, IEventArgs arg)
        {

            List<PoolObjInfo> o = Instance.infos.FindAll((i) => { return i.type == objType; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                o.ForEach((i) =>
                {
                    SetOne(i, arg);
                });
            }

        }
        public static void Set(Type objType, IPoolObject obj, IEventArgs arg)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj && i.type == objType; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                SetOne(o, arg);
            }
        }
        public static void Set(Type objType, IPoolObject[] objs, IEventArgs arg)
        {
            objs.ForEach((o) => { Set(objType, o, arg); });
        }
        public static void Set(Type objType, IPoolObjectOwner owner, IEventArgs arg)
        {

            List<PoolObjInfo> o = Instance.infos.FindAll((i) => { return i.owner == owner && i.type == objType; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                o.ForEach((i) =>
                {
                    SetOne(i, arg);
                });
            }
        }
        public static void Set(Type objType, IPoolObject obj, IPoolObjectOwner owner, IEventArgs arg)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj && i.type == objType && i.owner == owner; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                SetOne(o, arg);
            }
        }
        public static void Set(Type objType, IPoolObject[] objs, IPoolObjectOwner owner, IEventArgs arg)
        {
            objs.ForEach((o) => { Set(objType, o, owner, arg); });
        }

        public static void Set(IPoolObject obj, IEventArgs arg)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                SetOne(o, arg);
            }
        }
        public static void Set(IPoolObject[] objs, IEventArgs arg)
        {
            objs.ForEach((o) => { Set(o, arg); });
        }
        public static void Set(IPoolObjectOwner owner, IEventArgs arg)
        {
            List<PoolObjInfo> o = Instance.infos.FindAll((i) => { return i.owner == owner; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                o.ForEach((i) =>
                {
                    SetOne(i, arg);
                });
            }
        }
        public static void Set(IPoolObject obj, IPoolObjectOwner owner, IEventArgs arg)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj && i.owner == owner; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                SetOne(o, arg);
            }
        }

        public static void Set<T>(IEventArgs arg) where T : IPoolObject
        {
            Set(typeof(T), arg);
        }
        public static void Set<T>(T obj, IEventArgs arg) where T : IPoolObject
        {
            Set(typeof(T), obj, arg);

        }
        public static void Set<T>(T[] objs, IEventArgs arg) where T : IPoolObject
        {
            objs.ForEach((o) => { Set(typeof(T), o, arg); });
        }
        public static void Set<T>(IPoolObjectOwner owner, IEventArgs arg) where T : IPoolObject
        {
            Set(typeof(T), owner, arg);
        }
        public static void Set<T>(T t, IPoolObjectOwner owner, IEventArgs arg) where T : IPoolObject
        {
            Set(typeof(T), t, owner, arg);
        }
        public static void Set<T>(T[] objs, IPoolObjectOwner owner, IEventArgs arg) where T : IPoolObject
        {
            objs.ForEach((o) => { Set(typeof(T), o, owner, arg); });
        }


        private static void ClearOne(PoolObjInfo o, IEventArgs arg, bool ignoreRun = true)
        {
            if (Instance[o.type].IsRunning(o.content))
            {
                if (ignoreRun) return;
                Set(o.type, o.content, o.owner, arg);
            }
            Instance[o.type].Clear(o.content, arg);
            Instance.infos.Remove(o);
            Instance.infoCreater.Set(o);
        }

        public static void Clear(Type objType, IEventArgs arg, bool ignoreRun = true)
        {
            var o = Instance.infos.FindAll((i) => { return i.type == objType; });

            o.ForEach((i) =>
            {
                ClearOne(i, arg, ignoreRun);
            });
        }
        public static void Clear<T>(IEventArgs arg, bool ignoreRun = true)
        {
            Clear(typeof(T), arg, ignoreRun);
        }
        public static void Clear(IPoolObject[] objs, IEventArgs arg, bool ignoreRun = true)
        {
            objs.ForEach((o) => { Clear(o, arg, ignoreRun); });
        }
        public static void Clear(IPoolObjectOwner owner, IEventArgs arg, bool ignoreRun = true)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.owner == owner; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                ClearOne(o, arg, ignoreRun);
            }
        }
        public static void Clear(IPoolObject obj, IEventArgs arg, bool ignoreRun = true)
        {
            PoolObjInfo o = Instance.infos.Find((i) => { return i.content == obj; });
            if (o == null) Log.L("Not Find Same IPoolObject");
            else
            {
                ClearOne(o, arg, ignoreRun);
            }
        }

    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

}
