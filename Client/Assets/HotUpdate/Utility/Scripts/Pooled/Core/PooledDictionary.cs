using System;
using System.Collections.Generic;

public sealed class PooledDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
{
    private static readonly MonitoredObjectPool.ObjectPool<PooledDictionary<TKey, TValue>, (TKey, TValue)> s_Pool = 
        new("PooledDictionary", () => new PooledDictionary<TKey, TValue>(), 
            null,
            l => l.Clear());

    public static UnityEngine.Pool.PooledObject<PooledDictionary<TKey, TValue>> Get(out PooledDictionary<TKey, TValue> value) => s_Pool.Get(out value);
    public static PooledDictionary<TKey, TValue> Get() => s_Pool.Get();
    private PooledDictionary() { }

    void IDisposable.Dispose()
    {
        s_Pool.Release(this);
    }

#if POOLED_EXCEPTION
    ~PooledDictionary() => s_Pool.FinalizeDebug();
#endif

#if UNITY_EDITOR
    private class Example
    {
        // 修改 dict 方法
        private void SetDict(in IDictionary<int, int> dict)
        {
            // dict = new Dictionary<int, int>(); error!! in 限制，不允许在方法内 修改引用对象
            dict.Add(0, 0); // 支持 元子 操作
        }
            
        #region 局部变量
        private void Foo()
        {
            // 第一种写法，作用域为整个方法，在离开 Foo 方法后 自动释放
            using var dict = PooledDictionary<int, int>.Get();
            SetDict(dict);
            // dict = PooledDictionary<int, int>.Get(); error!! using 的引用对象不可变
        
            // 第二种写法
            using (var dict2 = PooledDictionary<int, int>.Get())
            {
                SetDict(dict2);
            } // 离开作用域，自动释放 进池 
        }
        #endregion
        
        #region 全局变量
        // eg!!! 千万不要使用 dict3 = new() 的方式定义
        // 脚本重新编译触发重置，每次修改脚本并返回运行模式时，Unity 会重新编译并重新初始化所有字段
        // 导致会执行多次脚本，触发多次调用，大于实际同时存在的数量变多，error 日志不精确
        // 快速检查 预制体上的所有 mono 脚本中是否存在该类型定义：
        //      1、运行前选中过所需预制体，然后运行
        //      2、重新编译前选中过所需预制体
        private PooledDictionary<int, int> dict3;
        private void Init()
        {
            dict3 = PooledDictionary<int, int>.Get(); // eg!! 初始化后不要在脚本中修改引用的对象，除非先释放
        }
        private void Foo2()
        {
            dict3.Clear();
            SetDict(dict3);
        }
        public void DestroyModel()
        {
            // eg!! 在销毁时，需要手动释放 !! 
            // 若缺少此步骤，当给这个池化集合初始化后，在触发 gc 或者 关闭编辑器时 会出现 error 日志
            ((IDisposable)dict3).Dispose();
        }
        #endregion
    }
#endif
}