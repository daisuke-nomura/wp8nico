
namespace NicoLibrary.nomula
{
    public class NavigationParameter
    {
        public Types Type { get; set; }
        public object Parameter { get; set; }

        public enum Types
        {
            Page,//ページ名　使わないかも
            ID,//プレイヤーへ飛ばす
            Keyword,//使わないかも
            Tag,
            Link,//http://www.nicovideo.jp/watch/sm****
            RankingResult,//単一の動画の情報
            RankingResults,//複数動画の情報
            Mylist,
            Category,
            Watched,
            ShareWatched,
            WatchedNsen,
            BigCategoryFeed,
            PublicMylist,//公開マイリスト
            IDs,//動画IDが複数
            Uploaded,//ユーザーが投稿
            Rss,
        }
    }
}
