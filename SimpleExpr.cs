using Arch.CoreInfo.Client;
using Freeway.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    class SimpleExpr : Expr
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SimpleExpr));

        private readonly Action<object, string> setter;

        private readonly Action<object, string> relativeSetter;

        private readonly Func<object, string> getter;

        private readonly InfoSecSensitiveField attribute;

        private readonly string fieldName;

        public SimpleExpr(Expression parent, FieldInfo fieldInfo, InfoSecSensitiveField attribute, FieldInfo relativeFieldInfo)
        {
            fieldName = fieldInfo.Name;
            this.attribute = attribute;
            var fr = Expression.Field(parent, fieldInfo);
            var valueParameter = Expression.Parameter(typeof(string));
            var parameters = new ParameterExpression[] { Constants.parameter, valueParameter };
            setter = Expression.Lambda<Action<object, string>>(Expression.Assign(fr, valueParameter), parameters).Compile();
            getter = Expression.Lambda<Func<object, string>>(fr, Constants.parameter).Compile();

            if (relativeFieldInfo != null)
            {
                var relativeFr = Expression.Field(parent, relativeFieldInfo);
                relativeSetter = Expression.Lambda<Action<object, string>>(Expression.Assign(relativeFr, valueParameter), parameters).Compile();
            }
            else
            {
                relativeSetter = null;
            }
        }

        public void Get(object parent, List<RawKey> result, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            //如果是加密，则类型必须是明文
            if (isEncrypt && !SensitiveFieldDataTypeEnum.PLAINTEXT.Equals(attribute.DataType)) return;

            //如果是解密，则类型必须是密文
            if (!isEncrypt && !SensitiveFieldDataTypeEnum.CIPHER.Equals(attribute.DataType)) return;

            var fieldVal = getter(parent);
            if (string.IsNullOrWhiteSpace(fieldVal)) return;

            if (!GetKeyType(parent, genCustomKeyType, out var keyType)) return;

            result.Add(new RawKey { Key = fieldVal, Type = keyType });
        }

        public void Set(object parent, IDictionary<KeyType, IDictionary<string, string>> processedData, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            if (isEncrypt && processedData == null)
            {
                Copy(parent, genCustomKeyType);
                return;
            }

            //如果是加密，并且字段是明文
            if (isEncrypt && SensitiveFieldDataTypeEnum.PLAINTEXT.Equals(attribute.DataType))
            {
                if (!GetKeyType(parent, genCustomKeyType, out var keyType)) return;

                string resultVal = null;
                var fieldVal = getter(parent);
                if (string.IsNullOrWhiteSpace(fieldVal)) return;

                if (processedData != null)
                {
                    processedData.TryGetValue(keyType, out var map);
                    if (map != null)
                    {
                        map.TryGetValue(fieldVal, out resultVal);
                    }
                }
                if (resultVal != null)
                {
                    setter(parent, resultVal);
                }
            }

            //如果是解密，并且字段是密文
            if (!isEncrypt && SensitiveFieldDataTypeEnum.CIPHER.Equals(attribute.DataType))
            {
                if (relativeSetter == null) return;

                if (!GetKeyType(parent, genCustomKeyType, out var keyType)) return;

                var resultVal = GetResultVal(parent, processedData, keyType);
                relativeSetter(parent, resultVal);
            }
        }

        private string GetResultVal(object parent, IDictionary<KeyType, IDictionary<string, string>> processedData, KeyType keyType)
        {
            var fieldVal = getter(parent);
            if (string.IsNullOrWhiteSpace(fieldVal)) return fieldVal;
            if (processedData == null) return fieldVal;
            processedData.TryGetValue(keyType, out var map);
            if (map == null) return fieldVal;
            map.TryGetValue(fieldVal, out var resultVal);
            if (resultVal == null) return fieldVal;
            return resultVal;
        }

        private void Copy(object parent, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType)
        {
            if (relativeSetter == null) return;
            var fieldVal = getter(parent);
            relativeSetter(parent, fieldVal);

        }

        private bool GetKeyType(object parent, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType, out KeyType keyType)
        {
            keyType = attribute.SecurityKeyType;
            if (attribute.NeedGenCustomKeyType)
            {
                keyType = genCustomKeyType(parent, fieldName);
            }
            return !default(KeyType).Equals(keyType);
        }
    }
}
