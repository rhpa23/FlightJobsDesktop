﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.Factorys
{
    public static class AbstractFactoryExtension
    {
        public static void AddAbstractFactory<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddTransient<TInterface, TImplementation>();
            services.AddSingleton<Func<TInterface>>(x => () => x.GetService<TInterface>());
            services.AddSingleton<IAbstractFactory<TInterface>, AbstractFactory<TInterface>>();
        }
    }

    public class AbstractFactory<T> : IAbstractFactory<T>
    {
        private readonly Func<T> _factory;
        public AbstractFactory(Func<T> factory)
        {
            _factory = factory;
        }

        public T Create()
        {
            return _factory();
        }
    }

    public interface IAbstractFactory<T>
    {
        T Create();
    }
}
