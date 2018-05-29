using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Apis.Items.Details;
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
            var location = api.GetDetailManager<IDetailLocationManager>().Read(terminal.Character.Item);
            var ioLocationUpdate = new IOLocationUpdate()
            {
                DetailLocationId = location.Id,
                Name = location.Name,
                Description = location.Description
            };
            location.Item.Children.ForEach(delegate (Item item)
            {
                if (api.GetDetailManager<IDetailPhysicalManager>().IsManaging(item))
                {
                    ioLocationUpdate.Adendums.Add("Something is here");
                }
                if (api.GetDetailManager<IDetailCharacterManager>().IsManaging(item))
                {
                    ioLocationUpdate.Adendums.Add("Someone is here");
                }
            });
            // TODO : Get all Physical Detail items and enumerate.
            terminal.SendToClient(ioLocationUpdate);
        }
    }
}
