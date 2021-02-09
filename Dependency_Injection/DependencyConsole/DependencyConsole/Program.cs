using System;
using System.Collections.Generic;
using System.Linq;


namespace DependencyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var helloService = new HelloService();
            //var serviceConsumer = new ServiceConsumer(helloService);

            // var helloService = (HelloService) Activator.CreateInstance(typeof(HelloService));
            // var serviceConsumer = (ServiceConsumer) Activator.CreateInstance(typeof(ServiceConsumer), helloService);
            // serviceConsumer.Print();

            var dependency = new DependencyContainer();
            dependency.AddTransient<HelloService>();
            dependency.AddTransient<ServiceConsumer>();
            dependency.AddSingleton<MessageService>();

            var resolver = new DependencyResolver(dependency);
            var serviceConsumer = resolver.GetService<ServiceConsumer>();
            
            serviceConsumer.Print();
            var messageService1 = resolver.GetService<ServiceConsumer>();
            messageService1.Print();
            var messageService2 = resolver.GetService<ServiceConsumer>();
            messageService1.Print();
            var messageService3 = resolver.GetService<ServiceConsumer>();
            messageService1.Print();
            Console.ReadKey();
        }
    }

    public class DependencyContainer
    {
        private readonly List<Dependency> _dependencies;

        public DependencyContainer()
        {
            _dependencies = new List<Dependency>();
        }

        public void AddSingleton<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton));
        }

        public void AddTransient<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
        }

        public Dependency GetDependency(Type type)
        {
            return _dependencies.First(x => x.Type.Name == type.Name);
        }
    }

    public class DependencyResolver
    {
        private readonly DependencyContainer _container;
        
        public DependencyResolver(DependencyContainer dependencyContainer)
        {
            _container = dependencyContainer;
        }

        
        
        public T GetService<T>()
        {
            return (T) GetService(typeof(T));
        }
        
        private object GetService(Type type)
        {
            var dependency = _container.GetDependency(type);
            var constructor = dependency.Type.GetConstructors().Single();
            var parameters = constructor.GetParameters().ToArray();
            var parameterImplementations = new object[parameters.Length];

            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameterImplementations[i] = GetService(parameters[i].ParameterType);
                }

                return CreateImplementation(dependency,
                    t => Activator.CreateInstance(t,
                        parameterImplementations));
            }

            return CreateImplementation(dependency, t => Activator.CreateInstance(t));
        }

        private object CreateImplementation(Dependency dependency, Func<Type, object> factory)
        {
            if (dependency.Implemented)
                return dependency.Implementation;

            var implementation = factory(dependency.Type);
            if (dependency.Lifetime == DependencyLifetime.Singleton)
            {
                dependency.AddImplementation(implementation);
            }
            return  implementation;
        }
    }

    public class Dependency
    {
        public Dependency(Type type, DependencyLifetime lifetime)
        {
            Type = type;
            Lifetime = lifetime;
        }

        public Type Type { get; set; }
        public DependencyLifetime Lifetime { get; set; }

        public object Implementation { get; set; }
        public bool Implemented { get; set; }

        public void AddImplementation(object impl)
        {
            Implementation = impl;
            Implemented = true;
        }
    }

    public class ServiceConsumer
    {
        private readonly HelloService _helloService;
        public ServiceConsumer(HelloService helloService)
        {
            _helloService = helloService;
        }

        public void Print()
        {
            _helloService.Print();
        }
    }

    public class HelloService
    {
        private readonly MessageService _messageService;
        public HelloService(MessageService messageService)
        {
            _messageService = messageService;
        }
        
        public void Print()
        {
            Console.WriteLine($"{_messageService.Message()}Hello World");
        }
    }

    public class MessageService
    {
        private readonly int _random;

        public MessageService()
        {
            _random = new Random().Next();
        }
        public string Message()
        {
            
            return $"Hi, how are you ? ${_random}";
        }
    }

    public enum DependencyLifetime
    {
        Singleton = 0,
        Transient
    }
}