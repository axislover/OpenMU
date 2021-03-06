﻿// <copyright file="PlayerShopClosedPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.PlayerShop
{
    using System.Runtime.InteropServices;
    using MUnique.OpenMU.GameLogic;
    using MUnique.OpenMU.GameLogic.Views;
    using MUnique.OpenMU.GameLogic.Views.PlayerShop;
    using MUnique.OpenMU.Network;
    using MUnique.OpenMU.PlugIns;

    /// <summary>
    /// The default implementation of the <see cref="IPlayerShopClosedPlugIn"/> which is forwarding everything to the game client with specific data packets.
    /// </summary>
    [PlugIn("PlayerShopClosedPlugIn", "The default implementation of the IPlayerShopClosedPlugIn which is forwarding everything to the game client with specific data packets.")]
    [Guid("351cf726-5091-4995-9228-81c089d1da16")]
    public class PlayerShopClosedPlugIn : IPlayerShopClosedPlugIn
    {
        private readonly RemotePlayer player;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerShopClosedPlugIn"/> class.
        /// </summary>
        /// <param name="player">The player.</param>
        public PlayerShopClosedPlugIn(RemotePlayer player) => this.player = player;

        /// <inheritdoc/>
        public void PlayerShopClosed(Player playerWithClosedShop)
        {
            var playerId = playerWithClosedShop.GetId(this.player);
            using (var writer = this.player.Connection.StartSafeWrite(0xC1, 0x07))
            {
                var packet = writer.Span;
                packet[2] = 0x3F;
                packet[3] = 3;
                packet[4] = 1;
                packet.Slice(5).SetShortSmallEndian(playerId);
                writer.Commit();
            }
        }
    }
}