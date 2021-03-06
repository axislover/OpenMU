﻿// <copyright file="ShowShopItemListPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.PlayerShop
{
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using MUnique.OpenMU.GameLogic;
    using MUnique.OpenMU.GameLogic.Views;
    using MUnique.OpenMU.GameLogic.Views.PlayerShop;
    using MUnique.OpenMU.Network;
    using MUnique.OpenMU.PlugIns;

    /// <summary>
    /// The default implementation of the <see cref="IShowShopItemListPlugIn"/> which is forwarding everything to the game client with specific data packets.
    /// </summary>
    [PlugIn("ShowShopItemListPlugIn", "The default implementation of the IShowShopItemListPlugIn which is forwarding everything to the game client with specific data packets.")]
    [Guid("14220c30-8a5e-458b-8fb5-f4c6bbc2c4a9")]
    public class ShowShopItemListPlugIn : IShowShopItemListPlugIn
    {
        private readonly RemotePlayer player;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowShopItemListPlugIn"/> class.
        /// </summary>
        /// <param name="player">The player.</param>
        public ShowShopItemListPlugIn(RemotePlayer player) => this.player = player;

        /// <inheritdoc/>
        /// <remarks>
        /// Maybe cache the result, because a lot of players could request the same list. However, this isn't critical.
        /// </remarks>
        public void ShowShopItemList(Player requestedPlayer)
        {
            const int maxCharacterNameLength = 10;
            const int maxStoreNameLength = 36;
            const int itemPriceSize = 4;
            var itemSerializer = this.player.ItemSerializer;
            var sizePerItem = itemSerializer.NeededSpace + itemPriceSize + 4; // don't know yet, where the additional 4 bytes come from

            var playerId = requestedPlayer.GetId(this.player);
            var itemlist = requestedPlayer.ShopStorage.Items.ToList();
            var itemcount = itemlist.Count;
            using (var writer = this.player.Connection.StartSafeWrite(0xC2, 11 + maxCharacterNameLength + maxStoreNameLength + (itemcount * sizePerItem)))
            {
                var packet = writer.Span;
                packet[3] = 0x3F;
                packet[4] = 0x05;
                packet[5] = 1;
                packet[6] = playerId.GetHighByte();
                packet[7] = playerId.GetLowByte();
                packet.Slice(8, maxCharacterNameLength).WriteString(this.player.SelectedCharacter.Name, Encoding.UTF8);
                var storeName = requestedPlayer.ShopStorage.StoreName;
                packet.Slice(18, maxStoreNameLength).WriteString(storeName, Encoding.UTF8);

                packet[8 + maxCharacterNameLength + maxStoreNameLength] = (byte)itemcount;
                int i = 0;

                foreach (var item in itemlist)
                {
                    var itemBlock = packet.Slice(9 + maxCharacterNameLength + maxStoreNameLength + (i * sizePerItem), sizePerItem);
                    itemBlock[0] = item.ItemSlot;
                    itemSerializer.SerializeItem(itemBlock.Slice(1), item);
                    itemBlock.Slice(1 + itemSerializer.NeededSpace).SetIntegerBigEndian((uint)(item.StorePrice ?? 0));
                }

                writer.Commit();
            }
        }
    }
}