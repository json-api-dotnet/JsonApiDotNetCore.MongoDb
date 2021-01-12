namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class ArticleTag
    {
        public string ArticleId { get; set; }
        public Article Article { get; set; }

        public string TagId { get; set; }
        public Tag Tag { get; set; }
    }
}