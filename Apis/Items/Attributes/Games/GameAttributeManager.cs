using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Games;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOurTime.Models.Items.Games;
using BeforeOurTime.Models.Items.Locations;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Games
{
    public class GameAttributeManager : AttributeManager<GameAttribute>, IGameAttributeManager
    {
        private IGameAttributeRepo DetailGameRepo { set; get; }
        private ILocationAttributeRepo DetailLocationRepo { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GameAttributeManager(
            IItemRepo itemRepo,
            IGameAttributeRepo detailGameRepo,
            ILocationAttributeRepo detailLocationRepo,
            IItemManager itemManager) : base(itemRepo, detailGameRepo)
        {
            DetailGameRepo = detailGameRepo;
            DetailLocationRepo = detailLocationRepo;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game and a location if one does not already exist
        /// </remarks>
        /// <returns></returns>
        public Item GetDefaultGame()
        {
            var defaultGameAttribute = DetailGameRepo.Read().FirstOrDefault();
            if (defaultGameAttribute == null)
            {
                var gameItem = ItemManager.Create(new GameItem()
                {
                    ParentId = null,
                    Attributes = new List<ItemAttribute>()
                    {
                        new GameAttribute()
                        {
                            Name = "Brave New World",
                            DefaultLocationId = null
                        }
                    }
                });
                var locationItem = ItemManager.Create(new LocationItem()
                {
                    ParentId = gameItem.Id,
                    Attributes = new List<ItemAttribute>()
                    {
                        new LocationAttribute()
                        {
                            Name = "A Dark Void",
                            Description = "Cool mists and dark shadows shroud "
                                + "everything in this place. Straining your eyes does little to resolve the "
                                + "amorphous blobs that are circulating about. The oppresive silence is occationaly "
                                + "puncuated by a distressed weeping or soft sob. A chill runs through your blood "
                                + "when you realise these forms may have once been human. The smell of rain "
                                + "and rotting wood pains your nose while the occational drip of water tickles "
                                + "the top of skulls both real and imagined. Any attempt to navigate in this damp "
                                + "cavern causes disorientation.",
                        }
                    }
                });
                defaultGameAttribute = gameItem.GetAttribute<GameAttribute>();
                defaultGameAttribute.DefaultLocationId = locationItem.GetAttribute<LocationAttribute>().Id;
                Update(defaultGameAttribute);
            }
            var defaultGameItem = ItemRepo.Read(
                defaultGameAttribute.ItemId, 
                new Models.TransactionOptions() { NoTracking = true });
            return defaultGameItem;
        }
        /// <summary>
        /// Get the default location of the default game
        /// </summary>
        /// <remarks>
        /// Default locations are used as the parent for item's whos parent is
        /// not specified
        /// </remarks>
        /// <returns></returns>
        public LocationAttribute GetDefaultLocation()
        {
            LocationAttribute locationAttribute = null;
            var game = GetDefaultGame();
            if (game.GetAttribute<GameAttribute>().DefaultLocationId != null)
            {
                locationAttribute = DetailLocationRepo.Read(
                    game.GetAttribute<GameAttribute>().DefaultLocationId.Value,
                    new Models.TransactionOptions() { NoTracking = true });
            }
            return locationAttribute;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (DetailGameRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update games's name
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="name">Game's new name</param>
        /// <returns></returns>
        public GameAttribute UpdateName(Guid id, string name)
        {
            var gameAttribute = Read(id);
            gameAttribute.Name = name;
            return Update(gameAttribute);
        }
        /// <summary>
        /// Update games's default location
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="locationId">Game's new default location</param>
        /// <returns></returns>
        public GameAttribute UpdateDefaultLocation(Guid id, Guid locationId)
        {
            var gameAttribute = Read(id);
            gameAttribute.DefaultLocationId = locationId;
            return Update(gameAttribute);
        }
    }
}
