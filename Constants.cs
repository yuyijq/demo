using System.Linq.Expressions;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    static class Constants
    {
        internal static readonly ParameterExpression parameter = Expression.Parameter(typeof(object));

        internal static readonly ParameterExpression indexExpr = Expression.Parameter(typeof(int), "index");
    }
}
