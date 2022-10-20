using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeGiftDataManager
{
    public class GiftBoxDataList
    {
        public List<GiftBoxData>? boxDataList { get; set; }
        public object? isAllDataCache { get; set; }
    }

    public class GiftBoxData
    {
        public object? gfMId { get; set; }              //_giftId
        public object? gtp { get; set; }                //_giftType
        public object? con { get; set; }                //_content
        public object? iconT { get; set; }              //_iconType
        public object? amo { get; set; }                //_amount
        public object? oSt { get; set; }                //_status
        public object? title { get; set; }              //_titleMsIdDicDic
        public object? txt { get; set; }                //_textMsIdDicDic
        public object? expAt { get; set; }              //_expiredUnixTime
        public object? creAt { get; set; }              //_createdUnixTime
        public object? updAt { get; set; }              //_updatedUnixTime
        public object? viAt { get; set; }               //_viewUnixTime
        public GiftBoxParameter? pm { get; set; }       //_pokemonInfos
        public object? riGrp { get; set; }              //_recieveTitleGroup
        public object? txtTg { get; set; }              //_textTg
        public object? titTg { get; set; }              //_titleTg
    }

    public class GiftBoxParameter
    {
        public List<GiftBoxPokemonInfo>? pkInfo { get; set; }
    }

    public class GiftBoxPokemonInfo
    {
        public object? id { get; set; }
        public object? form { get; set; }
        public object? sex { get; set; }
        public object? color { get; set; }
        public object? nicNg { get; set; }
    }
}