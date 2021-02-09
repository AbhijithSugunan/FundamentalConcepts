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
            dependency.AddDependency<HelloService>();
            dependency.AddDependency<ServiceConsumer>();
            dependency.AddDependency<MessageService>();

            var resolver = new DependencyResolver(dependency);
            var serviceConsumer = resolver.GetService<ServiceConsumer>();
            serviceConsumer.Print();
            
            
            Console.ReadKey();
        }
    }

    public class DependencyContainer
    {
        private readonly List<Type> _dependencies;

        public DependencyContainer()
        {
            _dependencies = new List<Type>();
        }

        public void AddDependency(Type type)
        {
            _dependencies.Add(type);
        }

        public void AddDependency<T>()
        {
            _dependencies.Add(typeof(T));
        }

        public Type GetDependency(Type type)
        {
            return _dependencies.First(x => x. Name == type.Name);
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
            var constructor = dependency.GetConstructors().Single();
            var parameters = constructor.GetParameters().ToArray();
            var parameterImplementations = new object[parameters.Length];

            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameterImplementations[i] = GetService(parameters[i].ParameterType);
                }

                return Activator.CreateInstance(dependency, parameterImplementations);
            }
            

            return  Activator.CreateInstance(type);
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
        public string Message()
        {
            return "Hi, how are you ?";
        }
    }
}