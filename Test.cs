public void Decrypt(List<TDecryptType> inputData, int exceptionBehavior)
{
    List<RawKey> keys = Expressions.Get<TDecryptType>(inputData, false, GenCustomKeyType, out var expr);
    List<List<RawKey>> listOfBatch = Util.Split(keys, MAX_INTERFACE_ITEM_COUNT);
    IDictionary<KeyType, IDictionary<string, string>> decryptData = new Dictionary<KeyType, IDictionary<string, string>>();
    foreach (var batch in listOfBatch)
    {
        var result = InfoSecurityHelper.BatchDecrypt(batch);
        if (result == null || result.Data == null)
        {
           return;
        }

        Util.Merge(decryptData, result.Data);
    }

    foreach (var item in inputData)
    {
        expr.Set(item, decryptData, false, GenCustomKeyType);
    }
}