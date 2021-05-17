using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

partial class MyCardModel
{
    private List<MyCard> _unitCards = null;

    public List<MyCard> unitCards
    {
        get
        {
            if (_unitCards==null)
            {
                _unitCards = list.Where(x => x.isPickable).ToList();//真就返回列表
            }
            return _unitCards;
        }
    }

   // /// <summary>
   // /// 按照卡牌id找卡牌数据
   // /// </summary>
   // /// <param name = "id" ></ param >
   // /// < returns ></ returns >
   //public MyCard FindById(int id)
   // {
   //     return list.Find((c) => (c.id == id));
   // }
}
