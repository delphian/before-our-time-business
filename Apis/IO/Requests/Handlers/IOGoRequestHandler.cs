using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class IOGoRequestHandler : IIORequestHandler
    {
        public IOGoRequestHandler()
        {
        }
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalInput)
        {
            if (terminalInput.GetType() == typeof(IOGoRequest))
            {
                var ioGoRequest = (IOGoRequest) Convert.ChangeType(terminalInput, typeof(IOGoRequest));
                var locationExit = api.GetAttributeManager<IAttributeExitManager>().Read(ioGoRequest.ExitId);
                var location = api.GetAttributeManager<IAttributeLocationManager>().Read(locationExit.DestinationLocationId);
                var player = api.GetAttributeManager<IAttributePlayerManager>().Read(terminal.PlayerId);
                api.GetItemManager().Move(player.Item, location.Item, locationExit.Item);
                var ioLocationUpdate = new IOLocationUpdate()
                {
                    DetailLocationId = location.Id,
                    Name = location.Name,
                    Description = location.Description
                };
                location.Item.Children.ForEach(delegate (Item item)
                {
                    if (api.GetAttributeManager<IAttributePhysicalManager>().IsManaging(item))
                    {
                        ioLocationUpdate.Adendums.Add("Something is here");
                    }
                    if (api.GetAttributeManager<IAttributePlayerManager>().IsManaging(item))
                    {
                        ioLocationUpdate.Adendums.Add("Someone is here");
                    }
                    if (api.GetAttributeManager<IAttributeExitManager>().IsManaging(item))
                    {
                        var exit = api.GetAttributeManager<IAttributeExitManager>().Read(item);
                        ioLocationUpdate.Exits.Add(new IOExitUpdate()
                        {
                            ExitId = exit.Id,
                            Name = exit.Name,
                            Description = exit.Description
                        });
                        ioLocationUpdate.Adendums.Add("Exit: " + exit.Name);
                    }
                });
                terminal.SendToClient(ioLocationUpdate);
            }
        }
    }
}
