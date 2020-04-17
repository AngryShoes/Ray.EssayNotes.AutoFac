# Core DI ����

## �������Ľӿڵ�ְ��

### IServiceScope����

��2��ְ��

* �ͷ���Դ
* ��װһ����������IServiceProvider����

��̳���IDisposable�ӿڣ����Ұ�����һ������������Ϊ���ԡ�

```
  /// <summary>
  /// The <see cref="M:System.IDisposable.Dispose" /> method ends the scope lifetime. Once Dispose
  /// is called, any scoped services that have been resolved from
  /// <see cref="P:Microsoft.Extensions.DependencyInjection.IServiceScope.ServiceProvider" /> will be
  /// disposed.
  /// </summary>
  public interface IServiceScope : IDisposable
  {
    /// <summary>
    /// The <see cref="T:System.IServiceProvider" /> used to resolve dependencies from the scope.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
  }

```

### IServiceProvider��������

ֻ��һ��ְ�𣺻�ȡʵ����

��δ�̳��κνӿڣ�ֻ��һ��GetService()������

```
  /// <summary>Defines a mechanism for retrieving a service object; that is, an object that provides custom support to other objects.</summary>
  public interface IServiceProvider
  {
    /// <summary>Gets the service object of the specified type.</summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType" />.
    /// -or-
    /// <see langword="null" /> if there is no service object of type <paramref name="serviceType" />.</returns>
    object GetService(Type serviceType);
  }

```

### IServiceScopeFactory���򹤳���

ֻ��һ��ְ��������IServiceScope�������򣩡�

��δʵ���κνӿڣ�ֻ��һ��CreateScope()������

```
  /// <summary>
  /// A factory for creating instances of <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope" />, which is used to create
  /// services within a scope.
  /// </summary>
  public interface IServiceScopeFactory
  {
    /// <summary>
    /// Create an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope" /> which
    /// contains an <see cref="T:System.IServiceProvider" /> used to resolve dependencies from a
    /// newly created scope.
    /// </summary>
    /// <returns>
    /// An <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope" /> controlling the
    /// lifetime of the scope. Once this is disposed, any scoped services that have been resolved
    /// from the <see cref="P:Microsoft.Extensions.DependencyInjection.IServiceScope.ServiceProvider" />
    /// will also be disposed.
    /// </returns>
    IServiceScope CreateScope();
  }

```

### IServiceProviderEngine���������棩

ְ��


�ȼ۸��ӵ�һ���ӿڣ��ڲ���װ��һ��ָ���������á�

~~~
  internal interface IServiceProviderEngine : IServiceProvider, IDisposable, IAsyncDisposable
  {
    IServiceScope RootScope { get; }

    void ValidateService(ServiceDescriptor descriptor);
  }

~~~


## ��������ʵ��

### ServiceProvider��������

��Ҫʵ���������ܣ�IServiceProvider����

�ö���ǳ��򵥣��ڲ���װ��һ��IServiceProviderEngine���������棩�������ô���ģʽ��������ȥʵ�־�����������ܡ�

Ŀ����Ϊ��ΪDI��������һ���װ���롣

���Կ���GetService()����ʲô��û������ֻ��ֱ�ӵ��õ������GetService()������

~~~
  public sealed class ServiceProvider : IServiceProvider, IDisposable, IServiceProviderEngineCallback, IAsyncDisposable
  {
    private readonly IServiceProviderEngine _engine;

    /// <summary>Gets the service object of the specified type.</summary>
    /// <param name="serviceType">The type of the service to get.</param>
    /// <returns>The service that was produced.</returns>
    public object GetService(Type serviceType)
    {
      return this._engine.GetService(serviceType);
    }
  }

~~~

### ServiceProviderEngine���������棩

�ȼ۸��ӣ���ʵ�����������湦�ܣ�IServiceProviderEngine����Ҳʵ�����������ܣ�IServiceProvider������ʵ�����򹤳����ܣ�IServiceScopeFactory����

* ��Ϊ��������ְ��

RootScope��ָ���������ã�ָ����ǵ�ǰ�������ڵ�������

RootScope��Root������ͬһ��ServiceProviderEngineScope����ֻ�ǲ�ͬ�Ķ�̬���ֶ��ѡ�

~~~
  internal abstract class ServiceProviderEngine : IServiceProviderEngine, IServiceProvider, IDisposable, IAsyncDisposable, IServiceScopeFactory
  {
    protected ServiceProviderEngine(
      IEnumerable<ServiceDescriptor> serviceDescriptors,
      IServiceProviderEngineCallback callback)
    {
      //...
      this.Root = new ServiceProviderEngineScope(this);
      //...
    }

    public ServiceProviderEngineScope Root { get; }

    public IServiceScope RootScope
    {
      get
      {
        return (IServiceScope) this.Root;
      }
    }

    //...
  }

~~~

### ServiceProviderEngineScope��������

��ʵ������ְ��IServiceScope����Ҳʵ��������ְ��IServiceProvider����

����ʱ��Ҫ����һ���������

~~~
  internal class ServiceProviderEngineScope : IServiceScope, IDisposable, IServiceProvider, IAsyncDisposable
  {
    public ServiceProviderEngineScope(ServiceProviderEngine engine)
    {
      this.Engine = engine;
    }
    public ServiceProviderEngine Engine { get; }
    //...
  }

~~~

* ��Ϊ��ְ��

��ǰ��������ְ�����2�����ܣ��ͷ���Դ�ͷ�װ������

    + �ͷŹ���

    �������ͷ�ǰ����������Dispose()�������÷�����ȥ�������ڵĿ��ͷ�ʵ���أ�����ִ��ʵ����Disposable����ʵ�ֶ��ڴ���Դ���ͷš�

    ~~~

  internal class ServiceProviderEngineScope : IServiceScope, IDisposable, IServiceProvider, IAsyncDisposable
  {
    private List<object> _disposables;//���ͷ�ʵ����
    private bool _disposed;

    internal Dictionary<ServiceCacheKey, object> ResolvedServices { get; } = new Dictionary<ServiceCacheKey, object>();//�־û�ʵ����

    public void Dispose()
    {
      List<object> objectList = this.BeginDispose();
      if (objectList == null)
        return;
      for (int index = objectList.Count - 1; index >= 0; --index)
      {
        IDisposable disposable = objectList[index] as IDisposable;
        if (disposable == null)
          throw new InvalidOperationException(Resources.FormatAsyncDisposableServiceDispose((object) TypeNameHelper.GetTypeDisplayName(objectList[index], true)));
        disposable.Dispose();
      }
    }

    private List<object> BeginDispose()
    {
      List<object> disposables;
      lock (this.ResolvedServices)
      {
        if (this._disposed)
          return (List<object>) null;
        this._disposed = true;
        disposables = this._disposables;
        this._disposables = (List<object>) null;
      }
      return disposables;
    }
    //...
  }

    ~~~

    + ��װ��������

    ��װ��������������ָ���Լ�����

    ~~~
  internal class ServiceProviderEngineScope : IServiceScope, IDisposable, IServiceProvider, IAsyncDisposable
  {
    public IServiceProvider ServiceProvider
    {
      get
      {
        return (IServiceProvider) this;
      }
    }
    //...
  }
    ~~~

* ��Ϊ����ְ��

��GetService()����ͬ��ʹ�ô���ģʽ�������װ���������ȥִ�о��������

~~~
  internal class ServiceProviderEngineScope : IServiceScope, IDisposable, IServiceProvider, IAsyncDisposable
  {
    public ServiceProviderEngineScope(ServiceProviderEngine engine)
    {
      this.Engine = engine;
    }

    public ServiceProviderEngine Engine { get; }

    public object GetService(Type serviceType)
    {
      if (this._disposed)
        ThrowHelper.ThrowObjectDisposedException();
      return this.Engine.GetService(serviceType, this);
    }
    //...
  }

~~~