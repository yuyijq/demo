using Arch.CoreInfo.Client;
using System.Collections.Generic;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    interface Expr
    {
        void Get(object parent, List<RawKey> result, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType);

        void Set(object parent, IDictionary<KeyType, IDictionary<string, string>> data, bool isEncrypt, DataEntitySplitJointHelper.GenCustomKeyType genCustomKeyType);
    }
}
