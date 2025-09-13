namespace Gastos.Shared.Api;

public static class GastosApiEndpoints
{
    public const string ApiBase = "/api";

    public static class Products
    {
        private const string _base = $"{ApiBase}/products";
        public const string Create = _base;
        public const string Get = $"{_base}/{{id}}";
        public const string GetAll = _base;
        public const string Update = _base;
        public const string Delete = $"{_base}/{{id}}";
    }

    public static class Stores
    {
        private const string _base = $"{ApiBase}/stores";
        public const string Create = _base;
        public const string Get = $"{_base}/{{id}}";
        public const string GetAll = _base;
        public const string Update = _base;
        public const string Delete = $"{_base}/{{id}}";
    }

    public static class Receipts
    {
        private const string _base = $"{ApiBase}/receipts";
        public const string Create = _base;
        public const string Get = $"{_base}/{{id}}";
        public const string GetAll = _base;
        public const string Update = _base;
        public const string Delete = $"{_base}/{{id}}";
        public const string ExistsBySourceId = $"{_base}/exists-sourceid/{{sourceId}}/{{receiptIdToExclude}}";
        public const string ExistsByStoreIdAndDate = $"{_base}/exists-storeid-date/{{storeId}}/{{transactionDateUtc}}/{{receiptIdToExclude}}";
        public const string ExistsByStoreSourceNameAndDate = $"{_base}/exists-storesourcename-date/{{storeSourceName}}/{{transactionDateUtc}}";
        public const string GetProductIdBySourceDescription = $"{_base}/same-description/{{description}}";
    }

    public static class Sizings
    {
        private const string _base = $"{ApiBase}/sizings";
        public const string Get = $"{_base}/{{id}}";
        public const string GetAll = _base;
    }

    public static class Stats
    {
        private const string _base = $"{ApiBase}/stats";
        public const string GetAll = _base;
    }
}
