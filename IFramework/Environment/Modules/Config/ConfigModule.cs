﻿using System;

namespace IFramework.Modules.Config
{
    /// <summary>
    /// config 模块
    /// </summary>
    public class ConfigModule : FrameworkModule, IConfigModule
    {
        private BindableObjectHandler _binder;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public override int priority { get { return 0; } }
        protected override void Awake()
        {
            _binder = new BindableObjectHandler();
        }
        protected override void OnDispose()
        {
            _binder.Dispose();
        }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        /// <summary>
        /// 获取数值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetConfig<T>(string name)
        {
            return _binder.GetValue<T>(name);
        }
        /// <summary>
        /// 获取数值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetConfig(Type type, string name)
        {
            return _binder.GetValue(type, name);
        }
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IConfigModule SetConfig(Type type, object value, string name)
        {
            _binder.PublishProperty(type, value, name);
            return this;
        }

        /// <summary>
        /// 发布变化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IConfigModule SetConfig<T>(T value, string name)
        {
            _binder.PublishProperty<T>(value, name);
            return this;
        }
        /// <summary>
        /// 绑定 config 字段 绑定对象需要继承BindableObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public IConfigModule BindConfig<T>(Action<T> setter, Func<T> getter)
        {
            _binder.BindProperty(setter, getter, BindableObject.BindOperation.Listen);
            return this;
        }


    }
}
