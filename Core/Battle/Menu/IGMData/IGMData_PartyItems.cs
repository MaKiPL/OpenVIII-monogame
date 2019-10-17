using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public partial class VictoryMenu
        {
            #region Classes

            private class IGMData_PartyItems : IGMData.Base
            {
                #region Fields

                private readonly FF8String DialogSelectedItem;
                private readonly FF8String str_NotFound;
                private readonly FF8String str_Over100;
                private readonly FF8String str_Recieved;
                private Queue<KeyValuePair<Cards.ID, byte>> _cards;
                private KeyValuePair<Cards.ID, byte> card;

                #endregion Fields

                #region Methods

                protected override void Init()
                {
                    base.Init();
                    Hide();
                    ITEM[0, 0] = new IGMDataItem.Box{Data=Memory.Strings.Read(Strings.FileID.KERNEL, 30, 21)Pos=new Rectangle(SIZE[0].X, SIZE[0].Y, SIZE[0].Width, 78),Title=Icons.ID.INFO,Options=Box_Options.Middle};
                    ITEM[0, 1] = new IGMDataItem.Box{Pos=new Rectangle(SIZE[0].X + 140, SIZE[0].Y + 189, 475, 78),Title=Icons.ID.ITEM,Options=Box_Options.Middle}; // item name
                    ITEM[0, 2] = new IGMDataItem.Box{Pos=new Rectangle(SIZE[0].X + 615, SIZE[0].Y + 189, 125, 78),Title=Icons.ID.NUM_,Options=Box_Options.Middle | Box_Options.Center}; // item count
                    ITEM[0, 3] = new IGMDataItem.Box{Pos=new Rectangle(SIZE[0].X, SIZE[0].Y + 444, SIZE[0].Width, 78),Title=Icons.ID.HELP,Options=Box_Options.Middle}; // item description
                    ITEM[0, 4] = IGMData.Dialog.Small.Create(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center | Box_Options.Middle, SIZE[0]); // Couldn't find any items
                    ITEM[0, 5] = IGMData.Dialog.Small.Create(null, SIZE[0].X + 230, SIZE[0].Y + 291, Icons.ID.NOTICE, Box_Options.Center, SIZE[0]); // over 100 discarded
                    ITEM[0, 6] = IGMData.Dialog.Small.Create(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center, SIZE[0]); // Recieved item
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }

                #endregion Methods

                #region Constructors

                public IGMData_PartyItems(Menu_Base container) : base()
                {
                    str_NotFound = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 28);
                    str_Over100 = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 24);
                    str_Recieved = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 6);
                    DialogSelectedItem = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedItem };

                    Init(1, 7, container, 1, 1);
                }

                #endregion Constructors

                #region Properties

                public Queue<Saves.Item> Items { get; private set; }

                public Saves.Item Item { get; private set; }

                #endregion Properties

                public void Earn()
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(17);
                }

                public override bool Inputs_CANCEL() => false;

                public override bool Inputs_OKAY()
                {
                    if (ITEM[0, 5].Enabled || ITEM[0, 6].Enabled)
                    {
                        if (Items != null && Items.Count > 0 || _cards != null && _cards.Count > 0)
                        {
                            Refresh();
                            base.Inputs_OKAY();
                            return true;
                        }
                    }
                    else if (Items != null && Items.Count > 0)
                    {
                        if (Memory.State.EarnItem(Items.Dequeue()))
                        {
                            ITEM?[0, 6]?.Show();
                            Earn();
                        }
                        else
                        {
                            ITEM?[0, 5]?.Show();
                            Earn();
                        }

                        base.Inputs_OKAY();
                        return true;
                    }
                    else if (_cards != null && _cards.Count > 0)
                    {
                        if (Memory.State.EarnItem(_cards.Dequeue()))
                        {
                            ITEM?[0, 6]?.Show();
                            Earn();
                        }
                        else
                        {
                            ITEM?[0, 5]?.Show();
                            Earn();
                        }

                        base.Inputs_OKAY();
                        return true;
                    }
                    return false;
                }

                public override void Refresh()
                {
                    base.Refresh();
                    if (Items != null && Items.Count > 0)
                    {
                        Item = Items.Peek();
                        ((IGMDataItem.Box)ITEM[0, 1]).Data = Item.DATA?.Name;
                        ((IGMDataItem.Box)ITEM[0, 2]).Data = $"{Item.QTY}";
                        ((IGMDataItem.Box)ITEM[0, 3]).Data = Item.DATA?.Description;
                        ((IGMData.Dialog.Small)ITEM[0, 5]).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ((IGMData.Dialog.Small)ITEM[0, 5]).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ITEM[0, 1].Show();
                        ITEM[0, 2].Show();
                        ITEM[0, 3].Show();
                        ITEM[0, 4].Hide();
                        ITEM[0, 5].Hide();
                        ITEM[0, 6].Hide();
                    }
                    else
                    if (_cards != null && _cards.Count > 0)
                    {
                        card = _cards.Peek();
                        var name = Memory.Strings.Read(Strings.FileID.MNGRP, 110, (int)card.Key);

                        int pos = 0;
                        for (; pos < name.Length; pos++)
                            if (name.Value[pos] == 2) break;
                        ((IGMDataItem.Box)ITEM[0, 1]).Data = new FF8String(name.Value.Take(pos-1).ToArray());
                        //TODO grab card name from start of string
                        ((IGMDataItem.Box)ITEM[0, 2]).Data = $"{card.Value}";
                        ((IGMDataItem.Box)ITEM[0, 3]).Data = "";
                        ((IGMData.Dialog.Small)ITEM[0, 5]).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ((IGMData.Dialog.Small)ITEM[0, 5]).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ITEM[0, 1].Show();
                        ITEM[0, 2].Show();
                        ITEM[0, 3].Hide();
                        ITEM[0, 4].Hide();
                        ITEM[0, 5].Hide();
                        ITEM[0, 6].Hide();
                    }
                    else
                    {
                        ITEM?[0, 1]?.Hide();
                        ITEM?[0, 2]?.Hide();
                        ITEM?[0, 3]?.Hide();
                        ITEM?[0, 4]?.Show();
                        ITEM?[0, 5]?.Hide();
                        ITEM?[0, 6]?.Hide();
                    }
                }

                public void SetItems(ConcurrentDictionary<Cards.ID, byte> cards)
                {
                    if (cards.Count > 0)
                    {
                        _cards = new Queue<KeyValuePair<Cards.ID, byte>>(cards.Count);
                        foreach (KeyValuePair<Cards.ID, byte> e in cards)
                            _cards.Enqueue(e);
                    }
                    else _cards = null;
                }

                public void SetItems(ConcurrentDictionary<byte, byte> items)
                {
                    if (items.Count > 0)
                    {
                        Items = new Queue<Saves.Item>(items.Count);
                        foreach (KeyValuePair<byte, byte> e in items)
                            Items.Enqueue(new Saves.Item(e));
                    }
                    else Items = null;
                }
            }

            #endregion Classes
        }

        #endregion Classes
    }
}