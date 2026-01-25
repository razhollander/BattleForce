using System;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Utils
{
    public static class ReflectionUtils
    {
        public static void CreateInstace(string fullTypeName, string assemblyName, params object[] args)
        {
            var installerType = Type.GetType(fullTypeName+", "+assemblyName);

            if (installerType != null)
            {
                Activator.CreateInstance(installerType, args: args);
            }
            else
            {
                LogService.LogError("No type found! " + fullTypeName);
            }
        }
    }
}