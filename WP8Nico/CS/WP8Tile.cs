using Microsoft.Phone.Shell;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace WP8Nico.nomula
{
    public class WP8Tile
    {
        public static void SetNicorepoTile(ObservableCollection<Nicorepo> nicorepo)
        {
            ShellTile tile = ShellTile.ActiveTiles.First();
            IconicTileData tileData = null;
            ObservableCollection<Nicorepo> repo = null;

            if (nicorepo != null && nicorepo.Count > 0 && (repo = Nicorepo.ReadNewItems(nicorepo)) != null && repo.Count > 0)
            {
                tileData = new IconicTileData()
                {
                    Title = "ニコアプリ(仮)",
                    Count = repo.Count,
                    WideContent1 = "niconico",
                    WideContent2 = string.Format("{0} さんがお気に入り登録しました", repo[0].UserName),
                    WideContent3 = repo[0].VideoTitle,
                    SmallIconImage = new Uri("Assets/Tiles/IconicTileSmall.png", UriKind.Relative),
                    IconImage = new Uri("Assets/Tiles/IconicTileMediumLarge.png", UriKind.Relative),
                };
            }
            else
            {
                tileData = new IconicTileData()
                {
                    Title = null,
                    Count = 0,
                    WideContent1 = null,
                    WideContent2 = null,
                    WideContent3 = null,
                    SmallIconImage = new Uri("Assets/Tiles/IconicTileSmall.png", UriKind.Relative),
                    IconImage = new Uri("Assets/Tiles/IconicTileMediumLarge.png", UriKind.Relative),
                };
            }

            if (tile != null)
                tile.Update(tileData);
        }

    }
}
