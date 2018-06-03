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
    public class IOLookRequestHandler : IIORequestHandler
    {
        public IOLookRequestHandler()
        {
        }
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalInput)
        {
            if (terminalInput.GetType() == typeof(IOLookRequest))
            {
                var player = api.GetAttributeManager<IAttributePlayerManager>().Read(terminal.PlayerId);
                var location = api.GetAttributeManager<IAttributeLocationManager>().Read(player.Item);
                var ioLocationUpdate = new IOLocationUpdate()
                {
                    DetailLocationId = location.Id,
                    Name = location.Name,
                    Description = location.Description,
                    Exits = new List<IOExitUpdate>()
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
                    }
                });
                terminal.SendToClient(ioLocationUpdate);
            }
        }
    }
}
