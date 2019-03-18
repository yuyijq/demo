using Arch.CoreInfo.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    class ListExpr : Expr
    {
        private readonly Func<object, int> lenFunc;

        private readonly Func<object, int, object> accessFunc;

        private Expr child;

        public ListExpr(Expression parentExpr, FieldInfo me)
        {
            var fieldExpr = Expression.Field(parentExpr, me);
            lenFunc = Expression.Lambda<Func<object, int>>(Expression.Property(fieldExpr, "Count"), Constants.parameter).Compile();
            accessFunc = Expression.Lambda<Func<object, int, object>>(Expression.Call(fieldExpr, "get_Item", null, Constants.indexExpr), new ParameterExpression[] { Constants.parameter, Constants.indexExpr }).Compile();

            var elementType = GetElementType(me.FieldType);
            if (elementType == null) return;
            child = new RootExpr(elementType);
        }

        /// <summary>
        /// 可能使用的是 AClass : List<T> 这种方式
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type GetElementType(Type type)
        {
            var args = type.GetGenericArguments();
            if (args.Length > 0) return args[0];

            var baseType = type.BaseType;
            args = baseType.GetGenericArguments();
            if (args.Length > 0) return args[0];
            return null;
        }

        public void Get(object parent, List<RawKey> result, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            if (child == null) return;
            int len = lenFunc(parent);
            for (int i = 0; i < len; ++i)
            {
                var elem = accessFunc(parent, i);
                if (elem == null) continue;
                child.Get(elem, result, isEncrypt, genCustomKeyType);
            }
        }

        public void Set(object parent, IDictionary<KeyType, IDictionary<string, string>> data, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            if (child == null) return;
            int len = lenFunc(parent);
            for (int i = 0; i < len; ++i)
            {
                var elem = accessFunc(parent, i);
                if (elem == null) continue;
                child.Set(elem, data, isEncrypt, genCustomKeyType);
            }
        }
    }
}
