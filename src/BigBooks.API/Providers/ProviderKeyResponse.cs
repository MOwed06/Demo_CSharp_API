namespace BigBooks.API.Providers
{
    public class ProviderKeyResponse(int? key, string error)
    {
        public int? Key { get; set; } = key;
        public string Error { get; set; } = error;
    }
}
