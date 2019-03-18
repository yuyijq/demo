using Arch.CoreInfo.Client;
using System;
using System.Collections.Generic;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    internal static class Expressions
    {
        private static readonly Dictionary<Type, Expr> cached = new Dictionary<Type, Expr>();

        internal static List<RawKey> Get<T>(List<T> input, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType, out Expr expr)
        {
            expr = Expressions.GetFor(typeof(T));
            List<RawKey> oneResult = new List<RawKey>();
            if (input.Count == 1)
            {
                expr.Get(input[0], oneResult, isEncrypt, genCustomKeyType);
                return oneResult;
            }

            //取出一个，然后算出一个list的大小来，避免List多次扩容
            expr.Get(input[0], oneResult, isEncrypt, genCustomKeyType);
            List<RawKey> result = new List<RawKey>(input.Count * oneResult.Count);
            result.AddRange(oneResult);
            for (int i = 1; i < input.Count; ++i)
            {
                expr.Get(input[i], result, isEncrypt, genCustomKeyType);
            }
            return result;
        }

        private static Expr GetFor(Type type)
        {
            lock (cached)
            {
                if (cached.TryGetValue(type, out Expr result))
                {
                    return result;
                }

                var expr = new RootExpr(type);
                cached.Add(type, expr);
                return expr;
            }
        }
    }
}
