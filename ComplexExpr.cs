using Arch.CoreInfo.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    class ComplexExpr : Expr
    {
        private readonly List<Expr> children;

        private readonly Func<object, object> meExpr;

        public ComplexExpr(Expression parentExpr, FieldInfo me, Type t)
        {
            meExpr = me == null ? null : Expression.Lambda<Func<object, object>>(Expression.Field(parentExpr, me), Constants.parameter).Compile();

            children = new List<Expr>();
            var type = me == null ? t : me.FieldType;
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var expr = Expression.Convert(Constants.parameter, type);
            for (int i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                var attributes = field.GetCustomAttributes(typeof(InfoSecSensitiveField), false);
                if (attributes == null || attributes.Length == 0) continue;

                var attribute = (InfoSecSensitiveField)attributes[0];
                var sensitiveFieldType = attribute.SensitiveFieldType;
                if (sensitiveFieldType == SensitiveFieldTypeEnum.NATIVE)
                {
                    children.Add(new SimpleExpr(expr, field, attribute, GetRelativeField(type, attribute)));
                }
                else if (sensitiveFieldType == SensitiveFieldTypeEnum.COLLECTIONS)
                {
                    var fieldType = field.FieldType;
                    children.Add(new ListExpr(expr, field));
                }
                else if (sensitiveFieldType == SensitiveFieldTypeEnum.COMPLEX)
                {
                    children.Add(new ComplexExpr(expr, field, null));
                }
            }
        }

        private FieldInfo GetRelativeField(Type type, InfoSecSensitiveField attribute)
        {
            var relativeFieldName = attribute.RelativeFieldName;
            if (string.IsNullOrWhiteSpace(relativeFieldName)) return null;
            return type.GetField(relativeFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Get(object parent, List<RawKey> result, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            var meValue = meExpr == null ? parent : meExpr(parent);
            if (meValue == null) return;
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Get(meValue, result, isEncrypt, genCustomKeyType);
            }
        }

        public void Set(object parent, IDictionary<KeyType, IDictionary<string, string>> data, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            var meValue = meExpr == null ? parent : meExpr(parent);
            if (meValue == null) return;
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Set(meValue, data, isEncrypt, genCustomKeyType);
            }
        }
    }
}
