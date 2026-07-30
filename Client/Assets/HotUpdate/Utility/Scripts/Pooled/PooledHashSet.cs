using System;
using System.Collections.Generic;

public sealed class PooledHashSet<T> : HashSet<T>, IDisposable
{
    private static readonly MonitoredObjectPool.ObjectPool<PooledHashSet<T>, T> s_Pool = 
        new("PooledHash", () => new PooledHashSet<T>(), 
            null,
            l => l.Clear());

    public static UnityEngine.Pool.PooledObject<PooledHashSet<T>> Get(out PooledHashSet<T> value) => s_Pool.Get(out value);
    public static PooledHashSet<T> Get() => s_Pool.Get();
    private PooledHashSet() { }

    void IDisposable.Dispose()
    {
        s_Pool.Release(this);
    }

#if POOLED_EXCEPTION
    ~PooledHashSet() => s_Pool.FinalizeDebug();
#endif

#if UNITY_EDITOR
    private class Example
    {
        // 修改 hashSet 方法
        private void SetHashSet(in HashSet<int> hashSet)
        {
            // hashSet = new HashSet<int>(); error!! in 限制，不允许在方法内 修改引用对象
            hashSet.Add(0); // 支持 元子 操作
        }
            
        #region 局部变量
        private void Foo()
        {
            // 第一种写法，作用域为整个方法，在离开 Foo 方法后 自动释放
            using var hashSet = PooledHashSet<int>.Get();
            SetHashSet(hashSet);
            // hashSet = PooledHashSet<int>.Get(); error!! using 的引用对象不可变
        
            // 第二种写法
            using (var hashSet2 = PooledHashSet<int>.Get())
            {
                SetHashSet(hashSet2);
            } // 离开作用域，自动释放 进池 
        }
        #endregion
        
        #region 全局变量
        // eg!!! 千万不要使用 hashSet3 = new() 的方式定义
        // 脚本重新编译触发重置，每次修改脚本并返回运行模式时，Unity 会重新编译并重新初始化所有字段
        // 导致会执行多次脚本，触发多次调用，大于实际同时存在的数量变多，error 日志不精确
        // 快速检查 预制体上的所有 mono 脚本中是否存在该类型定义：
        //      1、运行前选中过所需预制体，然后运行
        //      2、重新编译前选中过所需预制体
        private PooledHashSet<int> hashSet3;
        private void Init()
        {
            hashSet3 = PooledHashSet<int>.Get(); // eg!! 初始化后不要在脚本中修改引用的对象，除非先释放
        }
        private void Foo2()
        {
            hashSet3.Clear();
            SetHashSet(hashSet3);
        }
        public void DestroyModel()
        {
            // eg!! 在销毁时，需要手动释放 !! 
            // 若缺少此步骤，当给这个池化集合初始化后，在触发 gc 或者 关闭编辑器时 会出现 error 日志
            ((IDisposable)hashSet3).Dispose();
        }
        #endregion
    }
#endif
}