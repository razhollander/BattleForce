using System;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Utils
{
    public static class ReflectionUtils
    {
        public static object CreateInstace(string fullTypeName, string assemblyName, params object[] args)
        {
            var installerType = Type.GetType(fullTypeName + ", " + assemblyName);

            if (installerType != null)
            {
                return Activator.CreateInstance(installerType, args: args);
            }

            LogService.LogError("No type found! " + fullTypeName);
            throw new Exception();
        }
    }
}