using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ragnarok
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// 특정 Type에 해당하는 Instance List를 반환
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="constructorArgs">생성자 Args</param>
        /// <returns></returns>
        public static List<T> GetListOfType<T>(params object[] constructorArgs)
        {
            List<T> list = new List<T>();

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (var type in types)
            {
                list.Add((T)Activator.CreateInstance(type, constructorArgs));
            }

            list.Sort();
            return list;
        }

        /// <summary>
        /// 해당 Interface 에 해당하는 Type List를 반환
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllInterfaces<T>()
        {
            return Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(T).IsAssignableFrom(type));
        }

        /// <summary>
        /// 해당 class 에 해당하는 Type List를 반환
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllClasses<T>()
        {
            return Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)));
        }
    }
}