﻿// This file is part of HangFire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// HangFire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// HangFire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with HangFire. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HangFire.Common.Filters;
using HangFire.Storage;

namespace HangFire.Common
{
    /// <summary>
    /// Represents information about type and method that will be called during
    /// the performance of a job.
    /// </summary>
    /// 
    /// <remarks>
    /// Information about method that will be called consist of a 
    /// <see cref="MethodData.Type"/> and a <see cref="MethodInfo"/>.
    /// Although there is the <see cref="System.Reflection.MethodInfo.DeclaringType"/> property,
    /// this class allows you to set a class that contains the given method
    /// explicitly, enabling you to choose not only the base class, but one
    /// of its successors.
    /// </remarks>
    public class MethodData
    {
        internal MethodData(Type type, MethodInfo method)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (method == null) throw new ArgumentNullException("method");

            if (method.DeclaringType == null)
            {
                throw new NotSupportedException("Global methods are not supported. Use class methods instead.");
            }

            if (!method.DeclaringType.IsAssignableFrom(type))
            {
                throw new ArgumentException(String.Format(
                    "The type `{0}` must be derived from the `{1}` type.", 
                    method.DeclaringType, 
                    type));
            }

            Type = type;
            MethodInfo = method;
        }

        /// <summary>
        /// Gets an instance of <see cref="System.Type"/> class that contains 
        /// the given <see cref="MethodInfo"/>. It can be both the type that declares the
        /// method as well as its successor.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets an instance of the <see cref="System.Reflection.MethodInfo"/> class that points
        /// to the method that will be called during the performance of a job.
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }
        
        /// <summary>
        /// Gets wheither this instance contains the information in the
        /// Old Job Format.
        /// TODO: remove it before 1.0
        /// </summary>
        [Obsolete("This property will be removed before 1.0. Use the new version of the Client API.")]
        public bool OldFormat { get; private set; }

        public InvocationData Serialize()
        {
            return new InvocationData(
                Type.AssemblyQualifiedName,
                MethodInfo.Name,
                JobHelper.ToJson(MethodInfo.GetParameters().Select(x => x.ParameterType)));
        }

        public static MethodData Deserialize(InvocationData invocationData)
        {
            var oldFormat = false;

            // Normalize a job in the old format.
            if (invocationData.Method == null)
            {
                invocationData = new InvocationData(
                    invocationData.Type,
                    "Perform",
                    JobHelper.ToJson(new Type[0]));

                oldFormat = true;
            }

            try
            {
                var type = Type.GetType(invocationData.Type, throwOnError: true, ignoreCase: true);
                var parameterTypes = JobHelper.FromJson<Type[]>(invocationData.ParameterTypes);
                var method = type.GetMethod(invocationData.Method, parameterTypes);

                if (method == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "The type `{0}` does not contain a method with signature `{1}({2})`",
                        type.FullName,
                        invocationData.Method,
                        String.Join(", ", parameterTypes.Select(x => x.Name))));
                }

                return new MethodData(type, method) { OldFormat = oldFormat };
            }
            catch (Exception ex)
            {
                throw new JobLoadException("Could not load the job. See inner exception for the details.", ex);
            }
        }

        public static MethodData FromExpression(Expression<Action> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException("methodCall");

            var callExpression = methodCall.Body as MethodCallExpression;
            if (callExpression == null)
            {
                throw new ArgumentException("Expression body should be of type `MethodCallExpression`", "methodCall");
            }

            // Static methods can not be overridden in the derived classes, 
            // so we can take the method's declaring type.
            return new MethodData(callExpression.Method.DeclaringType, callExpression.Method);
        }

        public static MethodData FromExpression<T>(Expression<Action<T>> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException("methodCall");

            var callExpression = methodCall.Body as MethodCallExpression;
            if (callExpression == null)
            {
                throw new ArgumentException("Expression body should be of type `MethodCallExpression`", "methodCall");
            }

            return new MethodData(typeof(T), callExpression.Method);
        }

        internal IEnumerable<JobFilterAttribute> GetTypeFilterAttributes(bool useCache)
        {
            return useCache
                ? ReflectedAttributeCache.GetTypeFilterAttributes(Type)
                : GetFilterAttributes(Type);
        }

        internal IEnumerable<JobFilterAttribute> GetMethodFilterAttributes(bool useCache)
        {
            if (MethodInfo == null)
            {
                return Enumerable.Empty<JobFilterAttribute>();
            }

            return useCache
                ? ReflectedAttributeCache.GetMethodFilterAttributes(MethodInfo)
                : GetFilterAttributes(MethodInfo);
        }

        private IEnumerable<JobFilterAttribute> GetFilterAttributes(MemberInfo memberInfo)
        {
            return memberInfo
                .GetCustomAttributes(typeof(JobFilterAttribute), inherit: true)
                .Cast<JobFilterAttribute>();
        }
    }
}